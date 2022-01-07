using AnilTools;
using UnityEngine;
using AnilTools.Paint;
using Vehicle = MiddleGames.Misc.Mono;
using System.Collections;

public struct Temp : IRenderer
{
    public int GetPackIndex() => 0;
}

public class CleanerManager : Singleton<CleanerManager>, IDeformable
{

    public AnilMaterial[] carpets;

    public static int StageIndex;
    [SerializeField] private GameObject win, lose; // why you lose greek gold
    public ParticleSystem particle;
    public float RateOverTime;
    public int playerRange = 2;
    public Transform playerTransform;

    public LayerMask ground;

    private void Start()
    {
        StageIndex = 0; 
        _instance = this;
    }

    public void OnStageCompleted()
    {
        StartCoroutine(Delay());
    
        for (short i = 0; i < CurrentCarpet().FenceParent.childCount; i++)
        {
            CurrentCarpet().FenceParent.GetChild(i).GetComponent<BoxCollider>().isTrigger = true;
        }
        
        StageIndex++;

        if (StageIndex == carpets.Length)
        {
            var emission = particle.emission;
            emission.rateOverTime = 0;
            Stack.instance.SellAll();

            this.Delay(1.5f, () =>
            {
                win.SetActive(true);
                var joystick = FindObjectOfType<FloatingJoystick>();
                joystick.OnPointerUp(null);
                joystick.enabled = false;
            });
            Stack.instance.Finish();
            return;
        }

        Vehicle.instance.maxZ = carpets[StageIndex].FenceParent.GetChild(0).position.z - 1.5f;
        carpets[StageIndex].enabled = true;
        Amazing.Show();
    }

    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var main = particle.main;
        main.startColor = GetStackMaterial().color;
    }

    public IRenderer CurrentRenderer()
    {
        return new Temp();
    }

    public ParticleSystem[] particles;
    float emissionDelay;
    public void Deform()
    {
        if (Physics.Raycast(new Ray(playerTransform.position, Vector3.down), out RaycastHit hit, 10, ground))
        {
            int paintedPixels = Painter.ReplaceCircale(hit, playerRange, playerTransform.position);
            Stack.instance.OnItemCollected(paintedPixels);
            if (paintedPixels > 0)
            {
                emissionDelay = 0.2f;
                var emission = particle.emission; emission.rateOverTime = RateOverTime; 
                for(int i = 0; i < particles.Length; i++)
                {
                    particles[i].gameObject.SetActive(true);
                }
            }
            else if(paintedPixels == -1)
            {
                OnStageCompleted();
            }   
        }
        
        if (emissionDelay < 0)
        {
            var emission = particle.emission; emission.rateOverTime = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].gameObject.SetActive(false);
            }
        }
    }

    void Update()
    { 
        emissionDelay -= Time.deltaTime;
    }

    public Material GetStackMaterial()
    {
        return CurrentCarpet().StackMaterial;
    }

    private AnilMaterial CurrentCarpet()
    {
        return carpets[Mathf.Min(StageIndex, carpets.Length - 1)];
    }

    public void IncreaseDeformArea(float value)
    {
        playerRange += (int)value;
        Vehicle.instance.IncreaseColliderRadius(value / 10);
    }
}
