using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Animator MelusineAnimator;
    public Animator HubbyLeftAnimator;
    public Animator HubbyRightAnimator;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Dance());
    }

    // Update is called once per frame
    void Update()
    {
        
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PressingLeft(true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            PressingLeft(false);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PressingRight(true);
        } else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            PressingRight(false);
        }
    }

    #region PlayerController

    void PressingLeft(bool state)
    {
        MelusineAnimator.SetBool("LeftIsDown", state);
    }
    void PressingRight(bool state)
    {
        MelusineAnimator.SetBool("RightIsDown", state);
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

    #endregion
    
    IEnumerator Dance()
    {
        while (true)
        {
            float rand = Random.Range(0f, 1f);
            if (rand < 0.2f)
            {
                HubbyRightFire();
                HubbyRightLeaves();
            }
            if (rand > 0.8f)
            {
                HubbyLeftFire();
                HubbyLeftLeaves();
            }
            Debug.Log(rand);
            yield return new WaitForSeconds(2);   
        }
    }
}
