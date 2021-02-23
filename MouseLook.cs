using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float clampDegrees = 75f;
    public float mouseSensitivity = 200f;

    private float vRecoil;
    private float hRecoil;

    Camera fpsCam;
    public Transform playerBody;

    public float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        fpsCam = this.GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void Update()
    {   

        float mouseX = Input.GetAxis("Mouse X") * (fpsCam.fieldOfView/60 * mouseSensitivity) * Time.deltaTime + vRecoil;
        float mouseY = Input.GetAxis("Mouse Y") * (fpsCam.fieldOfView/60 * mouseSensitivity) * Time.deltaTime + hRecoil;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampDegrees, clampDegrees);

        transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
        playerBody.Rotate(Vector3.up * mouseX);

        if(Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus))
        {
            mouseSensitivity = mouseSensitivity + 0.2f;
        } else if(Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {
            mouseSensitivity = mouseSensitivity - 0.2f;
        }

        if(Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        if(mouseSensitivity < 50f)
            mouseSensitivity = 50f;
        else if(mouseSensitivity > 500f)
            mouseSensitivity = 500f;
            
    }

    public void AddRecoil(float v, float h)
    {
        vRecoil = v;
        hRecoil = h;
    }
}
