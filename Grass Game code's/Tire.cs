using UnityEngine;

public class Tire : MonoBehaviour
{
    public float angle = 12;
    Joystick joystick;

    private void Start()
    {
        joystick = FindObjectOfType<FloatingJoystick>();
    }

    void Update()
    {
        if (Market.instance.wheelLevels.currentLevel > 3) return;

        Vector3 old = transform.localEulerAngles;
#if UNITY_EDITOR
        old.y = Input.GetAxis("Horizontal") * angle;
        transform.localEulerAngles = old;
#else
        old.y = joystick.Horizontal * angle;
        transform.localEulerAngles = old;
#endif
    }
}
