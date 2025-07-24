using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public readonly string menu = "MainMenu";
    public readonly string map = "Map";
    public readonly string escapeScreen = "EscapeScreen";
    public readonly string credits = "Credits";
    
    #region Singleton

    public static LevelLoader Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(LevelLoader)) as LevelLoader;

            return _instance;
        }
        set { _instance = value; }
    }

    private static LevelLoader _instance;

    #endregion

    public void LoadLevel(string levelName)
    {
        StartCoroutine(DoTransition(levelName));
    }

    private IEnumerator DoTransition(string levelName)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelName);
    }
}
