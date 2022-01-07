using AnilTools;
using UnityEngine;

namespace MiddleGames.Misc
{ 
    // Character Controller
    public class Mono : MonoBehaviour
    {
        private static Mono _instance;
        public static Mono instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Mono>();
                }
                return _instance;
            }
        }

        public Material grassMaterial;
        public Joystick joystick;
        public float speed;
        [Tooltip("grass or leaf manager")]
        public Object Manager;
        IDeformable deformable;

        public float minX, maxX;
        public float minZ, maxZ;

        public Animator[] paletAnimators;

        private bool playerInUpgrade;
        public static bool canEnter = true; // to market
        public float turnSpeed = 5;

        private void Start()
        {
            Stack.Selling = false;
            canEnter = true;
            deformable = Manager as IDeformable;
        }
            

        float oldAngle;
        private void Update()
        {
            if (!canEnter || Market.UIOpen) return; // upgrading

#if UNITY_EDITOR
            Vector2 input = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")).normalized;
            Vector3 dir = Vector3.forward * input.x + (Vector3.right * input.y);
#else
            Vector3 dir = Vector3.forward * joystick.Vertical + (Vector3.right * joystick.Horizontal);
#endif

            if (Input.anyKey || Input.GetMouseButton(0))
            {
                Vector3 oldPos = transform.localPosition + speed * Time.deltaTime * dir;

                if (oldPos.z > 80 && oldPos.z <  120) 
                {
                    oldPos.x = Mathf.Clamp(oldPos.x, minX - 3, maxX);
                }
                else {
                    oldPos.x = Mathf.Clamp(oldPos.x, minX, maxX);
                }

                oldPos.z = Mathf.Clamp(oldPos.z, minZ, maxZ);

                transform.localPosition = oldPos;

                deformable.Deform();
                Vector3 oldEuler = transform.eulerAngles;
#if UNITY_EDITOR
                float newAngle = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;
#else
                float newAngle = Mathf.Atan2(joystick.Horizontal, joystick.Vertical) * Mathf.Rad2Deg;
#endif
                oldEuler.y = Mathf.LerpAngle(oldAngle, newAngle, Time.deltaTime * turnSpeed);
                transform.eulerAngles = oldEuler;
                oldAngle = oldEuler.y;
            }

            foreach (var animator in paletAnimators)
            {
                animator.speed = (!Input.anyKey) | Market.UIOpen ? 0 : 1;
            }
        }

        BoxCollider upgrade;

        private void OnTriggerEnter(Collider other)
        {

            if (other.CompareTag("Upgrade") && canEnter)
            {
                if (other.transform.childCount > 0)
                {
                    Vector3 oldPos = other.transform.GetChild(0).position;
                    oldPos.y = transform.position.y;
                    transform.position = oldPos; 
                }
                transform.rotation = Quaternion.identity;
                
                upgrade = other.transform.GetComponent<BoxCollider>();
                upgrade.size *= 10;

                CameraController.instance.SetZoom(true);
                canEnter = false;
                this.Delay(20f, () => canEnter = true);
                joystick.enabled = false;
                joystick.OnPointerUp(null);
                joystick.gameObject.SetActive(false);

                if (!playerInUpgrade) {
                    Market.instance.SetUI(true);
                    playerInUpgrade = true;
                }
            }

            if (other.CompareTag("Sell"))
            {
                if (!Stack.Selling)
                {
                    Stack.instance.SellAll();
                }
            }

        }

        public void Upgrade(float speed)
        {
            this.speed += speed;
        }

        // for breaking fences
        public void IncreaseColliderRadius(float value)
        {
            GetComponent<SphereCollider>().radius += value;
        }

        // market
        public void ExitUpgrade() 
        {
            this.Delay(3f, () => upgrade.size /= 10 );

            canEnter = true;
            joystick.enabled = true;
            playerInUpgrade = false;
            joystick.gameObject.SetActive(true);
        }

    }
}
