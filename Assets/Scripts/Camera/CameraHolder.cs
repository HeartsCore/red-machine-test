using Utils.Singleton;

namespace Camera
{
    public class CameraHolder : DontDestroyMonoBehaviourSingleton<CameraHolder>
    {
        [UnityEngine.SerializeField] private UnityEngine.Camera mainCamera;
        [UnityEngine.SerializeField] private CameraMovementConfig cameraMovementConfig;
        
        public UnityEngine.Camera MainCamera => mainCamera;
        public CameraMovementConfig CameraMovementConfig => cameraMovementConfig;

        private void Start()
        {
            var cameraMovementController = new CameraMovementController();
        }
    }
}