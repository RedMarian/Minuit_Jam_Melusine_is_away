using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    public void LoadSceneMelu(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }
}
