using Cysharp.Threading.Tasks;
using DG.Tweening;
using Pyra.ApplicationStateManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyra.LoadingScreen
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private ApplicationStateVariable applicationState;
        [SerializeField] private TMP_Text _splashText;
        [SerializeField] private Image _splashImage;
        [SerializeField] private Sprite _splashSprite;

        private void Start() => Initialize().Forget();

        private async UniTaskVoid Initialize()
        {
            var token = this.GetCancellationTokenOnDestroy();

            await DOTween.Sequence()
                .Append(DOTween.ToAlpha(() => _splashText.color, value => _splashText.color = value, 1, 3f).From(0))
                .AppendInterval(3f)
                .Append(DOTween.ToAlpha(() => _splashText.color, value => _splashText.color = value, 0, 3f))
                .ToUniTask(cancellationToken: token);
            
            applicationState.Value = ApplicationStateEnum.MainMenu;
        }
    }
}
