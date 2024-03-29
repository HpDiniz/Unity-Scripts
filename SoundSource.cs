﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviourPun, IPunInstantiateMagicCallback
{   
    PhotonView PV;
    PlayerMovement playerPosition;
    AudioSource [] audios;
    int audioIndex;
    float volumeMultiplier = 1.0f;

    void Awake()
	{   
		PV = GetComponent<PhotonView>();
        audios = this.GetComponentsInChildren<AudioSource>();
	}

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        audioIndex = (int) instantiationData[1];
        if(instantiationData.Length > 2){
            if(instantiationData[2] != null)
                volumeMultiplier = (float) instantiationData[2];
        }

        if(instantiationData[0] != null){

            PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();
            
            foreach (PlayerMovement player in players)
            {
                if(player.PV.InstantiationId == (int)instantiationData[0])
                    playerPosition = player;
            }
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {   
        audios[audioIndex].volume = audios[audioIndex].volume * volumeMultiplier;
        audios[audioIndex].maxDistance = audios[audioIndex].maxDistance * volumeMultiplier;

        audios[audioIndex].Play(0);

        if(audioIndex > 27)
            Destroy(this.gameObject,7f);
        else
            Destroy(this.gameObject,3f);
    }

    void Update()
    {   
        if(playerPosition != null)
            this.transform.position = playerPosition.transform.position;
    }
}
