using UnityEngine;

namespace Camera
{
    [CreateAssetMenu(fileName = "CameraMovementConfig", menuName = "Configs/CameraMovementConfig", order = 1)]
    public class CameraMovementConfig : ScriptableObject
    {
        [Range(0.1f, 1.0f)] [SerializeField] private float moveDragTime = 0.5f;
        [Range(0.1f, 1.0f)] [SerializeField] private float smoothMoveReleaseTime = 0.4f;
        [Range(0.1f, 1.5f)] [SerializeField] private float moveReleaseTweenAnimationDuration  = 1.0f;
        [SerializeField] private Vector2 testCameraBorders = new(7, 7);
        
        public float MoveDragTime => moveDragTime;
        public float SmoothMoveReleaseTime => smoothMoveReleaseTime;
        public float MoveReleaseTweenAnimationDuration => moveReleaseTweenAnimationDuration;
        public Vector2 TestCameraBorders =>  testCameraBorders;
    }
}