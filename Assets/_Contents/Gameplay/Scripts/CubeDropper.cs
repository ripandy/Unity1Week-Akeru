using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Pyra.EventSystem;
using Pyra.VariableSystem;
using UnityEngine;

namespace _Contents.Gameplay.Scripts
{
    public class CubeDropper : MonoBehaviour
    {
        [SerializeField] private Vector2Event _moveAxisEvent;
        [SerializeField] private BoolVariable _xVertical; // _zHorizontal
        
        [SerializeField] private GameEvent _dropEvent;
        [SerializeField] private IntEvent _cubeDropped;
        
        private const float MoveTreshold = 0.6f;
        
        private void Start()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _moveAxisEvent.Where(ShouldMove).Subscribe(MoveCube, token);
            _dropEvent.AsUniTaskAsyncEnumerable().SubscribeAwait(async _ => await AnimateDrop(), token);
        }

        private bool ShouldMove(Vector2 axis) => Mathf.Abs(axis.x) > MoveTreshold || Mathf.Abs(axis.y) > MoveTreshold;

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

            var tr = transform;
            var moveTo = tr.position + distance;
            tr.position = moveTo;
        }
        
        private async UniTask AnimateDrop()
        {
            
        }
    }
}