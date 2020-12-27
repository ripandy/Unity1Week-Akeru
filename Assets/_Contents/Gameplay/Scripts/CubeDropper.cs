using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class CubeDropper : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private Grid _grid;
        [SerializeField] private Vector2Event _moveAxisEvent;
        [SerializeField] private BoolVariable _xVertical; // _zHorizontal
        [SerializeField] private IntVariable _cubeIndex;
        
        [SerializeField] private GameEvent _dropEvent;

        [SerializeField] private GameObject _baseObject;
        [SerializeField] private Transform _cubeBase;

        [SerializeField] private MeshRenderer _dropMark;
        [SerializeField] private Material _droppableMaterial;
        [SerializeField] private Material _nonDroppableMaterial;
        
        private readonly AsyncReactiveProperty<bool> _droppable = new AsyncReactiveProperty<bool>(true);

        private Vector3 _defaultCubePosition;
        private int _activeIndex;
        private bool _dropping;
        
        private const float MoveTreshold = 0.6f;
        private const float AnimationDuration = 0.5f;

        private void Awake() => _defaultCubePosition = _cubeBase.localPosition;

        private void OnEnable() => ResetPosition();

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _moveAxisEvent.Where(ShouldMove).Subscribe(MoveCube, token);
            
            _dropEvent.AsUniTaskAsyncEnumerable()
                .Where(_ => _gameplayState.Value == GameplayStateEnum.Drop && _droppable)
                .SubscribeAwait(async _ => await AnimateDrop(token), token);
            
            _droppable.Subscribe(droppable =>
                _dropMark.material = droppable ? _droppableMaterial : _nonDroppableMaterial, token);
        }

        private bool ShouldMove(Vector2 axis) =>
            (Mathf.Abs(axis.x) > MoveTreshold || Mathf.Abs(axis.y) > MoveTreshold)
            && !_dropping;

        private void MoveCube(Vector2 axis)
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
                
            var cubeGrid = _grid.ToGrid(_activeIndex);
                cubeGrid.x += (int) x;
                cubeGrid.y -= (int) y;
                
            var isValid = cubeGrid.x >= 0 && cubeGrid.x < _grid.width
                          && cubeGrid.y >= 0 && cubeGrid.y < _grid.height;
            
            if (!isValid)
                return;

            var tr = transform;
            var moveTo = tr.position + distance;
            tr.position = moveTo;
            _activeIndex = _grid.ToIndex(cubeGrid);
            _droppable.Value = _grid[_activeIndex] == GridState.Empty;
        }
        
        private async UniTask AnimateDrop(CancellationToken cancellationToken)
        {
            _dropping = true;
            await _cubeBase.DOLocalMoveY(0, AnimationDuration).ToUniTask(cancellationToken: cancellationToken);
            _baseObject.SetActive(false);
            _cubeIndex.Value = _activeIndex;
            _gameplayState.Value = GameplayStateEnum.Fill;
        }

        private void ResetPosition()
        {
            var grid = _grid.ToGrid(_cubeIndex);
            transform.position = new Vector3(grid.y, 0, grid.x);
            _cubeBase.localPosition = _defaultCubePosition;
            _baseObject.SetActive(true);
            _activeIndex = _cubeIndex;
            _droppable.Value = _grid[_activeIndex] == GridState.Empty;
            _dropping = false;
        }
    }
}