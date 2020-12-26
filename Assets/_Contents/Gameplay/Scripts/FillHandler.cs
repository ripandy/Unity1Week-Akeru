using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class FillHandler : MonoBehaviour
    {
        [SerializeField] private Grid _activeGrid;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private GameObject _fillPrefab;
        [SerializeField] private Transform _fillContainer;
        
        private readonly Dictionary<int, GameObject> _fills = new Dictionary<int, GameObject>();

        private void Awake() => _activeGrid.Initialize();

        private void Start()
        {
            ResetFill();
            
            var token = this.GetCancellationTokenOnDestroy();
            _cubeIndex.Subscribe(FillGrid, token);
        }

        private void FillGrid(int index)
        {
            if (!_fills.ContainsKey(index))
                _fills.Add(index, Instantiate(_fillPrefab, _fillContainer));

            var grid = _activeGrid.ToGrid(index);
            _fills[index].transform.position = new Vector3(grid.y, 0, grid.x);
            _fills[index].SetActive(true);
        }
        
        private void ResetFill()
        {
            foreach (var fill in _fills.Values)
            {
                fill.SetActive(false);
            }
        }
    }
}