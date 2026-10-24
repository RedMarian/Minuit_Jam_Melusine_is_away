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
    public Husband lastHusband = Husband.None;

    //public AudioSource sdAmb;
    public AudioSource sdsfx1;
    public AudioSource sdsfx2;
    
    private bool isGameOver = false;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Dance());
        camPosX = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;
        if ((MelusineVariables.MelusinePosition == 1 && Hubby1CanSee.HubbyCanSee) || (MelusineVariables.MelusinePosition ==2 && Hubby2CanSee.HubbyCanSee))
        {
            GameOver();
            sdsfx2.Play();
        }
        else if (Cleanliness >= CleanTime)
        {
            Victory();
            sdsfx1.Play();
        }
        else
        {
            Cleanliness += Time.deltaTime;
            Progress.value = Cleanliness;
        }

        CameraTransform.position = new Vector3(camPosX, CameraTransform.position.y, CameraTransform.position.z);
        camPosX = Mathf.Lerp(camPosX, maxCamPosX, lerpValue);
        //Debug.Log("I'm live");
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MaxCamPosXDef(-1f);
            PressingLeft(true);
            //Debug.Log("PressingLeft");
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            PressingLeft(false);
            MaxCamPosXDef(0f);

        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MaxCamPosXDef(1f);
            PressingRight(true);
            //Debug.Log("PressingRight");
            
        } else if (Input.GetKeyUp(KeyCode.RightArrow))
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

    public void PressingLeft(bool state)
    {
        MelusineAnimator.SetBool("LeftIsDown", state);
        //CamLag(-1f, state);
        //Debug.Log("Melusine turns left"+state);
    }
    public void PressingRight(bool state)
    {
        MelusineAnimator.SetBool("RightIsDown", state);
        //(1f, state);
        //Debug.Log("Melusine turns right"+state);
    }

    public void Hide()
    {
        MelusineAnimator.SetTrigger("Hide");
    }
    
    public void MaxCamPosXDef(float target)
    {
        maxCamPosX =target;
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
            hubbyFireDelay = Random.Range(hubbyMinDelay, hubbyMaxDelay);
            yield return new WaitForSeconds(hubbyFireDelay);

            Husband next = NextHusband();
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
        //Debug.Log("Victory");
        // Make PopIn apear
    }
}
