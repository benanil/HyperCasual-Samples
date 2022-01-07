using UnityEngine;

namespace Assets.Scripts
{
    public class Trash : MonoBehaviour
    {
        public static bool hovered;
        public LayerMask trashMask;

        private void Update()
        {
            hovered = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),500, trashMask);
        }
    }
}
