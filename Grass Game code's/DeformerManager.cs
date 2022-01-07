using AnilTools;
using System.Collections;
using UnityEngine;
using Moni = MiddleGames.Misc.Mono;

public class DeformerManager : MonoBehaviour, IDeformable
{
    private static DeformerManager _instance;
    public static DeformerManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DeformerManager>();
            }
            return _instance;
        }
    }

    public MeshDeformer[] deformers;

    public static int StageIndex;

    [SerializeField] private GameObject win, lose; // why you lose greek gold

    public ParticleSystem particle;
    public float RateOverTime;
    public const float playerRange = 2.8f;

    public Transform playerTransform;

    private void Awake()
    {
        _instance = this;

        StageIndex = 0;

        var emission = particle.emission;
        emission.rateOverTime = 0;

        deformers[0].CanDeform = true;
        Moni.instance.maxZ = instance.deformers[StageIndex].fenceParent.GetChild(0).transform.position.z - 5;
    }

    private void OpenFences()
    {
        for (int i = 0; i < deformers[StageIndex].fenceParent.childCount; i++)
        {
            var child = deformers[StageIndex].fenceParent.GetChild(i);
            child.GetComponent<BoxCollider>().isTrigger = true;
        }
    }

    public static void NextStage()
    {
        Debug.Log("next stage Baby");

        instance.OpenFences();
        instance.deformers[Mathf.Min(StageIndex + 2, _instance.deformers.Length - 1)].gameObject.SetActive(true);
        StageIndex++;

        Amazing.Show();

        if (StageIndex == instance.deformers.Length)
        {
            instance.FinishGame();
            var emission = instance.particle.emission;
            emission.rateOverTime = 0;
            return;
        }
        instance.deformers[StageIndex].CanDeform = true;
        instance.StartCoroutine(instance.Delay());
        Moni.instance.maxZ = instance.deformers[StageIndex].fenceParent.GetChild(0).position.z - 5;
    }
    void Update()
    { 
        particleDelay -= Time.deltaTime;
        if (particleDelay > 0)
        {
            var emission = instance.particle.emission;
            emission.rateOverTime = RateOverTime;
        }
        else
        {
            var emission = instance.particle.emission;
            emission.rateOverTime = 0;
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var main = instance.particle.main;
        main.startColor = GetStackMaterial().color;
    }

    int GetStageIndex()
    {
        return Mathf.Min(StageIndex, deformers.Length-1); 
    }

    float particleDelay;
    public void Deform()
    {
        sbyte deform = 0;
        
        switch (level)
        {
            case 0:
            {
                deform = deformers[GetStageIndex()].Deform(new Vector2(playerTransform.position.x, playerTransform.position.z), 
                                                playerRange,  0.14f, -1f ,  -0.05f, Vector3.up);
                break;
            }
            case 1:
            {
                    Vector3 leftPos = playerTransform.position - playerTransform.right;
                    deform = deformers[GetStageIndex()].Deform(new Vector2(leftPos.x, leftPos.z), 
                                            playerRange,  0.14f, -1f ,  -0.05f, Vector3.up);
                    
                    if (deform == -1) { NextStage(); return; }
        
                    Vector3 rightPos = playerTransform.position + playerTransform.right;
                    deform = deformers[StageIndex].Deform(new Vector2(rightPos.x, rightPos.z),
                                            playerRange, 0.14f, -1f, -0.05f, Vector3.up);
                    break;
            }
            case 2:
            {
                    Vector3 leftPos = playerTransform.position - (playerTransform.right * 1.5f);
                    deform = deformers[GetStageIndex()].Deform(new Vector2(leftPos.x, leftPos.z),
                                            playerRange, 0.14f, -1f, -0.05f, Vector3.up);
                    
                    if (deform == -1) { NextStage(); return; }

                    deform = deformers[GetStageIndex()].Deform(new Vector2(playerTransform.position.x, playerTransform.position.z),
                                            playerRange, 0.14f, -1f, -0.05f, Vector3.up);
                    
                    if (deform == -1) { NextStage(); return; }
        
                    Vector3 rightPos = playerTransform.position + (playerTransform.right * 1.5f);
                    deform = deformers[GetStageIndex()].Deform(new Vector2(rightPos.x, rightPos.z),
                                            playerRange, 0.14f, -1f, -0.05f, Vector3.up);
                    break;    
            }
        }
        
        if (deform == -1) { NextStage(); }
        else if (deform == 1)
        {
            particleDelay = 0.8f;
        }
    }

    public IRenderer CurrentRenderer()
    {
        return deformers[Mathf.Min(StageIndex, deformers.Length)];
    }

    private void FinishGame()
    {
        Stack.instance.SellAll();
        this.Delay(1.5f, () =>
        {
            var joystick = FindObjectOfType<FloatingJoystick>();
            joystick.OnPointerUp(null);
            joystick.enabled = false;
            win.SetActive(true);
            Stack.instance.Finish();
        });
    }
    
    public Material GetStackMaterial()
    {
        return deformers[GetStageIndex()].packMaterial;
    }

    private int level;
    public void IncreaseDeformArea(float value)
    {
        level = Mathf.Min(++level, 2);
        Moni.instance.IncreaseColliderRadius(value / 10);
    }
}
