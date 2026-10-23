using UnityEngine;

/// <summary>
/// Shows the scanner canvas until the image is found, then swaps to the found canvas.
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

    void OnEnable() => scanner.onImageFound.AddListener(ShowFound);
    void OnDisable() => scanner.onImageFound.RemoveListener(ShowFound);

    void ShowFound()
    {
        scannerCanvas.SetActive(false);
        foundCanvas.SetActive(true);
    }
}
