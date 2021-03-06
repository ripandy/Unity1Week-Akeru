using System.Collections.Generic;
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
        [SerializeField] private GameplayStateVariable _gameplayState;
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
        private const float AnimationDuration = 0.35f;
        private const float RollHeight = 0.25f;
        private const float RollDegree = 90f;

        private int _availableMove;
        private readonly List<int> _moveCheck = new List<int>();

        private void OnEnable() => InitializePosition(_cubeIndex);

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _moveAxisEvent.Where(ShouldMove).SubscribeAwait(MoveCube, token);
        }
        
        private void InitializePosition(int index)
        {
            var grid = _grid.ToGrid(index);
            transform.position = new Vector3(grid.y, 0.5f, grid.x);
            _cubeContainer.rotation = Quaternion.identity;
            _cubeBase.rotation = Quaternion.identity;
            
            var token = this.GetCancellationTokenOnDestroy();
            CheckAvailableMove(token, true).Forget();
        }

        private bool ShouldMove(Vector2 axis) => Mathf.Abs(axis.x) > MoveTreshold || Mathf.Abs(axis.y) > MoveTreshold;

        private async UniTask MoveCube(Vector2 axis, CancellationToken token)
        {
            var x = 0;
            var y = 0;
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
                cubeGrid.x += x;
                cubeGrid.y -= y; 
            var predictedIndex = _grid.ToIndex(cubeGrid);

            var simulated = SimulateMove(moveTo, rotateTo, predictedIndex, out var onFloor);
                
            var isValid = cubeGrid.x >= 0 && cubeGrid.x < _grid.width 
                          && cubeGrid.y >= 0 && cubeGrid.y < _grid.height
                          && (_grid[predictedIndex] == GridState.Empty || _grid[predictedIndex] == GridState.Fresh)
                          && simulated;
            
            if (!isValid)
                return;

            await AnimateMove(moveTo, rotateTo, token);

            _activeCube.onFloor = onFloor;
            _cubeIndex.Value = predictedIndex;

            CheckAvailableMove(token).Forget();
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

        private bool SimulateMove(Vector3 moveTo, Vector3 rotateTo, int predictedIndex, out CubeSide onFloor)
        {
            _cubeSimulationBase.localRotation = _cubeBase.localRotation;
            
            _cubeSimulationContainer.position = moveTo;
            _cubeSimulationContainer.Rotate(rotateTo);
            
            _cubeSimulationBase.SetParent(null, true);
            _cubeSimulationContainer.rotation = Quaternion.identity;
            _cubeSimulationBase.SetParent(_cubeSimulationContainer, true);

            if (GetBottom(_cubeSimulationBase, out onFloor))
            {
                if (_activeCube.IsIntact(onFloor)
                    || _activeCube.Any(pair => pair.Value == predictedIndex))
                    return true;
            }

            return false;
        }
        
        private bool SimulateNonBacktrackMove(Vector3 moveTo, Vector3 rotateTo, Quaternion startingRotation)
        {
            _cubeSimulationBase.localRotation = startingRotation;
            
            _cubeSimulationContainer.position = moveTo;
            _cubeSimulationContainer.Rotate(rotateTo);
            
            _cubeSimulationBase.SetParent(null, true);
            _cubeSimulationContainer.rotation = Quaternion.identity;
            _cubeSimulationBase.SetParent(_cubeSimulationContainer, true);

            if (GetBottom(_cubeSimulationBase, out var onFloor))
            {
                if (_activeCube.IsIntact(onFloor))
                    return true;
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

        private int CheckAvailableMoves(int index, List<int> checkedIndex, Quaternion startingRotation)
        {
            var availableMove = 0;
            for (var j = -1; j <= 1; j++)
            {
                for (var i = -1; i <= 1; i++)
                {
                    if (i == 0 && j != 0 || i != 0 && j == 0)
                    {
                        var cubeGrid = _grid.ToGrid(index);
                        cubeGrid.x += i;
                        cubeGrid.y -= j; 
                        var predictedIndex = _grid.ToIndex(cubeGrid);
                        
                        if (checkedIndex.Contains(predictedIndex) || predictedIndex < 0 || predictedIndex >= _grid.Count)
                            continue;

                        var distance = Vector3.zero;
                            distance.x = i;
                            distance.z = j;

                        var moveTo = _cubeContainer.position + distance;

                        var rotateTo = Vector3.zero;
                            rotateTo.x = RollDegree * i;
                            rotateTo.z = RollDegree * j;

                        var simulated = SimulateNonBacktrackMove(moveTo, rotateTo, startingRotation);
                        
                        var isValid = cubeGrid.x >= 0 && cubeGrid.x < _grid.width
                                      && cubeGrid.y >= 0 && cubeGrid.y < _grid.height
                                      && _grid[predictedIndex] == GridState.Empty
                                      && simulated;

                        if (isValid)
                            availableMove++;
                        
                        checkedIndex.Add(index);

                        if (_grid[predictedIndex] == GridState.Fresh)
                            availableMove += CheckAvailableMoves(predictedIndex, checkedIndex,
                                _cubeSimulationBase.localRotation);
                    }
                }
            }

            return availableMove;
        }

        private async UniTaskVoid CheckAvailableMove(CancellationToken token, bool firstCheck = false)
        {
            await UniTask.NextFrame(cancellationToken: token);
            
            _moveCheck.Clear();
            _availableMove = CheckAvailableMoves(_cubeIndex.Value, _moveCheck, _cubeBase.localRotation);
            
            if (firstCheck)
                await UniTask.NextFrame(cancellationToken: token);
            
            if (_availableMove == 0 && (!_activeCube.IsCompleted || firstCheck))
                _gameplayState.Value = GameplayStateEnum.Lose;
        }
    }
}