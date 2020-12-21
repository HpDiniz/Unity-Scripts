using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GhostPosition : MonoBehaviour
{   
    public Camera fpsCam;
    public MouseLook mouseLook;
    public int gunIndex;
    
    public void Invisible()
    {
        this.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
    }

    public void Visible()
    {
        this.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }

}
