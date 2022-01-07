
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform targetPos;
    Vector3 anchor;
    
    void Start()
    {
        anchor = transform.position - targetPos.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = targetPos.position + anchor;
    }
}
