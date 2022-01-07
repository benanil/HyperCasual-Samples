
using Assets.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Outline))]
public class Prop : MonoBehaviour
{
    Rigidbody rig;
    Vector3 startPos;
    Quaternion startRotation;

    [SerializeField] private bool trash;

    public static bool Party
    {
        get => party;
        set {
            party = value;
            var props = GameObject.FindObjectsOfType<Prop>();
            GameObject.Find("DustDirtyPoof (1)").GetComponent<ParticleSystem>().Play(value);
            foreach (var item in props) {
                var rig = item.GetComponent<Rigidbody>();
                rig.AddForce(Random.insideUnitSphere * 6, ForceMode.Impulse);
                rig.AddTorque(Random.insideUnitSphere * 20,ForceMode.Impulse);
            }
        }
    }
    private static bool party;
    private static Vector3 partyPos => new Vector3(-2, 1, -2) + new Vector3(Mathf.Sin(Time.time * 2) * 1.5f, 0,Mathf.Cos(Time.time * 2) * 1.5f);

    private static readonly float force = 200;

    private Outline outline;
    private void Reset() {
        //GetComponent<MeshCollider>().convex = true;
    }

    private void Start() {
        outline = GetComponent<Outline>();
        rig = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRotation = transform.rotation;
    }

    private void Update() {
        
        if (Party) {
            rig.AddForce((partyPos - transform.position) * force * Time.deltaTime);
        }
    }
    
    public void Pick() {
        transform.rotation = startRotation;
        SetKinematic(true);
        outline.enabled = false;
    }
    public void Drop() {
        transform.rotation = startRotation;
        SetKinematic(false);

        if (Vector3.Distance(transform.position, startPos) < 1 && !trash) {
            transform.position = startPos;
            GameManager.instance.OnPropPlaced();
            GetComponent<Outline>().enabled = false;
            Destroy(this);
        }
        else if (Trash.hovered) {
            GameManager.instance.OnTrashAdded();
            Destroy(this.gameObject);
        }
        else {
            outline.enabled = true;
        }
    }

    private void SetKinematic(bool value) {
        rig.isKinematic = value;
        GetComponent<Collider>().isTrigger = value;
    }

}
