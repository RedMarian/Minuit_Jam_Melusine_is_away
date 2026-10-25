using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelusionVariables : MonoBehaviour
{
    public int MelusinePosition = 2;
    public GameObject PopInWin;
    public GameObject PopInLose;
    
    
    // Start is called before the first frame update
    public void MelusineIsOnPose(int pose)
    {
        MelusinePosition = pose;
        // 0 = Hidden, 1 = left, 2 = right, 4 = both can see
    }
    public void PopInWinDo()
    {
        PopInWin.SetActive(true);
    }

    public void PopInLoseDo()
    {
        PopInLose.SetActive(true);
    }

    public void PlaySound(AudioClip clip)
    {
        this.gameObject.GetComponent<AudioSource>().PlayOneShot(clip);
    }
    
}
