using DG.Tweening;
using UnityEngine;

public class MelusineController : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public Sprite NeutralSprite;
    [Tooltip("Pose per covered direction, in PeekDirection order: NW (up+left), NE (up+right), SW (down+left), SE (down+right)")]
    public Sprite[] CoverSprites = new Sprite[4];
    [Tooltip("Max time between the two arrow presses for them to count as one combo")]
    public float ChordWindow = 0.12f;

    // Direction Melusine is leaning towards with her body, set by the last combo pressed
    public PeekDirection? Cover { get; private set; }

    // Last arrow press on each axis, to pair them into a combo
    bool lastUp, lastLeft;
    float lastVerticalTime = float.NegativeInfinity;
    float lastHorizontalTime = float.NegativeInfinity;

    void Start()
    {
        UpdatePose();
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
            Cover = ComboDirection(lastUp, lastLeft);
            UpdatePose();
        }
    }

    static PeekDirection ComboDirection(bool up, bool left)
    {
        if (up) return left ? PeekDirection.NorthWest : PeekDirection.NorthEast;
        return left ? PeekDirection.SouthWest : PeekDirection.SouthEast;
    }

    void UpdatePose()
    {
        if (Renderer == null) return;

        Sprite sprite = Cover.HasValue ? CoverSprites[(int)Cover.Value] : NeutralSprite;
        if (sprite != null) Renderer.sprite = sprite;
    }

    public void Flinch()
    {
        // Finish any previous shake first so overlapping shakes don't leave her off position
        transform.DOComplete();
        transform.DOShakePosition(0.3f, 0.2f);
    }
}
