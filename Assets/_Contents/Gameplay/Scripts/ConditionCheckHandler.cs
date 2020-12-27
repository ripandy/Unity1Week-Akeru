using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class ConditionCheckHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private Grid _activeGrid;

        private void OnEnable() => CheckCondition().Forget();

        private async UniTaskVoid CheckCondition()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.NextFrame(cancellationToken: token);
            
            if (_activeGrid.Completed)
            {
                _gameplayState.Value = GameplayStateEnum.Win;
                return;
            }

            // TODO: check lose condition
            
            _gameplayState.Value = GameplayStateEnum.Drop;
        }
    }
}