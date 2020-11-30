using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GhostPosition : MonoBehaviour
{   
    public Camera fpsCam;
    public MouseLook mouseLook;
    float targetHeight = 1.9f;
    float test = 1f;

    int teste = 0;

    void Update()
    {   
        /*this.transform.rotation = fpsCam.transform.rotation;
        test++;
        if(Input.GetKey(KeyCode.Q))
        {
            teste++;
            if(teste > 6)
                teste = 0;
            this.transform.localRotation = Quaternion.Euler(27.294f,15.122f,3.132f);
        }
*/
        //this.transform.localRotation = Quaternion.Euler(fpsCam.transform.localRotation.x,fpsCam.transform.localRotation.y,fpsCam.transform.localRotation.z);
        
        if(mouseLook != null){

            //Debug.Log(teste);
            /*
            if(teste == 1)
                this.transform.localRotation = Quaternion.Euler(fpsCam.transform.localRotation.x,0f,0f);
            else if(teste == 2)
                this.transform.localRotation = Quaternion.Euler(0f,fpsCam.transform.localRotation.x,0f);
            else if(teste == 3)
                this.transform.localRotation = Quaternion.Euler(0f,0f,fpsCam.transform.localRotation.x);
            else if(teste == 4)
                this.transform.localRotation = Quaternion.Euler(fpsCam.transform.localRotation.y,0f,0f);
            else if(teste == 5)
                this.transform.localRotation = Quaternion.Euler(0f,fpsCam.transform.localRotation.y,0f);
            else if(teste == 6)
                this.transform.localRotation = Quaternion.Euler(0f,0f,fpsCam.transform.localRotation.y);
                */
        } 
        //this.transform.position = Vector3.Lerp(fpsCam.transform.position, new Vector3(fpsCam.transform.position.x,controller.transform.position.y + targetHeight/2 -0.1f,fpsCam.transform.position.z), 7.5f * Time.deltaTime);
    }

    public void Disable()
    {
        this.gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
        //Debug.Log("Desabilita meu zovo");
        //this.gameObject.SetActive(false);
    }

    public void SetReference(MouseLook mouseLook, Camera fpsCam)
    {
        this.mouseLook = mouseLook;
        this.fpsCam = fpsCam;
    }


}
