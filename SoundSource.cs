using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviourPun, IPunInstantiateMagicCallback
{   
    PhotonView PV;
    PlayerMovement playerPosition;
    AudioSource [] audios;
    int audioIndex;

    void Awake()
	{   
		PV = GetComponent<PhotonView>();
	}

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        audioIndex = (int) instantiationData[1];


        PlayerMovement [] players =  FindObjectsOfType<PlayerMovement>();
        
        foreach (PlayerMovement player in players)
        {
            if(player.PV.InstantiationId == (int)instantiationData[0])
                playerPosition = player;
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {   
        audios = this.GetComponentsInChildren<AudioSource>();
        audios[audioIndex].Play(0);
        Destroy(this.gameObject,4f);
    }

    void Update()
    {
        this.transform.position = playerPosition.transform.position;
    }
}
