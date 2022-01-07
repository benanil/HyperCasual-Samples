
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public static float StartSpeed;
    public static Rotater instance { get; private set; }
    public float speed = 31;

    private void Start()
    {
        StartSpeed = speed;
        instance = this;
    }

    void LateUpdate()
    {
        transform.Rotate(Vector3.up * speed , Space.Self);
    }
}
