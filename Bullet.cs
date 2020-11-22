using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{   
    /*
    public int damage = 20;
    private float speed = 100f;
    public int playerWhoShooted;
    */
    private float destroyTime = 1f;

    PhotonView PV;

    /*public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        this.transform.forward = (Vector3) instantiationData[0];
        playerWhoShooted = (int) instantiationData[1];
        
    }*/

	void Awake()
	{   
		PV = GetComponent<PhotonView>();
	}
    
    // Start is called before the first frame update
    void Start()
    {   
        Destroy(this.gameObject,2f);
        //StartCoroutine(DestroyObject());
    }

    // Update is called once per frame

    IEnumerator DestroyObject()
    {   
        yield return new WaitForSeconds(destroyTime);
        //Destroy(this);
        PV.RPC("DestroyObj",RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void DestroyObj()
    {   
        Destroy(this.gameObject);   
    }
    /*
    [PunRPC]
    public void UpdatePosition()
    {   
        this.transform.Translate(Vector3.forward * Time.deltaTime * speed); 
    }

    [PunRPC]
    public void AditionalInfo()
    {   
        this.transform.Translate(Vector3.forward * Time.deltaTime * speed); 
    }
    */
}

