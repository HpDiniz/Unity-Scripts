using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class MonsterStats : MonoBehaviour
{
    public int hits = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if(hits > 0 && hits % 10 == 0){
            
            bool grenade = false;
            object[] instanceData = new object[1];
			float percent = Random.Range(0.0f, 1.0f);
			
			if(percent <= 0.20f){
				instanceData[0] = 1;
			} else if(percent <= 0.35f){
				instanceData[0] = 2;
			} else if(percent <= 0.50f){
				instanceData[0] = 3;
			} else if(percent <= 0.65f){
				instanceData[0] = 4;
			} else if(percent <= 0.80f){
				instanceData[0] = 5;
			} else {
                grenade = true;
            }

            Vector3 dropPosition = this.transform.position;
            dropPosition = new Vector3(dropPosition.x + Random.Range(-4.0f, 4.0f), dropPosition.y+ 15f, dropPosition.z + Random.Range(-4.0f, 4.0f));

            if(grenade)
                PhotonNetwork.Instantiate("Grenade",dropPosition, Quaternion.identity);
            else
                PhotonNetwork.Instantiate("DroppedGun",dropPosition, Quaternion.identity,0,instanceData);

            percent = 0;
            hits++;
        }
    }


}
