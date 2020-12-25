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
        [SerializeField] private Vector2Event _moveAxisEvent;
        [SerializeField] private BoolVariable _xVertical; // _zHorizontal

        [SerializeField] private Transform _cubeContainer;
        [SerializeField] private Transform _cubeBase;

        private const float MoveTreshold = 0.6f;
        private const float AnimationDuration = 0.5f;
        private const float RollHeight = 0.2f;
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

            await AnimateMove(moveTo, rotateTo, token);
        }

        private async UniTask AnimateMove(Vector3 moveTo, Vector3 rotateTo, CancellationToken cancellationToken)
        {
            await DOTween.Sequence()
                .Append(_cubeContainer.DOMove(moveTo, AnimationDuration).SetEase(Ease.Linear))
                .Join(_cubeBase.DOLocalRotate(rotateTo, AnimationDuration).SetEase(Ease.Linear))
                .Join(_cubeBase.DOLocalMoveY(RollHeight, AnimationDuration * 0.5f).SetEase(Ease.InQuad).SetLoops(2, LoopType.Yoyo))
                .ToUniTask(cancellationToken: cancellationToken);

            _cubeBase.rotation = Quaternion.identity;
        }
    }
}