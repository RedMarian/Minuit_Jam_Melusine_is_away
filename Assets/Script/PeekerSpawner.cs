using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PeekerSpawner : MonoBehaviour
{
    public GameController Game;
    public MelusineController Melusine;

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

    [Header("Fakeouts")]
    [Tooltip("Chance per peek, once the game is in its fakeout phase, that the warning shows at the wrong corner")]
    [Range(0f, 1f)] public float FakeoutChance = 0.5f;

    void Start()
    {
        StartCoroutine(PeekLoop());
    }

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
            // Peekers already on their way finish, but no new ones after the round
            if (Game.IsRoundOver) yield break;

            var dir = (PeekDirection)Random.Range(0, 4);
            PeekDirection? warnAt = Lane(dir).showWarning ? dir : (PeekDirection?)null;

            // Fakeout: warn at a south corner, but the peeker comes from one of the other three directions
            if (Game.IsFakeoutPhase && Random.value < FakeoutChance)
            {
                PeekDirection fake = Random.value < 0.5f ? PeekDirection.SouthWest : PeekDirection.SouthEast;
                warnAt = fake;
                dir = (PeekDirection)(((int)fake + Random.Range(1, 4)) % 4);
            }

            yield return Peek(dir, warnAt);
        }
    }

    IEnumerator Peek(PeekDirection dir, PeekDirection? warnAt)
    {
        PeekLane lane = Lane(dir);

        if (warnAt.HasValue)
        {
            GameObject warning = SpawnWarning(Lane(warnAt.Value).warningPosition);
            Vector3 baseScale = warning.transform.localScale;
            warning.transform.localScale = Vector3.zero;
            warning.transform.DOScale(baseScale, 0.15f).SetEase(Ease.OutBack).SetLink(warning);
            warning.transform.DOScale(baseScale * 1.2f, 0.15f).SetDelay(0.15f).SetLoops(-1, LoopType.Yoyo).SetLink(warning);
            yield return new WaitForSeconds(WarningDuration);
            Destroy(warning);
        }

        GameObject prefab = PeekerPrefabs[Random.Range(0, PeekerPrefabs.Length)];
        GameObject peeker = Instantiate(prefab, lane.spawnPosition, Quaternion.identity);
        if (dir.IsEast())
        {
            Vector3 scale = peeker.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            peeker.transform.localScale = scale;
        }

        yield return peeker.transform.DOMove(lane.peekPosition, MoveInDuration).SetEase(Ease.OutQuad).SetLink(peeker).WaitForCompletion();

        // Safe if she's covering the right way on arrival or within the grace window after
        PeekDirection safeCover = dir.Opposite();
        float t = 0f;
        while (Melusine.Cover != safeCover && t < GraceWindow)
        {
            yield return null;
            t += Time.deltaTime;
        }
        if (Melusine.Cover != safeCover)
        {
            peeker.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(peeker);
            Game.TailSeen();
        }

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
