using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class CubeBreakdownHandler : MonoBehaviour
    {
        [SerializeField] private CubeSideCollection _activeCube;
        [SerializeField] private IntVariable _cubeIndex;
        [SerializeField] private Transform _cubeBase;
        [SerializeField] private List<GameObject> _cubeSides;

        private void Awake() => ResetCube();

        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _cubeIndex.Subscribe(OpenBottom, token);
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
    }
}