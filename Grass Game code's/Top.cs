using UnityEngine;

public class Top : MonoBehaviour
{
    public float çarpan = 8;

    public void OnCollisionStay(Collision collision)
    {

        
        if (collision.transform.gameObject.tag == "DeformableMesh")
        {
            MeshDeformer2 meshDeformer = collision.transform.GetComponent<MeshDeformer2>();
            ContactPoint[] contactPoints = new ContactPoint[collision.contactCount];
            collision.GetContacts(contactPoints);
            foreach (ContactPoint contactPoint in contactPoints)
            {
                meshDeformer.Deform(contactPoint.point, 0.7f * 10 / çarpan,  0.05f * 7 / çarpan, -1f * 2.5f / çarpan, -0.05f * 2.5f / çarpan, Vector3.up);
                // meshDeformer.Deform(contactPoint.point, 2, 0.5f, -1f , -.2f , Vector3.up);
            }
        }
    }
}

