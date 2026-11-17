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

    [System.Serializable]
    public class TutorialHusband
    {
        public Animator animator;
        public string comeTrigger;
        public string leaveTrigger = "TutorialEnd";
    }

    [System.Serializable]
    public class TutorialEyeline
    {
        public Animator animator; // triggers "not_ok" then "ok"
        public GameObject gameObject;
    }

    [System.Serializable]
    public class TutorialStep
    {
        public float comeDuration = 1f; // time for the husbands to arrive and stop
        public GameObject tapHere; // prompt shown while we wait for the press
        public Image tapHerePanel; // always enabled, only its alpha changes along with the prompt
    }

    public float tapHerePanelAlpha = 0.5f; // panel alpha while the prompt shows, 0 otherwise

    [Header("Tutorial")]
    public TutorialHusband rightHusband = new TutorialHusband { comeTrigger = "Hubby1" };
    public TutorialHusband leftHusband = new TutorialHusband { comeTrigger = "Hubby2" };
    public TutorialEyeline rightEyeline;
    public TutorialEyeline leftEyeline;

    public TutorialStep rightStep; // right husband + right eyeline
    public TutorialStep leftStep; // left husband + left eyeline
    public TutorialStep hideStep; // both husbands + both eyelines

    public float initialDelay = 1f;
    public float delayBetweenSteps = 0.5f;
    public float leaveDelay = 1f; // time between the "ok" and the husband leaving
    public int nextSceneId = -1; // -1 = next scene in the build settings

    public GameObject eyeSightPanel; // shown before the right / left steps, closed by CloseEyeSightPanel()
    public GameObject printMelusinePanel; // closed by ClosePrintMelusinePanel()
    public GameObject readyPanel;

    public AudioSource audioSource;
    public AudioClip[] popSounds; // one of them is played when a prompt or a panel shows up

    public bool isAskingForLeft = false;
    public bool hasPressedLeft = false;
    public bool isAskingForRight = false;
    public bool hasPressedRight = false;
    public bool isAskingForHide = false;
    public bool hasPressedHide = false;
    public bool tutorialDone = false;

    bool eyeSightPanelClosed = false;
    bool printPanelClosed = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (TutorialStep step in new[] { rightStep, leftStep, hideStep })
        {
            ShowTapHere(step, false);
        }
        rightEyeline.gameObject.SetActive(false);
        leftEyeline.gameObject.SetActive(false);
        eyeSightPanel.SetActive(false);
        printMelusinePanel.SetActive(false);
        readyPanel.SetActive(false);

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

        // EyeSight panel
        eyeSightPanel.SetActive(true);
        PlayPopSound();
        yield return new WaitUntil(() => eyeSightPanelClosed);

        // Right
        yield return RunStep(rightStep, new[] { rightHusband }, new[] { rightEyeline }, v => isAskingForRight = v, () => hasPressedRight);
        yield return new WaitForSeconds(delayBetweenSteps);

        // Left
        yield return RunStep(leftStep, new[] { leftHusband }, new[] { leftEyeline }, v => isAskingForLeft = v, () => hasPressedLeft);
        yield return new WaitForSeconds(delayBetweenSteps);

        // Hide
        yield return RunStep(hideStep, new[] { rightHusband, leftHusband }, new[] { rightEyeline, leftEyeline }, v => isAskingForHide = v, () => hasPressedHide);
        yield return new WaitForSeconds(0.1f);

        // Print Melusine panel
        printMelusinePanel.SetActive(true);
        PlayPopSound();
        yield return new WaitUntil(() => printPanelClosed);

        // Ready? panel, press anywhere to start the game
        readyPanel.SetActive(true);
        PlayPopSound();
        yield return WaitForAnyPress();

        tutorialDone = true;
        Debug.Log("Tutorial done");

        PlayerPrefs.SetInt(MainMenuManager.TutorialSeenKey, 1);
        PlayerPrefs.Save(); // needed on WebGL to write it to the browser storage right away

        if (nextSceneId >= 0)
        {
            SceneManager.LoadScene(nextSceneId);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    // Husbands come and stop, eyelines say "not_ok", we wait for the press, eyelines say "ok", husbands leave
    IEnumerator RunStep(TutorialStep step, TutorialHusband[] husbands, TutorialEyeline[] eyelines, System.Action<bool> setAsking, System.Func<bool> hasPressed)
    {
        foreach (TutorialHusband husband in husbands)
            husband.animator.SetTrigger(husband.comeTrigger);
        yield return new WaitForSeconds(step.comeDuration);

        foreach (TutorialEyeline eyeline in eyelines)
        {
            if (eyeline.gameObject != null) eyeline.gameObject.SetActive(true);
            eyeline.animator.SetTrigger("not_ok");
        }

        yield return new WaitForSeconds(0.5f); // so the "not_ok" animation is visible for a short time before the prompt shows up
        ShowTapHere(step, true);
        PlayPopSound();

        setAsking(true);
        yield return new WaitUntil(hasPressed);
        setAsking(false);

        ShowTapHere(step, false);
        foreach (TutorialEyeline eyeline in eyelines)
            eyeline.animator.SetTrigger("ok");

        yield return new WaitForSeconds(leaveDelay);
        foreach (TutorialHusband husband in husbands)
            husband.animator.SetTrigger(husband.leaveTrigger);
        foreach (TutorialEyeline eyeline in eyelines)
            if (eyeline.gameObject != null) eyeline.gameObject.SetActive(false);
    }

    // Tap here prompt is toggled, its panel image stays enabled and only its alpha changes
    void ShowTapHere(TutorialStep step, bool show)
    {
        if (step.tapHere != null) step.tapHere.SetActive(show);
        if (step.tapHerePanel != null)
        {
            Color color = step.tapHerePanel.color;
            color.a = show ? tapHerePanelAlpha : 0f;
            step.tapHerePanel.color = color;
        }
    }

    // Buttons: connect them in the Inspector (Button > OnClick)
    public void CloseEyeSightPanel()
    {
        eyeSightPanel.SetActive(false);
        eyeSightPanelClosed = true;
    }

    public void ClosePrintMelusinePanel()
    {
        printMelusinePanel.SetActive(false);
        printPanelClosed = true;
    }

    IEnumerator WaitForAnyPress()
    {
        yield return null; // so the press that closed the previous step isn't counted
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0); // anyKeyDown also counts mouse clicks
    }

    void PlayPopSound()
    {
        audioSource.PlayOneShot(popSounds[Random.Range(0, popSounds.Length)]);
    }

    #endregion
}
