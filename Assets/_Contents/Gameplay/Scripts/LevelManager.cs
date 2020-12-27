using System.Threading;
using Cysharp.Threading.Tasks;
using Pyra.ApplicationStateManagement;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private ApplicationStateVariable _applicationState;
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private IntVariable _activeLevel;
        [SerializeField] private GameEvent _actionEvent;
        [SerializeField] private GameEvent _restartLevelEvent;
        [SerializeField] private GameEvent _nextLevelEvent;
        [SerializeField] private IntEvent _setLevelEvent;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            WaitForRestartEvent(token).Forget();
            WaitForNextLevelEvent(token).Forget();
            WaitForSetLevelEvent(token).Forget();
            
            _actionEvent.Subscribe(OnAction, token);
        }

        private void OnAction()
        {
            if (_gameplayState.Value == GameplayStateEnum.Win)
                NextLevel();
            else if (_gameplayState.Value == GameplayStateEnum.Lose)
                LoadCurrentLevel();
        }

        private async UniTaskVoid WaitForRestartEvent(CancellationToken token)
        {
            if (!await _restartLevelEvent.WaitForEvent(token).SuppressCancellationThrow())
                LoadCurrentLevel();
        }

        private async UniTaskVoid WaitForNextLevelEvent(CancellationToken token)
        {
            if (!await _nextLevelEvent.WaitForEvent(token).SuppressCancellationThrow())
                NextLevel();
        }
        
        private async UniTaskVoid WaitForSetLevelEvent(CancellationToken token)
        {
            var (canceled, level) = await _setLevelEvent.WaitForEvent(token).SuppressCancellationThrow();
            if (!canceled)
                LoadLevel(level);
        }

        private void LoadCurrentLevel()
        {
            _gameplayState.Value = GameplayStateEnum.Setup;
            _applicationState.Value = ApplicationStateEnum.GamePlay;
        }

        private void NextLevel() => LoadLevel(_activeLevel.Value + 1);

        private void LoadLevel(int level)
        {
            _activeLevel.Value = level;
            LoadCurrentLevel();
        }
    }
}