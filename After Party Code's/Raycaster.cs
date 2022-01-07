using UnityEngine;

namespace Assets.Scripts
{
    public class Raycaster : MonoBehaviour
    {
        private Prop currentProp;
        public LayerMask PickableMask;
        public LayerMask UsingMask;
        public LayerMask GroundMask;
        
        private Ray ScreenRay => Camera.main.ScreenPointToRay(Input.mousePosition);

        private bool hasPickable = false;
        
        private void Update() {

            if (Input.GetMouseButton(0)) 
            if (hasPickable) MoveProp();
            else if(Physics.Raycast(ScreenRay, out RaycastHit hit, 500, PickableMask))  Pick(hit);
            if (Input.GetMouseButtonUp(0) && hasPickable) Drop();
        }

        private void MoveProp()
        {
            if (Physics.Raycast(ScreenRay, out RaycastHit hit, 500, GroundMask)) 
                currentProp.transform.position = hit.point + hit.normal * .25f; 
        }

        public void Pick(in RaycastHit hit) {
            
            if (hit.transform.TryGetComponent(out currentProp)) {
                currentProp.gameObject.layer = UsingMask;
                currentProp.Pick();
                hasPickable = true;
            }
        }

        public void Drop() {
            currentProp.gameObject.layer = LayerMask.NameToLayer("Pickable");
            currentProp.Drop();
            hasPickable = false;
        }

    }
}