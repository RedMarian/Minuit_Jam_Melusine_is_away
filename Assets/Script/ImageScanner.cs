using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Scans for one physical image with MindAR (WebGL only) and raises onImageFound when the camera sees it.
/// The image is drawn at random from a pool of targetFiles each time a scan starts. Drawn images are
/// removed from the pool and remembered in PlayerPrefs (browser storage on WebGL); once every image
/// has been drawn, the pool refills.
/// Requires the "MindAR" WebGL template and the compiled .mind files in Assets/StreamingAssets.
/// </summary>
public class ImageScanner : MonoBehaviour
{
    const string UsedKey = "ImageScanner.UsedTargets";
    const char Separator = '|';

    [Tooltip("Compiled MindAR target files, relative to Assets/StreamingAssets. Drawn at random without repeats.")]
    [SerializeField] string[] targetFiles = { "melusine1.mind", "melusine2.mind" };

    public UnityEvent onImageFound;
    public UnityEvent onScanCancelled;
    public UnityEvent<string> onScanError;

    public bool IsScanning { get; private set; }

    /// <summary>The randomly chosen image, 1-based (1 = first entry in targetFiles). 0 until the first scan.</summary>
    public int ChosenNumber { get; private set; }

    string ChosenFile => targetFiles[ChosenNumber - 1];

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void MindAR_StartScan(string receiverName, string targetUrl);
    [DllImport("__Internal")] static extern void MindAR_StopScan();
#else
    static void MindAR_StartScan(string receiverName, string targetUrl) => Debug.Log($"[Scanner] StartScan {targetUrl} (editor stub, use the context menu to simulate)");
    static void MindAR_StopScan() => Debug.Log("[Scanner] StopScan (editor stub)");
#endif

    // Called by BeginScan: every scan draws a new image from the pool.
    public void DrawFromPool()
    {
        var used = LoadUsed();
        var remaining = targetFiles.Where(f => !used.Contains(f)).ToList();
        if (remaining.Count == 0)
        {
            Debug.Log("[Scanner] Pool empty, refilling");
            used.Clear();
            remaining = targetFiles.ToList();
        }

        string file = remaining[Random.Range(0, remaining.Count)];
        ChosenNumber = System.Array.IndexOf(targetFiles, file) + 1;

        used.Add(file);
        SaveUsed(used);
        Debug.Log($"[Scanner] Chosen image {ChosenNumber} ({file}), {remaining.Count - 1} left in pool");
    }

    [ContextMenu("Reset Pool")]
    void ResetPoolFromMenu() => ResetPool();

    // Static so it can be called from any scene (e.g. a reset button in the main menu).
    public static void ResetPool()
    {
        PlayerPrefs.DeleteKey(UsedKey);
        PlayerPrefs.Save();
        Debug.Log("[Scanner] Pool reset");
    }

    static HashSet<string> LoadUsed() =>
        new HashSet<string>(PlayerPrefs.GetString(UsedKey, "").Split(new[] { Separator }, System.StringSplitOptions.RemoveEmptyEntries));

    static void SaveUsed(HashSet<string> used)
    {
        PlayerPrefs.SetString(UsedKey, string.Join(Separator.ToString(), used));
        PlayerPrefs.Save(); // on WebGL this writes to the browser's IndexedDB
    }

    // Call from a UI button: iOS only allows opening the camera after a user tap.
    public void BeginScan()
    {
        if (IsScanning) return;
        IsScanning = true;
        DrawFromPool();
        MindAR_StartScan(gameObject.name, Application.streamingAssetsPath + "/" + ChosenFile);
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
