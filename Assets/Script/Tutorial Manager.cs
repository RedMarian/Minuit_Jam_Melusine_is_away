using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public Animator MelusineAnimator;

    public Transform CameraTransform;

    public float camPosX;
    public float lerpValue;
    public float maxCamPosX;

    [Header("Tutorial")]
    public GameObject leftTapHere;
    public GameObject rightTapHere;
    public GameObject hideTapHere;

    public Image leftButtonImage;
    public Image rightButtonImage;
    public Image hideButtonImage;
    public float dimmedAlpha = 0.3f; // alpha of the buttons we are not asking for
    public float highlightAlpha = 0.7f; 

    public float initialDelay = 1f;
    public float delayBetweenSteps = 0.5f;
    public int nextSceneId = -1; // -1 = next scene in the build settings

    public GameObject readyText;

    public AudioSource audioSource;
    public AudioClip[] popSounds; // one of them is played when a Tap Here or the ready text shows up

    public bool isAskingForLeft = false;
    public bool hasPressedLeft = false;
    public bool isAskingForRight = false;
    public bool hasPressedRight = false;
    public bool isAskingForHide = false;
    public bool hasPressedHide = false;
    public bool tutorialDone = false;

    // Start is called before the first frame update
    void Start()
    {
        leftTapHere.SetActive(false);
        rightTapHere.SetActive(false);
        hideTapHere.SetActive(false);
        readyText.SetActive(false);

        StartCoroutine(RunTutorial());
        camPosX = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        CameraTransform.position = new Vector3(camPosX, CameraTransform.position.y, CameraTransform.position.z);
        camPosX = Mathf.Lerp(camPosX, maxCamPosX, lerpValue);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MaxCamPosXDef(-1.5f);
            PressingLeft(true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            PressingLeft(false);
            MaxCamPosXDef(0f);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MaxCamPosXDef(1.5f);
            PressingRight(true);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            PressingRight(false);
            MaxCamPosXDef(0f);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Hide();
        }
    }

    #region PlayerController

    // Only the button we are asking for works (all of them once the tutorial is done)
    bool CanPressLeft() { return tutorialDone || isAskingForLeft; }
    bool CanPressRight() { return tutorialDone || isAskingForRight; }
    bool CanPressHide() { return tutorialDone || isAskingForHide; }

    public void PressingLeft(bool state)
    {
        if (state && !CanPressLeft()) return; // releasing is always allowed
        MelusineAnimator.SetBool("LeftIsDown", state);
        if (state && isAskingForLeft) hasPressedLeft = true;
    }

    public void PressingRight(bool state)
    {
        if (state && !CanPressRight()) return;
        MelusineAnimator.SetBool("RightIsDown", state);
        if (state && isAskingForRight) hasPressedRight = true;
    }

    public void Hide()
    {
        if (!CanPressHide()) return;
        MelusineAnimator.SetTrigger("Hide");
        if (isAskingForHide) hasPressedHide = true;
    }

    public void MaxCamPosXDef(float target)
    {
        // going back to the center is always allowed
        if (target < 0f && !CanPressLeft()) return;
        if (target > 0f && !CanPressRight()) return;
        maxCamPosX = target;
    }

    #endregion

    #region Tutorial

    IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(initialDelay);

        // Right
        isAskingForRight = true;
        HighlightButton(rightButtonImage);
        rightTapHere.SetActive(true);
        PlayPopSound();
        yield return new WaitUntil(() => hasPressedRight);
        isAskingForRight = false;
        rightTapHere.SetActive(false);
        yield return new WaitForSeconds(delayBetweenSteps);

        // Left
        isAskingForLeft = true;
        HighlightButton(leftButtonImage);
        leftTapHere.SetActive(true);
        PlayPopSound();
        yield return new WaitUntil(() => hasPressedLeft);
        isAskingForLeft = false;
        leftTapHere.SetActive(false);
        yield return new WaitForSeconds(delayBetweenSteps);

        // Hide
        isAskingForHide = true;
        HighlightButton(hideButtonImage);
        hideTapHere.SetActive(true);
        PlayPopSound();
        yield return new WaitUntil(() => hasPressedHide);
        isAskingForHide = false;
        hideTapHere.SetActive(false);
        yield return new WaitForSeconds(delayBetweenSteps);

        // Ready, press anywhere to start the game
        readyText.SetActive(true);
        PlayPopSound();
        SetAlpha(leftButtonImage, dimmedAlpha);
        SetAlpha(rightButtonImage, dimmedAlpha);
        SetAlpha(hideButtonImage, dimmedAlpha);
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0); // anyKeyDown also counts mouse clicks

        tutorialDone = true;
        Debug.Log("Tutorial done");

        if (nextSceneId >= 0)
        {
            SceneManager.LoadScene(nextSceneId);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Full color on the button we ask for, dimmed on the others (null = all full color)
    void HighlightButton(Image active)
    {
        SetAlpha(leftButtonImage, active == null || active == leftButtonImage ? highlightAlpha : dimmedAlpha);
        SetAlpha(rightButtonImage, active == null || active == rightButtonImage ? highlightAlpha : dimmedAlpha);
        SetAlpha(hideButtonImage, active == null || active == hideButtonImage ? highlightAlpha : dimmedAlpha);
    }

    void PlayPopSound()
    {
        audioSource.PlayOneShot(popSounds[Random.Range(0, popSounds.Length)]);
    }

    void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    #endregion
}
