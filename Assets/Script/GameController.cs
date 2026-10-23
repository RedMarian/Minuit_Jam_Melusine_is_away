using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public MelusineController Melusine;

    [Header("Round")]
    public float RoundDuration = 60f;
    [Tooltip("UI Image with Image Type set to Filled")]
    public Image ProgressBar;
    [Tooltip("From this time on, warnings can lie about where the peeker comes from")]
    public float FakeoutStartTime = 45f;

    public UnityEvent OnSeen;
    public UnityEvent OnFakeoutPhase;
    public UnityEvent OnRoundEnd;

    public bool IsFakeoutPhase { get; private set; }
    public bool IsRoundOver { get; private set; }

    float elapsed;

    void Update()
    {
        if (IsRoundOver) return;

        elapsed = Mathf.Min(elapsed + Time.deltaTime, RoundDuration);
        if (ProgressBar != null) ProgressBar.fillAmount = elapsed / RoundDuration;

        if (!IsFakeoutPhase && elapsed >= FakeoutStartTime)
        {
            IsFakeoutPhase = true;
            Debug.Log("Fakeout phase started");
            OnFakeoutPhase.Invoke();
        }

        if (elapsed >= RoundDuration)
        {
            IsRoundOver = true;
            Debug.Log("Round over");
            OnRoundEnd.Invoke();
        }
    }

    public void TailSeen()
    {
        Debug.Log("Melusine's tail was seen!");
        if (Melusine != null) Melusine.Flinch();
        OnSeen.Invoke();
    }
}
