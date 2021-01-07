using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pyra.ApplicationStateManagement
{
    public class Boot : MonoBehaviour
    {
        private void Start() => Initialize().Forget();

        private async UniTaskVoid Initialize()
        {
            var token = this.GetCancellationTokenOnDestroy();
            
            await UniTask.WhenAll(
                SceneManager.LoadSceneAsync(SceneNamesEnumCore.Statics.ToString(), LoadSceneMode.Additive).ToUniTask(cancellationToken: token),
                SceneManager.LoadSceneAsync(SceneNamesEnumCore.SplashScreen.ToString(), LoadSceneMode.Additive).ToUniTask(cancellationToken: token));
            
            await UniTask.NextFrame(cancellationToken: token);

            SceneManager.UnloadSceneAsync(SceneNamesEnumCore.Boot.ToString());
        }
    }
}
