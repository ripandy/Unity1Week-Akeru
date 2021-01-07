using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Pyra.VariableSystem;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLindbergh.VirtualEvent.SceneManagement
{
    public class FadeHandler : MonoBehaviour
    {
        [SerializeField] private FloatVariable _fadeDuration;
        [SerializeField] private BoolVariable _isLoading;
        [SerializeField] private BoolVariable _shouldFade;
        [SerializeField] private Image _image;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _isLoading.Subscribe(OnLoadingStateChanged, token);
        }

        private void OnLoadingStateChanged(bool isLoading)
        {
            if (isLoading)
                LoadStarted();
            else
                LoadCompleted();
        }

        private void LoadStarted()
        {
            if (_shouldFade)
                FadeOut(_fadeDuration);
        }

        private void LoadCompleted()
        {
            if (_shouldFade)
                FadeIn(_fadeDuration);
        }

        private async UniTaskVoid StartFade(float duration, bool fadeIn)
        {
            var token = this.GetCancellationTokenOnDestroy();
            var fadeTo = fadeIn ? 0f : 1f;
            var fadeFrom = fadeIn ? 1f : 0f;
            
            if (!fadeIn)
                _image.gameObject.SetActive(true);
            
            await DOTween.ToAlpha(() => _image.color, value => _image.color = value, fadeTo, duration).From(fadeFrom)
                .ToUniTask(cancellationToken: token);
            
            if (fadeIn)
                _image.gameObject.SetActive(false);
        }

        private void FadeIn(float duration) => StartFade(duration, true).Forget();
        private void FadeOut(float duration) => StartFade(duration, false).Forget();
    }
}