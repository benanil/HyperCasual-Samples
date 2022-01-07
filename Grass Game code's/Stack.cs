using System.Collections.Generic;
using UnityEngine;
using AnilTools;
using AnilTools.Move;
using AnilTools.Lerping;
using UnityEngine.UI;
using System.Collections;

public class Stack : MonoBehaviour
{
    public static Stack instance;
    public Transform prefab;

    public int Capacity = 210;
    public float alignment = 0.3f;
    public int CountForPack = 500;
    public Vector3 scale;
    
    [Min(1)] public int width;
    // [Min(1)] public int maxHeight;
    [Min(1)] public int depth;

    private int currentIndex;

    private Stack<Transform> stack;

    public Transform[] itemPrefabs;
    // pools

    private List<TransformPool> prefabPools;

    public LayerMask ground;
    [NaughtyAttributes.Tag]
    public string packTag;

    public Transform sellTransform;

    private Image slider;

    public TMPro.TextMeshProUGUI moneyText;

    public static int Money;
    public static int ItemsInStorage; // grass or leaf

    public Object manager;
    private IDeformable deformable;

    public static void AddMoney(in int amount)    { Money += amount; instance.moneyText.text = Money.ToString(); }
    public static void RemoveMoney(in int amount)
    { Money -= amount; instance.moneyText.text = Money.ToString(); }

    private void Awake()
    {
        slider = GameObject.Find("SliderNew").GetComponent<Image>();
        AnilUpdate.Clean();
        if (manager == null) Debug.Log("Please Add Grass or leaf manager");

        deformable = manager as IDeformable;
        ItemsInStorage = 0;
        instance = this;

        stack = new Stack<Transform>();
        prefabPools = new List<TransformPool>(itemPrefabs.Length);

        Transform packsParent = new GameObject("pack's parent").transform;

        for (int i = 0; i < itemPrefabs.Length; i++)
        {
            var prefPool = new TransformPool(400, itemPrefabs[i], (p) =>
            {
                p.SetParent(packsParent);
            });
            prefabPools.Add(prefPool);
        }
    }
#if UNITY_EDITOR
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Market.instance.UpgradeTo(UpgradePart.leaf, 5);
            Market.instance.UpgradeTo(UpgradePart.grass, 5);
            Market.instance.UpgradeTo(UpgradePart.cleaner, 5);
            Market.instance.UpgradeTo(UpgradePart.wheels, 5);
            Market.instance.UpgradeTo(UpgradePart.snow, 5);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            LevelLoader.NextLevel();
        }
    }
#endif

    private Vector3 GetNextPosition()
    {
        currentIndex++;
        int index = 0;
        int maxLayer = Capacity / (width * depth) + 1;

        for (int y = 0; y < maxLayer; y++)
            for (int x = 0; x < width; x++)
                for (int z = 0; z < depth; z++)
                    if (++index == currentIndex)
                        return  new Vector3(x * alignment, y * alignment, -z * alignment);

        return default;
    }

    private void OnValidate() {}

    [ContextMenu("AddStack")]
    public void AddStack()
    {
        OnItemCollected(CountForPack * (width * depth) * 15);
    }

    public static bool Selling;

    [ContextMenu("SellAll")]
    public void SellAll()
    {
        slider.fillAmount = 0;
        Selling = true;

        IEnumerator SellCoroutine()
        {
            while (stack.Count > 0)
            {
                Transform pack = stack.Pop();
                for (int i = 0; i < 3; i++)
                {
                    pack.parent = null;
                    var upPoint = pack.position + (sellTransform.position - pack.position).normalized 
                                * (Vector3.Distance(pack.position, sellTransform.position) / 2) + ( Vector3.up * 20);

                    Vector3 worldToScreen = Camera.main.WorldToScreenPoint(sellTransform.position);

                    pack.transform.CubicLerpAnim(pack.position, upPoint, sellTransform.position, 2,
                        then: () =>
                        {
                            pack.gameObject.SetActive(false);
                            AddMoney(50);
                        });

                    currentIndex --;

                    if (stack.Count > 0) pack = stack.Pop();
                    else                 break;
                }

                if (slider) slider.fillAmount = currentIndex / (float)Capacity;

                yield return null;
            }
            currentIndex = 0;
            Selling = false;
        }
        StartCoroutine(SellCoroutine());

        if (currentIndex > 0)
        {
            FindObjectOfType<ObjectFlow>().Flow();
        }

        if (Rotater.instance) Rotater.instance.speed = Rotater.StartSpeed;
    }

    public bool IsFull() => stack.Count == Capacity;

    private void OnDrawGizmos()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Vector3 oldPos = transform.position;
                    oldPos.y += y * alignment;
                    oldPos.x += x * alignment;
                    oldPos.z -= z * alignment;
                    Gizmos.DrawWireCube(oldPos, scale);
                }
            }
        }
    }

    internal void OnItemCollected(int reducedItemAmount)
    {
        ItemsInStorage += reducedItemAmount;

        if (Selling) return;

        while (ItemsInStorage >= CountForPack)
        {
            ItemsInStorage = Mathf.Max(ItemsInStorage - CountForPack, 0);
            int prefabIndex = deformable.CurrentRenderer().GetPackIndex();
            var pref = prefabPools[prefabIndex].Get();

            pref.SetParent(transform, true);
            pref.gameObject.SetActive(true);

            pref.transform.localPosition = GetNextPosition();
            pref.localScale = Vector3.one;
            pref.localRotation = Quaternion.identity;
            pref.GetComponent<MeshRenderer>().material = deformable.GetStackMaterial();
            
            stack.Push(pref);
            
            if (IsFull())
            {
                if (Rotater.instance) Rotater.instance.speed = 0;
                break;
            }
        }

        if (slider)
        {
            slider.fillAmount = currentIndex / (float)Capacity;
        }
        else { Debug.Log("please add slider"); }
    }

    /// <summary>
    /// deactivates the slider
    /// </summary>
    internal void Finish()
    {
        slider.gameObject.SetActive(false);
    }
}
