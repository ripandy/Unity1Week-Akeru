using Cysharp.Threading.Tasks;
using DG.Tweening;
using Pyra.ApplicationStateManagement;
using Pyra.EventSystem;
using TMPro;
using UnityEngine;

namespace Pyra.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private ApplicationStateVariable applicationState;
        [SerializeField] private GameEvent gameStartEvent;
        [SerializeField] private TMP_Text _titleText;

        private void Start()
        {
            gameStartEvent
                .Subscribe(() => applicationState.Value = ApplicationStateEnum.GamePlay)
                .AddTo(this.GetCancellationTokenOnDestroy());

            DOTween.Sequence()
                .Append(DOTween.ToAlpha(() => _titleText.color, value => _titleText.color = value, 0.2f, 1f).From(1f))
                .Append(DOTween.ToAlpha(() => _titleText.color, value => _titleText.color = value, 1f, 1f).From(0.2f))
                .SetLoops(-1);
        }
    }
}