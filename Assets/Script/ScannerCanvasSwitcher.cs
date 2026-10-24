using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the scanner canvas until the image is found, then swaps to the found canvas.
/// Reloads the scene when the player closes the scanner without finding the image.
/// </summary>
[RequireComponent(typeof(ImageScanner))]
public class ScannerCanvasSwitcher : MonoBehaviour
{
    [SerializeField] GameObject scannerCanvas;
    [SerializeField] GameObject foundCanvas;

    ImageScanner scanner;

    void Awake()
    {
        scanner = GetComponent<ImageScanner>();
        scannerCanvas.SetActive(true);
        foundCanvas.SetActive(false);
    }

    void OnEnable()
    {
        scanner.onImageFound.AddListener(ShowFound);
        scanner.onScanCancelled.AddListener(ReloadScene);
    }

    void OnDisable()
    {
        scanner.onImageFound.RemoveListener(ShowFound);
        scanner.onScanCancelled.RemoveListener(ReloadScene);
    }

    void ShowFound()
    {
        scannerCanvas.SetActive(false);
        foundCanvas.SetActive(true);
    }

    void ReloadScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
