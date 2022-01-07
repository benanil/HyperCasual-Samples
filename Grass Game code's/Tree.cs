using System;
using UnityEngine;

public class Tree : MonoBehaviour
{
    Vector3 targetAngle;
    bool falling;
    TreeManager treeManager;

    private void Update()
    {
        if (falling)
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetAngle, Time.deltaTime * 3);
            if (Vector3.Distance(transform.eulerAngles, targetAngle) < 1)
            {
                falling = false;
                Stack.instance.OnItemCollected(1);
            }
        }
    }

    public void Fall(TreeManager manager)
    {
        falling = true;
        targetAngle = (manager.transform.position - transform.position).normalized * 360;
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
    }
}
