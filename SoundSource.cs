using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviourPun, IPunInstantiateMagicCallback
{   
    PhotonView PV;
    AudioSource [] audios;
    int audioIndex;

    void Awake()
	{   
		PV = GetComponent<PhotonView>();
	}

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        audioIndex = (int) instantiationData[0];
        
    }
    // Start is called before the first frame update
    void Start()
    {   

        Debug.Log(this.transform.position.ToString());
        audios = this.GetComponentsInChildren<AudioSource>();
        audios[audioIndex].Play(0);
        Destroy(this.gameObject,2f);
    }
}
