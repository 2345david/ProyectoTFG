using UnityEngine;

namespace Metroidvania.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomController : MonoBehaviour
    {
        [Header("Room Info")]
        public string roomName;
        public bool isCheckpoint;

        public delegate void RoomEvent();
        public event RoomEvent OnEnter;
        public event RoomEvent OnExit;

        [Header("Camera Configuration")]
        public Transform cameraFollowTarget;
        // Reference by name or find in children to avoid missing types
        public MonoBehaviour virtualCamera; 

        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnter();
            }
        }

        private void OnPlayerEnter()
        {
            Debug.Log($"Entered Room: {roomName}");
            OnEnter?.Invoke();

            if (CameraController.instance != null)
            {
                CameraController.instance.RegisterRoom(this.transform);
            }

            if (virtualCamera != null)
            {
                var prop = virtualCamera.GetType().GetProperty("Priority");
                if (prop != null) prop.SetValue(virtualCamera, 10);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnExit?.Invoke();
                if (CameraController.instance != null)
                {
                    CameraController.instance.UnregisterRoom(this.transform);
                }

                if (virtualCamera != null)
                {
                    var prop = virtualCamera.GetType().GetProperty("Priority");
                    if (prop != null) prop.SetValue(virtualCamera, 0);
                }
            }
        }
}
}
