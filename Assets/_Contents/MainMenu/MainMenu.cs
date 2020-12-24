using Cysharp.Threading.Tasks;
using Pyra.ApplicationStateManagement;
using Pyra.EventSystem;
using UnityEngine;

namespace Pyra.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private ApplicationStateVariable applicationState;
        [SerializeField] private GameEvent gameStartEvent;

        private void Start()
        {
            gameStartEvent
                .Subscribe(() => applicationState.Value = ApplicationStateEnum.GamePlay)
                .AddTo(this.GetCancellationTokenOnDestroy());
        }
    }
}