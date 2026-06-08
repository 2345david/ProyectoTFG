using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform activeRoom;
    public float smoothTime = 0.2f;

    public static CameraController instance;

    private Camera _cam;
    private Vector3 _currentVelocity = Vector3.zero;
    
    // List to keep track of rooms the player is currently inside
    private List<Transform> _detectedRooms = new List<Transform>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (player == null || activeRoom == null) return;

        Bounds roomBounds = activeRoom.GetComponent<BoxCollider2D>().bounds;

        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;

        float minPosX = roomBounds.min.x + halfWidth;
        float maxPosX = roomBounds.max.x - halfWidth;
        float minPosY = roomBounds.min.y + halfHeight;
        float maxPosY = roomBounds.max.y - halfHeight;

        if (roomBounds.size.x < halfWidth * 2)
        {
            minPosX = maxPosX = roomBounds.center.x;
        }
        if (roomBounds.size.y < halfHeight * 2)
        {
            minPosY = maxPosY = roomBounds.center.y;
        }

        float targetX = Mathf.Clamp(player.position.x, minPosX, maxPosX);
        float targetY = Mathf.Clamp(player.position.y, minPosY, maxPosY);

        Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, smoothTime);
    }

    public void RegisterRoom(Transform room)
    {
        if (!_detectedRooms.Contains(room))
        {
            _detectedRooms.Add(room);
        }
        activeRoom = room;
    }

    public void UnregisterRoom(Transform room)
    {
        _detectedRooms.Remove(room);
        
        // If we just left the active room, switch to the previous one in the stack
        if (activeRoom == room)
        {
            if (_detectedRooms.Count > 0)
            {
                activeRoom = _detectedRooms[_detectedRooms.Count - 1];
            }
        }
    }
}
