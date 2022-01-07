using AnilTools;
using MiddleGames.Misc;
using System.Collections;
using UnityEngine;
using Vehicle = MiddleGames.Misc.Mono;

public class LeafManager : MonoBehaviour, IDeformable
{
    private static LeafManager _instance;
    public static LeafManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LeafManager>();
            }
            return _instance;
        }
    }

    public LeafRenderer[] LeafRenderers;

    public static int StageIndex;

    [SerializeField] private GameObject win, lose; // why you lose greek gold

    public ParticleSystem particle;
    public float RateOverTime;
    public float playerRange = 2;

    private void Awake()
    {
        _instance = this;

        foreach (var renderer in LeafRenderers)
        {
            renderer.playerRange = playerRange;
        }

        StageIndex = 0;

        var emission = particle.emission;
        emission.rateOverTime = 0;

        LeafRenderers[0].CanDeform = true;
        Vehicle.instance.maxZ = instance.LeafRenderers[StageIndex].fenceParent.GetChild(0).transform.position.z - 2;
    }

    public static void NextStage()
    {
        instance.LeafRenderers[Mathf.Min(StageIndex + 2, _instance.LeafRenderers.Length - 1)].gameObject.SetActive(true);
        StageIndex++;

        if (StageIndex == instance.LeafRenderers.Length)
        {
            Stack.instance.SellAll();

            instance.Delay(1.5f, () =>
            {
                instance.win.SetActive(true); 
                var joystick = FindObjectOfType<FloatingJoystick>();
                joystick.OnPointerUp(null);
                joystick.enabled = false;
                var emission = instance.particle.emission;
                emission.rateOverTime = 0;
                Stack.instance.Finish();
            });
            return;
        }
        instance.LeafRenderers[StageIndex].CanDeform = true;
        instance.StartCoroutine(instance.Delay());
        Vehicle.instance.maxZ = instance.LeafRenderers[StageIndex].fenceParent.GetChild(0).position.z - 1.5f;
    }

    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var main = instance.particle.main;
        main.startColor = GetStackMaterial().color;
    }


    public IRenderer CurrentRenderer()
    {
        return LeafRenderers[Mathf.Min(StageIndex, LeafRenderers.Length)];
    }

    public void Deform()
    {
        for (var i = 0; i < LeafRenderers.Length; i++)
        {
            if (LeafRenderers[i].CanDeform)
            {
                LeafRenderers[i].Deform();
            }
        }
    }

    public Material GetStackMaterial()
    {
        return instance.LeafRenderers[StageIndex].packMaterial;
    }

    public void IncreaseDeformArea(float value)
    {
        Vehicle.instance.IncreaseColliderRadius(value);
        foreach (var renderer in LeafRenderers)
        {
            renderer.Upgrade(value);
        }
    }
}
