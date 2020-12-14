using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [SerializeField]
    private AudioSource [] audioController;

    [SerializeField]
    private AudioClip[] soundClips;

    public int footStepLenght = 3;

    private PlayerMovement characterController;

    [HideInInspector]
    public float volume_Min, volume_Max;

    private float accumulated_Distance;

    [HideInInspector]
    public float step_Distance;

    int lastRandom;

    void Awake()
    {
        audioController = GetComponents<AudioSource>();
        characterController = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckFootstepSound();

        /*audioController.volume = 1f;
        audioController.clip = soundClips[3];
        audioController.Play();*/
    }

    void CheckFootstepSound(){
        if(!characterController.isGrounded)
            return;

        if(characterController.walkMagnitude > 0)
        {
            accumulated_Distance += Time.deltaTime;

            if(accumulated_Distance > step_Distance)
            {
                float volume = Random.Range(volume_Min, volume_Max);

                int newRandom = Random.Range(0, footStepLenght);

                while(newRandom == lastRandom)
                    newRandom = Random.Range(0, footStepLenght);
                
                PlaySound(newRandom,volume,0);
                
                lastRandom = newRandom;

                accumulated_Distance = 0f;
            } 
        } else {
            accumulated_Distance = 0f;
        }
    }

    public void PlaySound(int index,float volume, int audioIndex)
    {   
        audioController[audioIndex].volume = volume;
        audioController[audioIndex].clip = soundClips[index];
        audioController[audioIndex].Play();
    }
}
