using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    static bool firstStart = true;
    public Text levelText;

    private void Start()
    {
#if UNITY_EDITOR
        PlayerPrefs.SetInt("Level" + Application.version.ToString(), SceneManager.GetActiveScene().buildIndex);
        levelText.text = $"Level { SceneManager.GetActiveScene().buildIndex + 1}";
#else
        levelText.text = $"Level { (PlayerPrefs.GetInt("Level" + Application.version.ToString()) % SceneManager.sceneCountInBuildSettings) + 1}";
        if (firstStart)
        {
            firstStart = false;
            SceneManager.LoadScene(PlayerPrefs.GetInt("Level"+ Application.version.ToString()) % SceneManager.sceneCountInBuildSettings);
        }
#endif
    }


    [ContextMenu("Test")]
    public void Test()
    {
        NextLevel();
    }

    public static void NextLevel()
    {
        if (Stack.Selling) return;

        PlayerPrefs.SetInt("Level" + Application.version.ToString(), PlayerPrefs.GetInt("Level" + Application.version.ToString()) + 1);
        SceneManager.LoadScene( PlayerPrefs.GetInt("Level" + Application.version.ToString()) % (SceneManager.sceneCountInBuildSettings) );
    }

    public static void ReloadLevel()
    {   
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
