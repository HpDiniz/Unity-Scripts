using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviourPunCallbacks
{
    public float delay = 3f;
    public float radius = 5f;
    public float bombForce = 500f;

    public GameObject explosionEffect;

    float countdown;
    bool hasExploded = false;
    // Start is called before the first frame update
    void Start()
    {   
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {   
        countdown -= Time.deltaTime;
        if( countdown <= 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }

    void Explode()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        List<PlayerMovement> playersHitted = new List<PlayerMovement>();

        for (int i = 0; i < colliders.Length; i++)
        {   
            
            PlayerMovement player = colliders[i].GetComponentInParent<PlayerMovement>();
            if(player == null)
                player = colliders[i].GetComponent<PlayerMovement>();
            if(player != null){
                int distance = (int)(Vector3.Distance(this.transform.position, player.transform.position));
                int damage = ((int)radius + 1) - distance;
                player.ExplosionDamage(damage);
            }
        }

        object[] instanceData = new object[3];
        instanceData[0] = null;
        instanceData[1] = 29;

        PhotonNetwork.Instantiate("Sounds",this.transform.position, Quaternion.identity,0,instanceData);
        Destroy(this.gameObject);
    }
}
