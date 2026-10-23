using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum PeekDirection { NorthWest, NorthEast, SouthWest, SouthEast }

[System.Serializable]
public class PeekLane
{
    [Tooltip("Show a warning mark before the peeker walks in (used for the south lanes, which come from off screen)")]
    public bool showWarning;
    public Vector3 warningPosition;
    public Vector3 spawnPosition;
    public Vector3 peekPosition;
}

public class GameController : MonoBehaviour
{
    [Header("Melusine")]
    public SpriteRenderer MelusineRenderer;
    public Sprite NeutralSprite;
    [Tooltip("Pose per covered direction, in PeekDirection order: NW (up+left), NE (up+right), SW (down+left), SE (down+right)")]
    public Sprite[] CoverSprites = new Sprite[4];

    [Header("Peekers")]
    [Tooltip("Husband and anyone else who comes to peek. Sprites should face right (coming from the west); east peekers get flipped.")]
    public GameObject[] PeekerPrefabs;
    [Tooltip("Optional. If empty a red \"!!!\" text is used.")]
    public GameObject WarningPrefab;

    public PeekLane NorthWest = new PeekLane { spawnPosition = new Vector3(-2.5f, 2.5f), peekPosition = new Vector3(-1f, 2.5f) };
    public PeekLane NorthEast = new PeekLane { spawnPosition = new Vector3(2.5f, 2.5f), peekPosition = new Vector3(1f, 2.5f) };
    public PeekLane SouthWest = new PeekLane { showWarning = true, warningPosition = new Vector3(-8f, -4f), spawnPosition = new Vector3(-10f, -6f), peekPosition = new Vector3(-6.5f, -3.5f) };
    public PeekLane SouthEast = new PeekLane { showWarning = true, warningPosition = new Vector3(8f, -4f), spawnPosition = new Vector3(10f, -6f), peekPosition = new Vector3(6.5f, -3.5f) };

    [Header("Timing (seconds)")]
    public float MinDelay = 1f;
    public float MaxDelay = 1.5f;
    public float WarningDuration = 0.4f;
    public float MoveInDuration = 0.5f;
    [Tooltip("How long the peeker stays at the peek position")]
    public float PeekDuration = 0.4f;
    [Tooltip("How late after the peeker arrives the player can still press the right combo")]
    public float GraceWindow = 0.2f;
    public float MoveOutDuration = 0.4f;
    [Tooltip("Max time between the two arrow presses for them to count as one combo")]
    public float ChordWindow = 0.12f;

    public UnityEvent OnSeen;

    // Direction Melusine is leaning towards with her body, set by the last combo pressed
    PeekDirection? cover;

    // Last arrow press on each axis, to pair them into a combo
    bool lastUp, lastLeft;
    float lastVerticalTime = float.NegativeInfinity;
    float lastHorizontalTime = float.NegativeInfinity;

    void Start()
    {
        UpdatePose();
        StartCoroutine(PeekLoop());
    }

    void Update()
    {
        bool vertical = false, horizontal = false;

        if (Input.GetKeyDown(KeyCode.UpArrow)) { lastUp = true; vertical = true; }
        if (Input.GetKeyDown(KeyCode.DownArrow)) { lastUp = false; vertical = true; }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { lastLeft = true; horizontal = true; }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { lastLeft = false; horizontal = true; }

        if (vertical) lastVerticalTime = Time.time;
        if (horizontal) lastHorizontalTime = Time.time;
        if (!vertical && !horizontal) return;

        // A combo is one press on each axis close together
        if (Mathf.Abs(lastVerticalTime - lastHorizontalTime) <= ChordWindow)
        {
            cover = ComboDirection(lastUp, lastLeft);
            UpdatePose();
        }
    }

    #region PlayerController

    static PeekDirection ComboDirection(bool up, bool left)
    {
        if (up) return left ? PeekDirection.NorthWest : PeekDirection.NorthEast;
        return left ? PeekDirection.SouthWest : PeekDirection.SouthEast;
    }

    void UpdatePose()
    {
        if (MelusineRenderer == null) return;

        Sprite sprite = cover.HasValue ? CoverSprites[(int)cover.Value] : NeutralSprite;
        if (sprite != null) MelusineRenderer.sprite = sprite;
    }

    #endregion

    #region Peekers

    static PeekDirection Opposite(PeekDirection dir)
    {
        switch (dir)
        {
            case PeekDirection.NorthWest: return PeekDirection.SouthEast;
            case PeekDirection.NorthEast: return PeekDirection.SouthWest;
            case PeekDirection.SouthWest: return PeekDirection.NorthEast;
            default: return PeekDirection.NorthWest;
        }
    }

    static bool IsEast(PeekDirection dir) => dir == PeekDirection.NorthEast || dir == PeekDirection.SouthEast;

    PeekLane Lane(PeekDirection dir)
    {
        switch (dir)
        {
            case PeekDirection.NorthWest: return NorthWest;
            case PeekDirection.NorthEast: return NorthEast;
            case PeekDirection.SouthWest: return SouthWest;
            default: return SouthEast;
        }
    }

    IEnumerator PeekLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(MinDelay, MaxDelay));
            yield return Peek((PeekDirection)Random.Range(0, 4));
        }
    }

    IEnumerator Peek(PeekDirection dir)
    {
        PeekLane lane = Lane(dir);

        if (lane.showWarning)
        {
            GameObject warning = SpawnWarning(lane.warningPosition);
            Vector3 baseScale = warning.transform.localScale;
            warning.transform.localScale = Vector3.zero;
            warning.transform.DOScale(baseScale, 0.15f).SetEase(Ease.OutBack).SetLink(warning);
            warning.transform.DOScale(baseScale * 1.2f, 0.15f).SetDelay(0.15f).SetLoops(-1, LoopType.Yoyo).SetLink(warning);
            yield return new WaitForSeconds(WarningDuration);
            Destroy(warning);
        }

        GameObject prefab = PeekerPrefabs[Random.Range(0, PeekerPrefabs.Length)];
        GameObject peeker = Instantiate(prefab, lane.spawnPosition, Quaternion.identity);
        if (IsEast(dir))
        {
            Vector3 scale = peeker.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            peeker.transform.localScale = scale;
        }

        yield return peeker.transform.DOMove(lane.peekPosition, MoveInDuration).SetEase(Ease.OutQuad).SetLink(peeker).WaitForCompletion();

        // Safe if she's covering the right way on arrival or within the grace window after
        PeekDirection safeCover = Opposite(dir);
        float t = 0f;
        while (cover != safeCover && t < GraceWindow)
        {
            yield return null;
            t += Time.deltaTime;
        }
        if (cover != safeCover) Seen(peeker);

        if (t < PeekDuration) yield return new WaitForSeconds(PeekDuration - t);

        yield return peeker.transform.DOMove(lane.spawnPosition, MoveOutDuration).SetEase(Ease.InQuad).SetLink(peeker).WaitForCompletion();
        Destroy(peeker);
    }

    GameObject SpawnWarning(Vector3 position)
    {
        if (WarningPrefab != null) return Instantiate(WarningPrefab, position, Quaternion.identity);

        var go = new GameObject("Warning");
        go.transform.position = position;
        var text = go.AddComponent<TextMeshPro>();
        text.text = "!!!";
        text.fontSize = 8;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.red;
        text.alignment = TextAlignmentOptions.Center;
        text.sortingOrder = 100;
        return go;
    }

    void Seen(GameObject peeker)
    {
        Debug.Log("Melusine's tail was seen!");
        peeker.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(peeker);
        if (MelusineRenderer != null)
        {
            // Finish any previous shake first so overlapping shakes don't leave her off position
            MelusineRenderer.transform.DOComplete();
            MelusineRenderer.transform.DOShakePosition(0.3f, 0.2f);
        }
        OnSeen.Invoke();
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        foreach (PeekLane lane in new[] { NorthWest, NorthEast, SouthWest, SouthEast })
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lane.spawnPosition, lane.peekPosition);
            Gizmos.DrawWireSphere(lane.peekPosition, 0.25f);
            if (lane.showWarning)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(lane.warningPosition, 0.3f);
            }
        }
    }
}
