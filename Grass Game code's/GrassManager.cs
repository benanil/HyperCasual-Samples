using AnilTools;
using MiddleGames.Misc;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Moni = MiddleGames.Misc.Mono;

public interface IDeformable
{
    void Deform();
    IRenderer CurrentRenderer();
    Material GetStackMaterial();
    public void IncreaseDeformArea(float value);
}

public class GrassManager : MonoBehaviour, IDeformable
{
    private static GrassManager _instance;
    public static GrassManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GrassManager>();
            }
            return _instance;
        }
    }
    public GrassRendering[] GrassRenderers;

    public static int StageIndex;

    [SerializeField] private GameObject win, lose; // why you lose greek gold

    public ParticleSystem particle;
    public float RateOverTime;
    public float playerRange = 2;
    private ParticleSystem particleChild;

    private void Awake()
    {
        _instance = this;
        StageIndex = 0;

        particleChild = particle.transform.GetChild(0).GetComponent<ParticleSystem>();

        foreach (var renderer in GrassRenderers)
        {
            renderer.playerRange = playerRange;
        }

        var emission = particle.emission;
        emission.rateOverTime = 0;

        GrassRenderers[0].CanDeform = true;
        Moni.instance.maxZ = instance.GrassRenderers[StageIndex].fenceParent.GetChild(0).transform.position.z - 2;
    }

    public void SetParticleEmission(bool value)
    {
        float rate= value ? RateOverTime : 0;
        var emission = particle.emission;
        var emission1 = particleChild.emission;
        emission.rateOverTime = rate;
        emission1.rateOverTime = rate;
    }

    public static void NextStage()
    {
        instance.GrassRenderers[Mathf.Min(StageIndex + 2, _instance.GrassRenderers.Length - 1)].gameObject.SetActive(true);
        StageIndex++;

        if (StageIndex == instance.GrassRenderers.Length) {
            instance.FinishGame();
            Stack.instance.SellAll();
            var emission = instance.particle.emission;
            emission.rateOverTime = 0;
            return;
        }
        instance.GrassRenderers[StageIndex].CanDeform = true;
        instance.StartCoroutine(instance.Delay());
        Moni.instance.maxZ = instance.GrassRenderers[StageIndex].fenceParent.GetChild(0).position.z - 1.5f;
    }

    IEnumerator Delay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        var main = instance.particle.main;
        main.startColor = instance.GrassRenderers[StageIndex]._grassColor;
        var main1 = instance.particleChild.main;
        main1.startColor = instance.GrassRenderers[StageIndex]._grassColor;
    }

    private void FinishGame()
    {
        Stack.instance.SellAll();
        this.Delay(1.5f, () =>
        { 
            win.SetActive(true);
            var joystick = FindObjectOfType<FloatingJoystick>();
            joystick.OnPointerUp(null);
            joystick.enabled = false;
            Stack.instance.Finish();
        });
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // fix it for now only one scene
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }

    public void Deform()
    {
        for (var i = 0; i < GrassRenderers.Length; i++)
        {
            if (GrassRenderers[i].CanDeform)
            {
                GrassRenderers[i].Deform();
            }
        }
    }

    public IRenderer CurrentRenderer()
    {
        return GrassRenderers[Mathf.Min(StageIndex, GrassRenderers.Length-1)];
    }

    public Material GetStackMaterial()
    {
        return instance.GrassRenderers[StageIndex].stackMaterial;
    }

    public void IncreaseDeformArea(float value)
    {
        Moni.instance.IncreaseColliderRadius(value);
        playerRange += value;
        foreach (var renderer in GrassRenderers)
        {
            renderer.playerRange = playerRange;
        }
    }
}
