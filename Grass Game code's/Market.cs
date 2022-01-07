
using AnilTools;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NaughtyAttributes;

public enum UpgradePart  { leaf , grass, snow , cleaner , wheels}

public class Market : MonoBehaviour
{
    public static Market instance;

    public UpgradeTree leafLevels   ;
    public UpgradeTree grassLevels  ;
    public UpgradeTree snowLevels   ;
    public UpgradeTree cleanerLevels;
    public UpgradeTree wheelLevels  ;

    public ParticleSystem UpgradeParticle;

    public GameObject PlayerUI, MarketUI;
    public Text MoneyNotEnoughText;
    public Text LevelText;
    public UpgradePart startMachine;

    private UpgradeTree startTree;
    private UpgradeTree CurrentTree;

    public Sprite Max, upGreen;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        LevelText.text = "";
    }

    private void Start()
    {
        startTree = GetTree(startMachine);
        CurrentTree = startTree;
#if UNITY_EDITOR
        // Stack.AddMoney(500000);
#endif
        Stack.AddMoney(500);
        
        string[] upgradeNames = Enum.GetNames(typeof(UpgradePart));
        
        foreach (var name in upgradeNames)
        {
            UpgradePart partType = (UpgradePart)Enum.Parse(typeof(UpgradePart), name);
            UpgradeTo(partType, PlayerPrefs.GetInt( name + Application.version.ToString() ));
        }

        int level = Mathf.Max(0, PlayerPrefs.GetInt("Level" + Application.version.ToString()));

        gameObject.SetActive(false);
        
        wheelLevels.SetSprite(upGreen);
        //  if (level >= 0) grassLevels  .SetSprite(upGreen);
        //  if (level > 0) cleanerLevels.SetSprite(upGreen);
    }


    [ContextMenu("Test")]
    public void Test()
    {
        string[] upgradeNames = Enum.GetNames(typeof(UpgradePart));

        foreach (var name in upgradeNames)
        {
            Debug.Log(Enum.TryParse<UpgradePart>(name, out var part));
            Debug.Log(PlayerPrefs.GetInt(name + Application.version.ToString()));
            Debug.Log(Enum.GetName(typeof(UpgradePart), part) + Application.version.ToString());
        }
    }


#if UNITY_EDITOR
    private void OnApplicationQuit() { PlayerPrefs.DeleteAll(); }
#endif

    public void UpgradeTo(UpgradePart part, int level)
    {
        for (var i = 0; i < level; i++) GetTree(part).Upgrade(false, part);
    }

    public void Upgrade(UpgradePart part)
    {
        var tree = GetTree(part);

        if (Stack.Money >= tree.GetCurrentPrice())
        {
            if (!tree.Upgrade(true, part))
            {
                // save current level of upgradePart
                UpgradeParticle.Play();
            }
        }
        else
        {
            if (MoneyNotEnoughText)
            {
                MoneyNotEnoughText.enabled = true;
                this.Delay(1f, () => MoneyNotEnoughText.enabled = false );
            }
        }
        // LevelText.text = $"Level {tree.currentLevel}";
    }

    public void OnMenuChanged(UpgradePart part)
    { 
        if (CurrentTree.mainParent != null) CurrentTree.mainParent.SetActive(false); // deactivate old machine ie = grass | leaf machine
        CurrentTree = GetTree(part);
        // LevelText.text = $"Level { CurrentTree.currentLevel} ";
        if (CurrentTree.mainParent) CurrentTree.mainParent.SetActive(true); // activate machine 
    }

    public UpgradeTree GetTree(UpgradePart part)
    {
        switch (part)
        {
            case UpgradePart.leaf:    return leafLevels   ;
            case UpgradePart.grass:   return grassLevels  ;
            case UpgradePart.snow:    return snowLevels   ;
            case UpgradePart.cleaner: return cleanerLevels;
            case UpgradePart.wheels:  return wheelLevels  ;
            default: throw new InvalidOperationException("upgrade part is invaild");
        }
    }

    public static bool UIOpen;

    public void SetUI(bool value)
    {
        PlayerUI.SetActive(!value);
        MarketUI.SetActive(value);
        CameraController.instance.SetZoom(value);

        if (value == false)
        {
            MiddleGames.Misc.Mono.instance.ExitUpgrade();
            OnMenuChanged(startMachine);
        }
        UIOpen = value;
    }

    // button events
    [ContextMenu("SetUITrue ")] public void SetUITrue () => SetUI(true);
    [ContextMenu("UpgradeGrass")] public void UpgradeGrass() { Upgrade(UpgradePart.grass  ); }
    [ContextMenu("UpgradeLeaf ")] public void UpgradeLeaf () { Upgrade(UpgradePart.leaf   ); }
    [ContextMenu("UpgradeSnow ")] public void UpgradeSnow () { Upgrade(UpgradePart.snow   ); }
    [ContextMenu("UpgradeClean")] public void UpgradeClean() { Upgrade(UpgradePart.cleaner); }
    [ContextMenu("UpgradeClean")] public void UpgradeWheel() { Upgrade(UpgradePart.wheels ); }
    
    [ContextMenu("MenuChangedGrass")] public void MenuChangedGrass() { OnMenuChanged(UpgradePart.grass  ); }
    [ContextMenu("MenuChangedLeaf ")] public void MenuChangedLeaf () { OnMenuChanged(UpgradePart.leaf   ); }
    [ContextMenu("MenuChangedSnow ")] public void MenuChangedSnow () { OnMenuChanged(UpgradePart.snow   ); }
    [ContextMenu("MenuChangedClean")] public void MenuChangedClean() { OnMenuChanged(UpgradePart.cleaner); }
    [ContextMenu("MenuChangedWheel")] public void MenuChangedWheel() { /* OnMenuChanged(UpgradePart.wheels); */ }
}

[Serializable]
public class UpgradeTree
{
    public UpgradeLevel[] levels;
    public GameObject mainParent;
    public Slider slider;

    [ReadOnly]
    public int currentLevel;
    
    public int GetCurrentPrice() => levels[Mathf.Min(currentLevel, levels.Length-1)].price;
    public bool IsMaxLevel() => currentLevel == levels.Length;

    public void SetSprite(Sprite sprite)
    { 
        slider.transform.parent.GetChild(2).GetComponent<Image>().sprite = sprite;
        slider.transform.parent.GetChild(2).GetComponent<Button>().enabled = sprite.name == "up_open";
    }

    /// <returns> is max level </returns>
    public bool Upgrade(bool removeMoney, UpgradePart part)
    {
        levels[currentLevel].Apply(removeMoney);
        ++currentLevel;
        slider.value = currentLevel / (float)levels.Length;
        PlayerPrefs.SetInt(Enum.GetName(typeof(UpgradePart), part) + Application.version.ToString(), currentLevel);
        if (IsMaxLevel())
        {
            currentLevel = Mathf.Min(currentLevel, levels.Length-1);
            SetSprite(Market.instance.Max);
            return true;
        }
        return false;
    }
}

[Serializable]
public struct UpgradeLevel
{
    public int price;
    public GameObject[] thingsToOpen;
    public GameObject[] thingsToClose;
    public UnityEvent OnUpgrade; // upgrade speed capacity etc. call other scripts here 

    public void Apply(bool removeMoney = true)
    {
        if (removeMoney) Stack.RemoveMoney(price);
        
        foreach (var thing in thingsToOpen)  thing.SetActive(true);
        foreach (var thing in thingsToClose) thing.SetActive(false);

        if (OnUpgrade != null) OnUpgrade.Invoke();
    }
}