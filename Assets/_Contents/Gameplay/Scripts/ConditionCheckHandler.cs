using Cysharp.Threading.Tasks;
using Pyra.Collection;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class ConditionCheckHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _activeLevel;
        [SerializeField] private StringCollection _levelList;

        private void OnEnable() => CheckCondition().Forget();

        private async UniTaskVoid CheckCondition()
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.NextFrame(cancellationToken: token);

            if (_activeGrid.Completed)
            {
                _gameplayState.Value = _activeLevel == _levelList.Count - 1 ? GameplayStateEnum.AllClear : GameplayStateEnum.Win;
                if (_activeLevel == _levelList.Count - 1)
                    _activeLevel.Value = 0;
                return;
            }

            _gameplayState.Value = GameplayStateEnum.Drop;
        }
    }
}