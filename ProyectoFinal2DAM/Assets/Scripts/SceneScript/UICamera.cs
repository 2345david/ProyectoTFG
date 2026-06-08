using UnityEngine;

public class UICamera : MonoBehaviour
{
    public Transform player;
    public float xpos, ypos, zpos;
    
    
    void Start()
    {
        transform.position = new Vector3(player.position.x + xpos, player.position.y + ypos, zpos);
    }

    
    void Update()
    {
        transform.position = new Vector3(player.position.x + xpos, player.position.y + ypos, zpos);
    }
}
