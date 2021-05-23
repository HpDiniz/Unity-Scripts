using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.UI;

public class MouseLook : MonoBehaviour
{
    public float clampDegrees = 75f;
    public float mouseSensitivity = 150f;

    private float vRecoil;
    private float hRecoil;

    Camera fpsCam;
    public Transform playerBody;
    public GameObject settingsScreen;
    public Slider sensitivitySlider;
    public Slider volumeSlider;
    public Slider grassSlider;

    public float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        fpsCam = this.GetComponent<Camera>();
        settingsScreen.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {   
        bool settingsOpen = settingsScreen.activeInHierarchy;

        if(Input.GetKeyUp(KeyCode.Escape))
        {   
            OpenSettings(settingsOpen);
        }

        if(settingsOpen)
            return;

        float mouseX = Input.GetAxis("Mouse X") * (fpsCam.fieldOfView/60 * mouseSensitivity) * Time.deltaTime + vRecoil;
        float mouseY = Input.GetAxis("Mouse Y") * (fpsCam.fieldOfView/60 * mouseSensitivity) * Time.deltaTime + hRecoil;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampDegrees, clampDegrees);

        transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
        playerBody.Rotate(Vector3.up * mouseX);

        if(Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Plus))
        {
            mouseSensitivity = mouseSensitivity + 0.2f;
            sensitivitySlider.SetValueWithoutNotify(mouseSensitivity);
        } else if(Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus))
        {
            mouseSensitivity = mouseSensitivity - 0.2f;
            sensitivitySlider.SetValueWithoutNotify(mouseSensitivity);
        }
        
        if(mouseSensitivity < 1f){
            mouseSensitivity = 1f;
            sensitivitySlider.SetValueWithoutNotify(1f);
        }else if(mouseSensitivity > 400f){
            mouseSensitivity = 400f;
            sensitivitySlider.SetValueWithoutNotify(400f);
        }
            
    }

    public void AddRecoil(float v, float h)
    {
        vRecoil = v;
        hRecoil = h;
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void ChangeGrass()
    {
        if(Terrain.activeTerrain != null)
            Terrain.activeTerrain.detailObjectDistance = grassSlider.value;
    }

    public void ChangeSensibility()
    {
        mouseSensitivity = (float)sensitivitySlider.value;
    }

    public void OpenSettings(bool isOpen)
    {
        settingsScreen.SetActive(!isOpen);

        if(isOpen)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !isOpen;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
