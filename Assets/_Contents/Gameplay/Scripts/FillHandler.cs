using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class FillHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private GameObject _fillPrefab;
        [SerializeField] private Material _emptyMaterial;
        [SerializeField] private Material _freshMaterial;
        [SerializeField] private Material _fillMaterial;
        
        private readonly Dictionary<int, MeshRenderer> _fills = new Dictionary<int, MeshRenderer>();
        private readonly Dictionary<GridState, Material> _materials = new Dictionary<GridState, Material>();

        private void Awake()
        {
            _materials.Add(GridState.Empty, _emptyMaterial);
            _materials.Add(GridState.Fresh, _freshMaterial);
            _materials.Add(GridState.Filled, _fillMaterial);
        }

        private void Start()
        {
            ResetFill();
            
            var token = this.GetCancellationTokenOnDestroy();
            _cubeIndex.WithoutCurrent().Subscribe(FillGrid, token);
            _gameplayState.Where(state => state == GameplayStateEnum.CubeComplete).Subscribe(_ => FinalizeFills(), token);
        }

        private void FillGrid(int index) => FillGrid(index, GridState.Fresh);

        private void FillGrid(int index, GridState gridState)
        {
            if (!_fills.ContainsKey(index))
                _fills.Add(index, Instantiate(_fillPrefab, transform).GetComponent<MeshRenderer>());

            var grid = _activeGrid.ToGrid(index);
            _fills[index].transform.position = new Vector3(grid.y, 0, grid.x);
            _fills[index].gameObject.SetActive(true);
            _fills[index].material = _materials[gridState];

            _activeGrid[index] = gridState;
        }

        private void FinalizeFills()
        {
            for (var i = 0; i < _activeGrid.Count; i++)
            {
                if (_activeGrid[i] == GridState.Fresh)
                {
                    _activeGrid[i] = GridState.Filled; 
                    _fills[i].material = _fillMaterial;
                }
            }
        }
        
        private void ResetFill()
        {
            foreach (var fill in _fills.Values)
            {
                fill.gameObject.SetActive(false);
            }
            
            for (var i = 0; i < _activeGrid.Count; i++)
            {
                if (_activeGrid[i] == GridState.Empty)
                {
                    FillGrid(i, _activeGrid[i]);
                    _fills[i].material = _emptyMaterial;
                }
            }
        }
    }
}