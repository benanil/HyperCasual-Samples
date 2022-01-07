using MiddleGames.Misc;
using UnityEngine;

public class PaintCamera : MonoBehaviour
{
    public LayerMask ground;
    private GrassRendering[] grassRenderers;
    public Color paintColor = Color.black;
    public float cameraSpeed = 3;
    public float scrollSpeed = 5;
    public float paintDistance = 1;
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        grassRenderers = FindObjectsOfType<GrassRendering>();
    }

    RaycastHit hit;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 500f, ground))
            {
                foreach (var renderer in grassRenderers)
                {
                    renderer.Paint(hit.point, paintColor, paintDistance);
                }
            }
        }

        transform.position += cameraSpeed * Input.GetAxis("Vertical") * Time.deltaTime * transform.up;
        transform.position += cameraSpeed * Input.GetAxis("Horizontal") * Time.deltaTime * transform.right;
        transform.position += Input.mouseScrollDelta.y * scrollSpeed * Vector3.up;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(hit.point, 2);       
    }
}
