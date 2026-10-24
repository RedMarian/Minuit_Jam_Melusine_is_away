// Bridge between ImageScanner.cs and the MindAR scanner defined in the WebGL template (Assets/WebGLTemplates/MindAR).
mergeInto(LibraryManager.library, {
  // receiverName: GameObject that gets OnImageFound / OnScanError / OnScanCancelled via SendMessage
  // targetUrl: URL of the compiled .mind file
  // targetCount: number of images compiled into that file (any one of them counts as found)
  MindAR_StartScan: function (receiverNamePtr, targetUrlPtr, targetCount) {
    var receiverName = UTF8ToString(receiverNamePtr);
    var targetUrl = UTF8ToString(targetUrlPtr);
    if (typeof window.mindarStartScan !== "function") {
      console.error("[Scanner] window.mindarStartScan missing - is the MindAR WebGL template selected?");
      SendMessage(receiverName, "OnScanError", "MindAR template not loaded");
      return;
    }
    window.mindarStartScan(receiverName, targetUrl, targetCount);
  },

  MindAR_StopScan: function () {
    if (typeof window.mindarStopScan === "function") window.mindarStopScan();
  },
});
