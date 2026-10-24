using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubbyVariables : MonoBehaviour
{
    public bool HubbyCanSee = false;
    // Start is called before the first frame update
    public void ThisHubbyCanSee(bool canSee)
    {
        HubbyCanSee = canSee;
    }
}
