using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    
    Vector3 anchor;
    public static float zAnchor;
    public FloatingJoystick joystick;
    public Transform zoomTransform;
    public Transform targetPos;

    float startX;
    private Vector2 target;

    bool zooming;

    void Start()
    {
        startX = transform.localEulerAngles.x;
        instance = this;
        joystick = FindObjectOfType<FloatingJoystick>();
        anchor = transform.position - targetPos.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 euler = transform.localEulerAngles;
        float x = zooming ? 70 : startX;
        euler.x = Mathf.Lerp(euler.x, x, Time.deltaTime * 2);
        transform.localEulerAngles = euler;

        Vector3 oldPos = targetPos.position + anchor;
#if UNITY_EDITOR
        Vector2 joyDir = new Vector2(Input.GetAxis("Vertical") , Input.GetAxis("Horizontal"));
#else
        Vector2 joyDir = new Vector2(joystick.Vertical, joystick.Horizontal);
#endif
        target = Vector2.Lerp(target, joyDir, Time.deltaTime * 3);

        oldPos.x += target.y;
        oldPos.z += target.x * 2.25f;

        transform.position = oldPos;
    }

    public void SetZoom(bool value)
    {
        zooming = value;
    }

    [ContextMenu("SetTrue")] public void SetZoomTrue()  => SetZoom(true);
    [ContextMenu("SetFalse")] public void SetZoomFalse() => SetZoom(false);
}
