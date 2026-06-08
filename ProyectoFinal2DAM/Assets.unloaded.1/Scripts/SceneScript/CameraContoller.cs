using Unity.Mathematics.Geometry;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Transform activeRoom;
    public float dampSpeed;

    public static CameraController instance;
    
    [Range(-5, 5)]
    public float minModX, maxModX, minModY, maxModY;
    
    private float cameraZ = -10f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void FixedUpdate()
    {
        var minPosY = activeRoom.GetComponent<BoxCollider2D>().bounds.min.y + minModY;
        var maxPosY = activeRoom.GetComponent<BoxCollider2D>().bounds.max.y + maxModY;
        var minPosX = activeRoom.GetComponent<BoxCollider2D>().bounds.min.x +  minModX;
        var maxPosX = activeRoom.GetComponent<BoxCollider2D>().bounds.max.x + maxModX;
        
        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(player.position.x, minPosX, maxPosX),
            Mathf.Clamp(player.position.y, minPosY, maxPosY),
            Mathf.Clamp(player.position.z, -10f, -10f)
            );
        Vector3 smoothPos = Vector3.Lerp(transform.position, clampedPos, dampSpeed * Time.deltaTime);
        transform.position = smoothPos;
    }
}
