using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubbyVariables : MonoBehaviour
{
    public bool HubbyCanSee = false;
    // Start is called before the first frame update
    public void ThisHubbyCanSee()
    {
        HubbyCanSee = true;
    }
    
    public void ThisHubbyCantSee()
    {
        HubbyCanSee = false;
    }
}
