using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class CubeMoveHandler : MonoBehaviour
    {
        [SerializeField] private Grid _grid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private CubeSideCollection _activeCube;
        
        [SerializeField] private Vector2Event _moveAxisEvent;
        [SerializeField] private BoolVariable _xVertical; // _zHorizontal

        [SerializeField] private Transform _cubeContainer;
        [SerializeField] private Transform _cubeBase;
        
        [SerializeField] private Transform _cubeSimulationContainer;
        [SerializeField] private Transform _cubeSimulationBase;

        private const float MoveTreshold = 0.6f;
        private const float AnimationDuration = 0.5f;
        private const float RollHeight = 0.25f;
        private const float RollDegree = 90f;

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _moveAxisEvent.Where(ShouldMove).SubscribeAwait(MoveCube, token);
        }

        private bool ShouldMove(Vector2 axis) => Mathf.Abs(axis.x) > MoveTreshold || Mathf.Abs(axis.y) > MoveTreshold;

        private async UniTask MoveCube(Vector2 axis, CancellationToken token)
        {
            var x = 0f;
            var y = 0f;
            if (Mathf.Abs(axis.x) > MoveTreshold)
                x = axis.x > 0 ? 1 : -1;
            else
                y = axis.y > 0 ? 1 : -1;

            var distance = Vector3.zero;
                distance.x = (_xVertical ? y : x) * -1;
                distance.z = _xVertical ? x : y;

            var moveTo = _cubeContainer.position + distance;

            var rotateTo = Vector3.zero;
                rotateTo.x = RollDegree * (_xVertical ? x : y);
                rotateTo.z = RollDegree * (_xVertical ? y : x);

            var cubeGrid = _grid.ToGrid(_cubeIndex);
                cubeGrid.x += (int) x;
                cubeGrid.y -= (int) y; 
            var predictedIndex = _grid.ToIndex(cubeGrid);

            var simulated = SimulateMove(moveTo, rotateTo, predictedIndex);
                
            var isValid = cubeGrid.x >= 0 && cubeGrid.x < _grid.width 
                          && cubeGrid.y >= 0 && cubeGrid.y < _grid.height
                          && simulated;
            
            if (!isValid)
                return;

            await AnimateMove(moveTo, rotateTo, token);

            _cubeIndex.Value = predictedIndex;
        }

        private async UniTask AnimateMove(Vector3 moveTo, Vector3 rotateTo, CancellationToken cancellationToken)
        {
            var height = RollHeight + _cubeContainer.position.y;
            await DOTween.Sequence()
                .Append(_cubeContainer.DOMove(moveTo, AnimationDuration).SetEase(Ease.Linear))
                .Join(_cubeContainer.DOLocalRotate(rotateTo, AnimationDuration).SetEase(Ease.Linear))
                .Join(_cubeContainer.DOLocalMoveY(height, AnimationDuration * 0.5f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo))
                .ToUniTask(cancellationToken: cancellationToken);
            
            _cubeBase.SetParent(null, true);
            _cubeContainer.rotation = Quaternion.identity;
            _cubeBase.SetParent(_cubeContainer, true);
        }

        private bool SimulateMove(Vector3 moveTo, Vector3 rotateTo, int predictedIndex)
        {
            _cubeSimulationBase.localRotation = _cubeBase.localRotation;
            
            _cubeSimulationContainer.position = moveTo;
            _cubeSimulationContainer.Rotate(rotateTo);
            
            _cubeSimulationBase.SetParent(null, true);
            _cubeSimulationContainer.rotation = Quaternion.identity;
            _cubeSimulationBase.SetParent(_cubeSimulationContainer, true);

            if (GetBottom(_cubeSimulationBase, out var onFloor))
            {
                if (_activeCube.IsIntact(onFloor)
                    || _activeCube.Any(pair => pair.Value == predictedIndex) )
                {
                    _activeCube.onFloor = onFloor;
                    return true;
                }
            }

            return false;
        }
        
        private bool GetBottom(Transform cubeBase, out CubeSide onFloor)
        {
            var bottom = Vector3.up * -1;
            if (bottom == cubeBase.forward)
                onFloor = CubeSide.Front;
            else if (bottom == cubeBase.forward * -1)
                onFloor = CubeSide.Back;
            else if (bottom == cubeBase.up)
                onFloor = CubeSide.Up;
            else if (bottom == cubeBase.up * -1)
                onFloor = CubeSide.Down;
            else if (bottom == cubeBase.right)
                onFloor = CubeSide.Right;
            else if (bottom == cubeBase.right * -1)
                onFloor = CubeSide.Left;
            else
            {
                onFloor = CubeSide.Down;
                return false;
            }
            return true;
        }
    }
}