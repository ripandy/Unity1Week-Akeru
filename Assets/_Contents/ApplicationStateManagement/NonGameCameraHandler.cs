using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

namespace Pyra.ApplicationStateManagement
{
    public class NonGameCameraHandler : MonoBehaviour
    {
        [SerializeField] private ApplicationStateVariable _applicationState;
        [SerializeField] private GameObject _nonGameCameraObject;

        private void Start()
        {
            _applicationState.Subscribe(state =>
                _nonGameCameraObject.SetActive(state != ApplicationStateEnum.GamePlay));
        }
    }
}