using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviourPun, IPunInstantiateMagicCallback
{   
    public int damage = 20;
    private float speed = 100f;
    private float destroyTime = 1f;

    public int playerWhoShooted;

    PhotonView PV;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        this.transform.forward = (Vector3) instantiationData[0];
        playerWhoShooted = (int) instantiationData[1];

        LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, this.transform.position);
        lineRenderer.SetPosition(1, (Vector3) instantiationData[0]);


    }

	void Awake()
	{   
		PV = GetComponent<PhotonView>();
	}
    
    // Start is called before the first frame update
    void Start()
    {   
        StartCoroutine(DestroyBullet());
    }

    // Update is called once per frame
    void Update()
    {  
        this.transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    IEnumerator DestroyBullet()
    {   
        yield return new WaitForSeconds(destroyTime);
        PV.RPC("DestroyObj",RpcTarget.AllBuffered);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player" && other.tag != "PlayerController" && other.tag != "Bullet"){
            PhotonNetwork.Instantiate("HitParticles",this.transform.position,Quaternion.identity);
            PV.RPC("DestroyObj",RpcTarget.AllBuffered);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag != "Player" && other.tag != "PlayerController" && other.tag != "Bullet"){
            PhotonNetwork.Instantiate("HitParticles",this.transform.position,Quaternion.identity);
            PV.RPC("DestroyObj",RpcTarget.AllBuffered);
        }
    }

    public void HitPlayer()
    {   
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

