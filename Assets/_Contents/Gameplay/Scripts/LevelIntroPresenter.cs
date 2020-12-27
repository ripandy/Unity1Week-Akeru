using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Contents.Gameplay.Scripts
{
    public class LevelIntroPresenter : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private GameEvent _actionEvent;
        [SerializeField] private Button _toNextLevelButton;
        [SerializeField] private TMP_Text _introText;
        [SerializeField] private FloatVariable _fadeDuration;
        [SerializeField] private float _duration = 2.5f;

        private CancellationTokenSource _cts;

        private void Start()
        {
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(_duration + _fadeDuration * 2));
            
            _toNextLevelButton.OnClickAsAsyncEnumerable()
                .Subscribe(_ => IntroDone(), _cts.Token);
            
            _actionEvent.AsUniTaskAsyncEnumerable()
                .Where(_ => _gameplayState.Value == GameplayStateEnum.Setup)
                .Subscribe(_ => IntroDone(), _cts.Token);

            AnimateIntro(_cts.Token).Forget();
        }

        private async UniTaskVoid AnimateIntro(CancellationToken token)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_fadeDuration), cancellationToken: token);
            
            var inOutDuration = _duration * 0.2f;
            var standbyDuration = _duration - inOutDuration * 2;
            
            await DOTween.Sequence()
                .Append(DOTween.ToAlpha(() => _introText.color, value => _introText.color = value, 1, inOutDuration).From(0))
                .AppendInterval(standbyDuration)
                .Append(DOTween.ToAlpha(() => _introText.color, value => _introText.color = value, 0, inOutDuration).From(1))
                .ToUniTask(cancellationToken: token);
            
            IntroDone();
        }

        private void IntroDone()
        {
            try
            {
                _gameplayState.Value = GameplayStateEnum.Drop;
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}