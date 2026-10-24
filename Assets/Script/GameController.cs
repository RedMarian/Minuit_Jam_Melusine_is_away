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

    public float hubbyFireDelay = 2f;
    public float hubbyRightChance = 0.4f;
    public float hubbyLeftChance = 0.5f;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Dance());
        camPosX = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if ((MelusineVariables.MelusinePosition == 1 && Hubby1CanSee.HubbyCanSee) || (MelusineVariables.MelusinePosition ==2 && Hubby2CanSee.HubbyCanSee))
        {
            GameOver();
        }
        else if (Cleanliness >= CleanTime)
        {
            Victory();
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
        HubbyLeftAnimator.SetTrigger("Hubby2");
    }

    void HubbyRightFire()
    {
        HubbyRightAnimator.SetTrigger("Hubby1");
    }

    void HubbyLeftLeaves()
    {
        HubbyLeftAnimator.SetTrigger("Hubby2Leave");
    }

    void HubbyRightLeaves()
    {
        HubbyRightAnimator.SetTrigger("Hubby1Leave");
    }
    
    IEnumerator Dance()
    {
        while (true)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < hubbyRightChance)
            {
                HubbyRightFire();
                //yield return new WaitForSeconds(0.5f);
                //HubbyRightLeaves();
            }
            else if (rand > hubbyLeftChance) // we need the value after checking hubby right
            {
                HubbyLeftFire();
                //yield return new WaitForSeconds(0.5f);
                //HubbyLeftLeaves();
            }
            //Debug.Log(rand);
            yield return new WaitForSeconds(hubbyFireDelay);   
        }
    }

    #endregion
    
    public void GameOver()
    {
        StopAllCoroutines();
        MelusineAnimator.SetTrigger("GameOver");
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
