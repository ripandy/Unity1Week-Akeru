using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class CubeBreakdownHandler : MonoBehaviour
    {
        [SerializeField] private GameplayStateVariable _gameplayState;
        [SerializeField] private CubeSideCollection _activeCube;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private List<GameObject> _cubeSides;

        private void OnEnable()
        {
            ResetCube();
            OpenBottom(_cubeIndex);
        }

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            
            _cubeIndex.WithoutCurrent().Subscribe(OpenBottom, token);
            
            UniTaskAsyncEnumerable.EveryValueChanged(_activeCube, collection => collection.IsCompleted)
                .Where(completed => completed)
                .Subscribe(_ => OnCubeCompleted(), token);
        }

        private void OpenBottom(int index)
        {
            var onFloor = _activeCube.onFloor;
            _cubeSides[(int) onFloor].SetActive(false);
            _activeCube[onFloor] = index;
            _activeCube.onFloor = onFloor;
        }

        private void ResetCube()
        {
            _activeCube.ResetSides();
            foreach (var side in _cubeSides)
            {
                side.SetActive(true);
            }
        }

        private void OnCubeCompleted()
        {
            gameObject.SetActive(false);
            _gameplayState.Value = GameplayStateEnum.CubeComplete;
        }
    }
}