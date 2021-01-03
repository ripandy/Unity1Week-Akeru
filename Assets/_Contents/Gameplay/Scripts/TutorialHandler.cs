using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using TMPro;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class TutorialHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private IntVariable _activeLevel;
        [SerializeField] private CubeSideCollection _cubeSide;
        [SerializeField] private TMP_Text[] _basicTutorialTexts;
        [SerializeField] private TMP_Text[] _stepTexts;
        [SerializeField] private TMP_Text[] _helpTexts;
        [SerializeField] private float _helpIdleTime = 2f;
        [SerializeField] private GameEventCollection _cancelHelpEvents;

        private int _step;
        private int _helpState; // 0 = hidden, 1 = animate, 2 = shown
        private bool _cancelHelpFlag;

        private void Start()
        {
            HideHelp();
            
            var token = this.GetCancellationTokenOnDestroy();

            if (_activeLevel == 0)
            {
                _gameplayState.SubscribeAwait(ShowTutorial, token);
            }
            else
            {
                StandbyShowHelp(token).Forget();
                _cancelHelpEvents.SubscribeToAny(() => _cancelHelpFlag = true, token);
            }
        }

        private async UniTaskVoid StandbyShowHelp(CancellationToken destroyToken)
        {
            while (!destroyToken.IsCancellationRequested)
            {
                await UniTask.WaitUntil(() =>
                    _gameplayState.Value == GameplayStateEnum.Drop || _gameplayState.Value == GameplayStateEnum.Fill, cancellationToken: destroyToken);
                
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_helpIdleTime)))
                {
                    await UniTask.WaitUntil(() => _cancelHelpFlag, cancellationToken: cts.Token)
                        .SuppressCancellationThrow();

                    ShowHelpAsync(!_cancelHelpFlag, destroyToken).SuppressCancellationThrow();
                    _cancelHelpFlag = false;
                }
            }
        }

        private void ShowHelp(bool show)
        {
            var to = show ? 1f : 0f;
            var duration = 0.5f;
            var delay = show ? 0.5f : 0f;
            for (var i = 0; i < _helpTexts.Length; i++)
            {
                if (i < 5
                    || i == 5 && _gameplayState.Value == GameplayStateEnum.Drop
                    || i > 5 && _gameplayState.Value == GameplayStateEnum.Fill)
                {
                    var text = _helpTexts[i];
                    var tween = DOTween.ToAlpha(() => text.color, value => text.color = value, to, duration)
                        .SetDelay(delay);
                    if (show) tween.From(0);
                }
            }
        }
        
        private async UniTask ShowHelpAsync(bool show, CancellationToken token)
        {
            if (_helpState == 1
                || _helpState == 0 && !show
                || _helpState == 2 && show)
                return;
            
            _helpState = 1;
            var to = show ? 1f : 0f;
            var duration = 0.5f;
            var delay = show ? 0.5f : 0f;
            for (var i = 0; i < _helpTexts.Length; i++)
            {
                if (i < 5
                    || i == 5 && _gameplayState.Value == GameplayStateEnum.Drop
                    || i > 5 && _gameplayState.Value == GameplayStateEnum.Fill)
                {
                    var text = _helpTexts[i];
                    DOTween.ToAlpha(() => text.color, value => text.color = value, to, duration)
                        .SetDelay(delay);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            _helpState = show ? 2 : 0;
        }

        private void HideHelp()
        {
            foreach (var text in _helpTexts)
            {
                var c = text.color;
                    c.a = 0;
                text.color = c;
            }
        }

        private async UniTaskVoid ActivateBasicTutorial(CancellationToken token)
        {
            var duration = 0.5f;
            foreach (var text in _basicTutorialTexts)
            {
                DOTween.ToAlpha(() => text.color, value => text.color = value, 1f, duration).From(0f);
            }

            await UniTask.WaitUntil(() => _activeLevel.Value != 0, cancellationToken: token);
            
            foreach (var text in _basicTutorialTexts)
            {
                DOTween.ToAlpha(() => text.color, value => text.color = value, 0f, duration);
            }
        }

        private async UniTask ShowTutorial(GameplayStateEnum gameState, CancellationToken token)
        {
            if (gameState == GameplayStateEnum.Drop)
            {
                ActivateBasicTutorial(token).Forget();
                return;
            }
            
            if (gameState == GameplayStateEnum.Fill)
            {
                await NextStep(token);
                return;
            }
            
            HideTutorial();
        }

        private async UniTask NextStep(CancellationToken token)
        {
            if (_step == 0)
            {
                var duration = 0.5f;
                await DOTween.ToAlpha(() => _stepTexts[_step].color, value => _stepTexts[_step].color = value, 0f, duration).ToUniTask(cancellationToken: token);
                _step++;
                await DOTween.ToAlpha(() => _stepTexts[_step].color, value => _stepTexts[_step].color = value, 1f, duration)
                    .From(0f).ToUniTask(cancellationToken: token);
                await UniTask.WaitUntil(() => _cubeSide.IntactCount <= 2, cancellationToken: token);
                _step++;
                await DOTween.ToAlpha(() => _stepTexts[_step].color, value => _stepTexts[_step].color = value, 1f, duration)
                    .From(0f).ToUniTask(cancellationToken: token);
            }
        }

        private void HideTutorial()
        {
            var duration = 0.5f;
            foreach (var text in _stepTexts)
            {
                DOTween.ToAlpha(() => text.color, value => text.color = value, 0f, duration);
            }
            ShowHelp(false);
        }
    }
}