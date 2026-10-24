using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Put on a UI Button: clears the saved image pool so every image can be drawn again.
/// </summary>
[RequireComponent(typeof(Button))]
public class ResetPoolButton : MonoBehaviour
{
    [Tooltip("Reload the current scene after resetting, so the scanner draws a fresh image from the full pool.")]
    [SerializeField] bool reloadScene = true;

    void Awake() => GetComponent<Button>().onClick.AddListener(ResetPool);

    void ResetPool()
    {
        ImageScanner.ResetPool();
        if (reloadScene)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
