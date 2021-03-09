using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSounds : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private AudioSource [] audioController;

    [SerializeField]
    private AudioClip[] soundClips;

    public int footStepLenght = 3;

    public int walkingStatus = 0;

    private PlayerMovement characterController;

    private float accumulated_Distance;

    [HideInInspector]
    public float step_Distance;

    int lastRandom;
    bool inWater = false;

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
        if(!characterController.resetGame)
            CheckFootstepSound();

    }

    void OnTriggerStay(Collider other) 
    {
        if(other.tag == "Water")
        {
            inWater = true;
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if(other.tag == "Water")
        {
            inWater = false;
        }
    }

    void CheckFootstepSound(){
        if(!characterController.isGrounded)
            return;

        if(characterController.walkMagnitude > 0)
        {
            accumulated_Distance += Time.deltaTime;

            if(accumulated_Distance > step_Distance)
            {
                int startIndex = (16 + (walkingStatus * 3));

                if(inWater){
                    startIndex = 13;
                }

                int newRandom = Random.Range(startIndex, startIndex+footStepLenght);

                while(newRandom == lastRandom)
                    newRandom = Random.Range(startIndex, startIndex+footStepLenght);

                float volumeMultiplier = characterController.perks[2] ? 0.5f : 1f;
                
                PlaySound(newRandom,volumeMultiplier);
                
                lastRandom = newRandom;

                accumulated_Distance = 0f;
            } 
        } else {
            accumulated_Distance = 0f;
        }
    }

    public void PlayOfflineSound(int index,float volume, int audioIndex)
    {   
        audioController[audioIndex].volume = volume;
        audioController[audioIndex].clip = soundClips[index];
        audioController[audioIndex].Play();
    }

    public void PlaySound(int audioIndex, float volumeMultiplier)
    {   

        object[] instanceData = new object[3];
        instanceData[0] = characterController.PV.InstantiationId;
        instanceData[1] = audioIndex;
        instanceData[2] = volumeMultiplier;

        PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);

    }
}
