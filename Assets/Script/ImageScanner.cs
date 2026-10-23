using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scans for one physical image with MindAR (WebGL only) and raises onImageFound when the camera sees it.
/// Requires the "MindAR" WebGL template and the compiled .mind file in Assets/StreamingAssets.
/// </summary>
public class ImageScanner : MonoBehaviour
{
    [Tooltip("Compiled MindAR target file, relative to Assets/StreamingAssets")]
    [SerializeField] string targetFile = "targets.mind";

    public UnityEvent onImageFound;
    public UnityEvent onScanCancelled;
    public UnityEvent<string> onScanError;

    public bool IsScanning { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void MindAR_StartScan(string receiverName, string targetUrl);
    [DllImport("__Internal")] static extern void MindAR_StopScan();
#else
    static void MindAR_StartScan(string receiverName, string targetUrl) => Debug.Log($"[Scanner] StartScan {targetUrl} (editor stub, use the context menu to simulate)");
    static void MindAR_StopScan() => Debug.Log("[Scanner] StopScan (editor stub)");
#endif

    // Call from a UI button: iOS only allows opening the camera after a user tap.
    public void BeginScan()
    {
        if (IsScanning) return;
        IsScanning = true;
        MindAR_StartScan(gameObject.name, Application.streamingAssetsPath + "/" + targetFile);
    }

    public void CancelScan()
    {
        if (!IsScanning) return;
        IsScanning = false;
        MindAR_StopScan();
    }

    void OnDisable() => CancelScan();

    // --- Called from JavaScript via SendMessage (names must match the template) ---

    void OnImageFound()
    {
        IsScanning = false;
        onImageFound.Invoke();
    }

    void OnScanCancelled()
    {
        IsScanning = false;
        onScanCancelled.Invoke();
    }

    void OnScanError(string message)
    {
        IsScanning = false;
        Debug.LogWarning($"[Scanner] {message}");
        onScanError.Invoke(message);
    }

    [ContextMenu("Simulate Image Found")]
    void SimulateImageFound() => OnImageFound();
}
