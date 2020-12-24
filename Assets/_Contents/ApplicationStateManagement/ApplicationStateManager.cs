using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.VariableSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pyra.ApplicationStateManagement
{
    public class ApplicationStateManager : MonoBehaviour
    {
        [SerializeField] private FloatVariable _loadingProgress;
        [SerializeField] private ApplicationStateVariable _applicationState;
        [SerializeField] private List<ApplicationStateConfig> _stateConfigs = new List<ApplicationStateConfig>();

        // cache
        private readonly Dictionary<ApplicationStateEnum, ApplicationStateConfig> _cachedConfigs = new Dictionary<ApplicationStateEnum, ApplicationStateConfig>();
        private ApplicationStateConfig ActiveState => _cachedConfigs[_applicationState];
        private ApplicationStateConfig PrevState { get; set; }
        
        private void Awake()
        {
            _stateConfigs.ForEach(config =>
            {
                if (!_cachedConfigs.ContainsKey(config.applicationState))
                    _cachedConfigs.Add(config.applicationState, config);
                else
                    UnityEngine.Debug.LogWarning($"[{GetType().Name}] Duplicated config for {config.applicationState}. Config already exist!");
            });
        }


        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _applicationState.ForEachAwaitWithCancellationAsync(async (value, ct) => await OnApplicationStateChanged(value, ct), token).Forget();
        }

        private async UniTask OnApplicationStateChanged(ApplicationStateEnum newApplicationState, CancellationToken token)
        {
            // necessary to let _applicationState value to be correctly assigned
            await UniTask.NextFrame(cancellationToken: token);

            // reset progress
            _loadingProgress.Value = 0;

            // unload/load scenes
            await UnloadPreviousState(token);
            await LoadCurrentState(token);

            await UniTask.NextFrame(cancellationToken: token);

            GC.Collect();

            await UniTask.NextFrame(cancellationToken: token);

            Shader.WarmupAllShaders();

            await UniTask.NextFrame(cancellationToken: token);

            // wait for load buffer time
            await ProcessOffset(token);

            SetActiveScene();

            PrevState = _cachedConfigs[newApplicationState];
        }

        private IList<SceneNamesEnum> FilterScenesToUnload()
        {
            var toUnload = new List<SceneNamesEnum>();

            if (PrevState != null)
            {
                foreach (var scene in PrevState.scenes)
                    if (!ActiveState.scenesToKeep.Contains(scene)
                        && SceneManager.GetSceneByName(scene.ToString()).isLoaded)
                        toUnload.Add(scene);
            }

            return toUnload;
        }

        private IList<SceneNamesEnum> FilterScenesToLoad()
        {
            var toLoad = new List<SceneNamesEnum>();

            foreach (var scene in ActiveState.scenes)
                if (!SceneManager.GetSceneByName(scene.ToString()).isLoaded)
                    toLoad.Add(scene);

            return toLoad;
        }

        private async UniTask UnloadPreviousState(CancellationToken token)
        {
            var unloadPercentage = 0.45f; // 0.45 for unload, 0.45 for load, 0.1 for buffer

            var scenes = FilterScenesToUnload();

            if (scenes.Count <= 0)
            {
                Debug.Log($"[{GetType().Name}] No scene to unload..");
                await UniTask.NextFrame(cancellationToken: token);
                _loadingProgress.Value += unloadPercentage;
                return;
            }

            var sceneCountInv = unloadPercentage / scenes.Count;

            foreach (var scene in scenes)
            {
                var progress = new Progress<float>(value =>
                    _loadingProgress.Value = Mathf.Min(0.9f, _loadingProgress.Value + value * sceneCountInv));
                await SceneManager.UnloadSceneAsync(scene.ToString())
                    .ToUniTask(progress, cancellationToken: token);
            }
        }

        private async UniTask LoadCurrentState(CancellationToken token)
        {
            var loadPercentage = 0.45f;

            var scenes = FilterScenesToLoad();

            if (scenes.Count <= 0)
            {
                await UniTask.Yield();
                _loadingProgress.Value += loadPercentage;
                return;
            }

            var sceneCountInv = loadPercentage / scenes.Count;

            foreach (var scene in scenes)
            {
                Debug.Log($"[{GetType().Name}] loading scene {scene}");
                var progress = new Progress<float>(value =>
                    _loadingProgress.Value = Mathf.Min(0.9f, _loadingProgress.Value + value * sceneCountInv));
                await SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Additive)
                    .ToUniTask(progress, cancellationToken: token);
            }
        }

        private void SetActiveScene()
        {
            var sceneName = ActiveState.activeScene.ToString();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        private async UniTask ProcessOffset(CancellationToken token)
        {
            // hold loading progress as long as loadBufferTime. -> loading screen will stay visible.
            var bufferMultiplier = 0.1f / Mathf.Max(0.1f, ActiveState.loadBufferTime);
            while (_loadingProgress.Value < 1)
            {
                await UniTask.NextFrame(cancellationToken: token);
                var delta = Time.deltaTime * bufferMultiplier;
                _loadingProgress.Value += delta;
            }

            _loadingProgress.Value = 1f;
        }
    }
}