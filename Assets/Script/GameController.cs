using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public Animator MelusineAnimator;
    public Animator HubbyLeftAnimator;
    public Animator HubbyRightAnimator;

    public Transform CameraTransform;

    public float camPosX;
    public float lerpValue;
    public float maxCamPosX;
    public float maxCamTarget = 1.5f;

    public HubbyVariables Hubby1CanSee;
    public HubbyVariables Hubby2CanSee;

    public MelusionVariables MelusineVariables;
    
    public float Cleanliness = 0f;
    public float CleanTime = 60f;
    public Slider Progress;

    public enum Husband { None, Left, Right, Both }

    public float hubbyMinDelay = 1f;
    public float hubbyMaxDelay = 3.3f;
    public float hubbyRandomHusbandDelay = 2f; // delays below this force the other husband
    public float hubbyBothChance = 0.3f; // chance both come out after a hubbyRandomHusbandDelay
    public float hubbyFireDelay;
    private Husband lastHusband = Husband.None;
    private float lastFireTime;

    //public AudioSource sdAmb;
    public AudioSource sdsfx1;
    public AudioSource sdsfx2;
    
    private bool isGameOver = false;
    private bool isVictorious = false;

    private float t;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Dance());
        camPosX = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver || isVictorious) return;
        switch (MelusineVariables.MelusinePosition)
        {
            case 1 when Hubby1CanSee.HubbyCanSee:
            case 2 when Hubby2CanSee.HubbyCanSee:
            case 4 when (Hubby1CanSee.HubbyCanSee || Hubby2CanSee.HubbyCanSee):
                GameOver();
                sdsfx2.Play();
                break;
            default:
            {
                if (Cleanliness >= CleanTime)
                {
                    Victory();
                    sdsfx1.Play();
                }
                else
                {
                    Cleanliness += Time.deltaTime;
                    Progress.value = Cleanliness;
                }

                break;
            }
        }

        CameraTransform.position = new Vector3(camPosX, CameraTransform.position.y, CameraTransform.position.z);
        t += Time.deltaTime * lerpValue;
        camPosX = Mathf.Lerp(camPosX, maxCamPosX, t);
        //Debug.Log("I'm live");
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PressingLeft(true);
            //Debug.Log("PressingLeft");
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            PressingLeft(false);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PressingRight(true);
            //Debug.Log("PressingRight");
            
        } else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            PressingRight(false);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Hide();
        }
    }

    #region PlayerController

    public void PressingLeft(bool state)
    {
        MelusineAnimator.SetBool("LeftIsDown", state);
        var target = state ? -maxCamTarget : 0f;
        MaxCamPosXDef(target);
        //CamLag(-1f, state);
        //Debug.Log("Melusine turns left"+state);
    }
    public void PressingRight(bool state)
    {
        MelusineAnimator.SetBool("RightIsDown", state);
        var target = state ? maxCamTarget : 0f;
        MaxCamPosXDef(target);
        //(1f, state);
        //Debug.Log("Melusine turns right"+state);
    }

    public void Hide()
    {
        MelusineAnimator.SetTrigger("Hide");
    }
    
    private void MaxCamPosXDef(float target)
    {
        maxCamPosX =target;
        t = 0f;
    }

    #endregion
    
    #region Hubbies

    void HubbyLeftFire()
    {
        lastHusband = Husband.Left;
        HubbyLeftAnimator.SetTrigger("Hubby2");
    }

    void HubbyRightFire()
    {
        lastHusband = Husband.Right;
        HubbyRightAnimator.SetTrigger("Hubby1");
    }

    void HubbyBothFire()
    {
        HubbyLeftFire();
        HubbyRightFire();
        lastHusband = Husband.Both;
    }

    void HubbyLeftLeaves()
    {
        HubbyLeftAnimator.SetTrigger("Hubby2Leave");
    }

    void HubbyRightLeaves()
    {
        HubbyRightAnimator.SetTrigger("Hubby1Leave");
    }

    bool IsHubbyIdle(Animator animator, string sleepingState)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(sleepingState) && !animator.IsInTransition(0);
    }

    bool CanFire(Husband husband)
    {
        bool leftIdle = IsHubbyIdle(HubbyLeftAnimator, "HubbyLeft_sleeping");
        bool rightIdle = IsHubbyIdle(HubbyRightAnimator, "HubbyRight_sleeping");

        if (husband == Husband.Left) return leftIdle;
        if (husband == Husband.Right) return rightIdle;
        return leftIdle && rightIdle;
    }

    Husband NextHusband()
    {
        // short delays force the other husband, long delays can bring out both
        if (hubbyFireDelay < hubbyRandomHusbandDelay)
        {
            if (lastHusband == Husband.Left) return Husband.Right;
            if (lastHusband == Husband.Right) return Husband.Left;
        }
        else if (Random.value < hubbyBothChance)
        {
            return Husband.Both;
        }
        // first husband, or after both came out
        return Random.Range(0, 2) == 0 ? Husband.Left : Husband.Right;
    }

    IEnumerator Dance()
    {
        while (true)
        {
            // gap is measured from when the last husband came out
            hubbyFireDelay = Random.Range(hubbyMinDelay, hubbyMaxDelay);
            yield return new WaitUntil(() => Time.time - lastFireTime >= hubbyFireDelay);

            // only fire husbands who are back to sleeping, otherwise the trigger stays queued
            // and he walks out again the moment he gets back
            Husband next = NextHusband();
            while (!CanFire(next))
            {
                yield return null;
            }

            if (next == Husband.Left)
            {
                HubbyLeftFire();
            }
            else if (next == Husband.Right)
            {
                HubbyRightFire();
            }
            else
            {
                HubbyBothFire();
            }
            lastFireTime = Time.time;
            //Debug.Log(hubbyFireDelay + " " + lastHusband);
        }
    }

    #endregion
    
    public void GameOver()
    {
        StopAllCoroutines();
        MelusineAnimator.SetTrigger("GameOver");
        isGameOver = true;
        HubbyLeftAnimator.SetBool("SeeSnake", true);
        HubbyRightAnimator.SetBool("SeeSnake", true);
        //Debug.Log("Game Over");
        // Make PopIn appear
    }

    public void Victory()
    {
        StopAllCoroutines();
        MelusineAnimator.SetTrigger("Victory");
        isVictorious = true;
        //Debug.Log("Victory");
        // Make PopIn apear
    }
}
