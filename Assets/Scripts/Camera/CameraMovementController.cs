using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using Player;
using Player.ActionHandlers;
using UnityEngine;

namespace Camera
{
    public class CameraMovementController : System.IDisposable
    {
        private readonly ClickHandler _clickHandler;
        private readonly Transform _cameraTransform;
        private readonly float _moveDragTime;
        private readonly float _smoothMoveReleaseTime;
        private readonly float _moveReleaseTweenAnimationDuration;
        private readonly Vector2 _testCameraBorders;
        
        private Vector3 _endClickPoint;
        private Vector3 _startClickPoint;

        private CameraMovementState _currentState;
        private CameraMovementState _lastState;
        private CancellationTokenSource _moveReleaseCts;
        
        public CameraMovementController()
        {
            _clickHandler = ClickHandler.Instance;
            _clickHandler.AddDragEventHandlers(OnDragStart, OnDragEnd);
            
            _cameraTransform = CameraHolder.Instance.MainCamera.transform;
            
            var config = CameraHolder.Instance.CameraMovementConfig;
            _moveDragTime = config.MoveDragTime;
            _smoothMoveReleaseTime = config.SmoothMoveReleaseTime;
            _testCameraBorders = config.TestCameraBorders;
            _moveReleaseTweenAnimationDuration = config.MoveReleaseTweenAnimationDuration;
        }

        public void Dispose()
        {
            _clickHandler.RemoveDragEventHandlers(OnDragStart, OnDragEnd);
        }

        private void OnDragStart(Vector3 startPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Scrolling)
                return;

            _currentState = CameraMovementState.Moving;
            MoveAsync().Forget();
        }

        private void OnDragEnd(Vector3 finishPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Scrolling)
                return;

            _currentState = CameraMovementState.Slowing;

            if (_moveReleaseCts != null)
            {
                _moveReleaseCts.Cancel();
                _moveReleaseCts.Dispose();
            }

            _moveReleaseCts = new CancellationTokenSource();
            MoveReleaseAsync(_moveReleaseCts.Token).Forget();
        }

        private async UniTaskVoid MoveReleaseAsync(CancellationToken cancellationToken)
        {
            var cameraPos = _cameraTransform.position;

            var distance = Vector3.Distance(_endClickPoint, _startClickPoint);
            var directionNormal = (_endClickPoint - _startClickPoint).normalized;

            var targetPosition = cameraPos + directionNormal * distance;
            targetPosition.x = Mathf.Clamp(targetPosition.x, -_testCameraBorders.x, _testCameraBorders.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -_testCameraBorders.y, _testCameraBorders.y);
            targetPosition.z = cameraPos.z;

            var velocity = Vector3.zero;
            var moveTime = 0f;

            while (moveTime < _smoothMoveReleaseTime)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                cameraPos = Vector3.SmoothDamp(cameraPos, targetPosition, ref velocity, _smoothMoveReleaseTime);
                _cameraTransform.position = cameraPos;

                moveTime += Time.deltaTime;

                await UniTask.Yield();
            }
            await _cameraTransform.DOMove(targetPosition, _moveReleaseTweenAnimationDuration).SetEase(Ease.OutQuad).WithCancellation(cancellationToken);
        }
        
        private async UniTaskVoid MoveAsync()
        {
            var initialMousePos = Input.mousePosition;
            var velocity = Vector3.zero;
            var z = _cameraTransform.position.z;

            while (_currentState == CameraMovementState.Moving)
            {
                var currentMousePos = Input.mousePosition;

                var movement = (initialMousePos - currentMousePos) / Screen.dpi;

                if (movement != Vector3.zero)
                {
                    _endClickPoint = _startClickPoint + movement;

                    var position = _cameraTransform.position;
                    var cameraPos = position;
                    var directionNormal = (_endClickPoint - _startClickPoint).normalized;
                    var distance = Vector3.Distance(_endClickPoint, _startClickPoint);

                    var targetPosition = cameraPos + directionNormal * distance;
                    targetPosition.x = Mathf.Clamp(targetPosition.x, -_testCameraBorders.x, _testCameraBorders.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, -_testCameraBorders.y, _testCameraBorders.y);
                    targetPosition.z = z;

                    position =
                        Vector3.SmoothDamp(position, targetPosition, ref velocity, _moveDragTime);

                    _cameraTransform.position = position;

                    await UniTask.DelayFrame(1);
                }
                else
                {
                    await UniTask.Yield();
                }
            }
        }

        private enum CameraMovementState
        {
            Moving,
            Slowing
        }
    }
}
