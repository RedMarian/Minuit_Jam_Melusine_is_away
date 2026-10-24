using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public const string TutorialSeenKey = "TutorialSeen"; // set to 1 by the TutorialManager once the tutorial is done

    public int tutorialSceneId = 4;
    public int gameSceneId = 1;

    public void Play()
    {
        // first time playing: go through the tutorial first
        if (PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0)
        {
            SceneManager.LoadScene(tutorialSceneId);
        }
        else
        {
            SceneManager.LoadScene(gameSceneId);
        }
    }

    public void Tutorial()
    {
        SceneManager.LoadScene(tutorialSceneId);
    }

    [ContextMenu("Reset Tutorial Seen")]
    void ResetTutorialSeen()
    {
        PlayerPrefs.DeleteKey(TutorialSeenKey);
        PlayerPrefs.Save();
        Debug.Log("Tutorial seen key reset");
    }
}
