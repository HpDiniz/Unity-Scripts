using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField]
    private AudioSource footstep_Sound;

    [SerializeField]
    private AudioClip[] footstep_Clip;

    private PlayerMovement characterController;

    [HideInInspector]
    public float volume_Min, volume_Max;

    private float accumulated_Distance;

    [HideInInspector]
    public float step_Distance;

    int lastRandom;

    void Awake()
    {
        footstep_Sound = GetComponent<AudioSource>();
        characterController = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckFootstepSound();
    }

    void CheckFootstepSound(){
        if(!characterController.isGrounded)
            return;

        if(characterController.walkMagnitude > 0)
        {
            accumulated_Distance += Time.deltaTime;

            if(accumulated_Distance > step_Distance)
            {
                footstep_Sound.volume = Random.Range(volume_Min, volume_Max);

                int newRandom = Random.Range(0, footstep_Clip.Length);

                while(newRandom == lastRandom)
                    newRandom = Random.Range(0, footstep_Clip.Length);
                    
                footstep_Sound.clip = footstep_Clip[newRandom];
                footstep_Sound.Play();
                lastRandom = newRandom;

                accumulated_Distance = 0f;
            } 
        } else {
            accumulated_Distance = 0f;
        }
    }
}
