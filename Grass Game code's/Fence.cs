
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Fence : MonoBehaviour
{
    BoxCollider _collider;
    Rigidbody _rig;

    private void Reset()
    {
        _collider = GetComponent<BoxCollider>();
        _rig = GetComponent<Rigidbody>();
        _rig.useGravity = true;
        _rig.isKinematic = true;
    }

    private void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _rig = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            _collider.isTrigger = false;
            _rig.isKinematic = false;
            Vector3 randPos = Random.insideUnitSphere;
            randPos.y = 1; 
            _rig.AddForce(randPos * 15, ForceMode.Impulse);
            Delay();
        }
    }
    private IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(10);
        gameObject.SetActive(false);
    }

}
