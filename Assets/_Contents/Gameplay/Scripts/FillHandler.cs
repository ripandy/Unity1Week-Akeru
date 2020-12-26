using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class FillHandler : MonoBehaviour
    {
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private GameEvent _cubeCompleted;
        [SerializeField] private GameObject _fillPrefab;
        [SerializeField] private Material _freshMaterial;
        [SerializeField] private Material _fillMaterial;
        
        private readonly Dictionary<int, MeshRenderer> _fills = new Dictionary<int, MeshRenderer>();

        private void Awake() => _activeGrid.Initialize();

        private void Start()
        {
            ResetFill();
            
            var token = this.GetCancellationTokenOnDestroy();
            _cubeIndex.Subscribe(FillGrid, token);
            _cubeCompleted.Subscribe(FinalizeFills, token);
        }

        private void FillGrid(int index)
        {
            if (!_fills.ContainsKey(index))
                _fills.Add(index, Instantiate(_fillPrefab, transform).GetComponent<MeshRenderer>());

            var grid = _activeGrid.ToGrid(index);
            _fills[index].transform.position = new Vector3(grid.y, 0, grid.x);
            _fills[index].gameObject.SetActive(true);
            _fills[index].material = _freshMaterial;

            _activeGrid[index] = GridState.Fresh;
        }

        private void FinalizeFills()
        {
            for (var i = 0; i < _activeGrid.Count; i++)
            {
                if (_activeGrid[i] == GridState.Fresh)
                    _activeGrid[i] = GridState.Filled;
                if (_fills.ContainsKey(i))
                    _fills[i].material = _fillMaterial;
            }
        }
        
        private void ResetFill()
        {
            foreach (var fill in _fills.Values)
            {
                fill.gameObject.SetActive(false);
            }
        }
    }
}