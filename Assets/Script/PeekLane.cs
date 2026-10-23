using UnityEngine;

[System.Serializable]
public class PeekLane
{
    [Tooltip("Show a warning mark before the peeker walks in (used for the south lanes, which come from off screen)")]
    public bool showWarning;
    public Vector3 warningPosition;
    public Vector3 spawnPosition;
    public Vector3 peekPosition;
}
