using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AkGhostPosition : MonoBehaviour
{   
    PlayerMovement playerMovement;
    
    void Start()
    {
        //playerMovement = GetComponentInParent<PlayerMovement>();
    }


    // Update is called once per frame
    void Update()
    {   
        //transform.localRotation = Quaternion.Euler(playerMovement.mouseLook.xRotation,0f,0f);
    }
}
