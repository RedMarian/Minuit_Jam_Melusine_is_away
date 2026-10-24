using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelusionVariables : MonoBehaviour
{
    public int MelusinePosition = 2;
    public GameObject PopInVictory;
    public GameObject PopInGameOver;
    
    // Start is called before the first frame update
    public void MelusineIsOnPose(int pose)
    {
        MelusinePosition = pose;
        // 0 = Hidden, 1 = left, 2 = right
    }
    public void PopIn(GameObject popIn)
    {
        popIn.SetActive(true);
    }
    
}
