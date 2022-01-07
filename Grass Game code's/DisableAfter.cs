using UnityEngine;

public class DisableAfter : MonoBehaviour
{
    public float delay;
    private void Start() => Invoke(nameof(Disable), delay);
    private void Disable() => gameObject.SetActive(false);
}
