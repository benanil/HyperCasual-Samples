using UnityEngine;

public class Amazing : MonoBehaviour
{
    private bool moving;
    Vector3 startPos;
    public static Amazing instance;

    void Start()
    {
        instance = this;
        var oldPos = transform.position;
        oldPos.y -= 100;
        transform.position = oldPos;
        startPos = oldPos;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (moving)
        {
            Vector3 oldPos = transform.position;
            oldPos.y += Time.deltaTime * 300;
            transform.position = oldPos;
        }
    }

    public static void Show()
    {
        instance.gameObject.SetActive(true);
        instance.gameObject.transform.position = instance.startPos;
        instance.moving = true;
    }

}
