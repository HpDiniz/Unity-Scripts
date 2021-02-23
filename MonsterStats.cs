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
        if(hits > 0 && hits % 25 == 0){

            object[] instanceData = new object[1];
			float percent = Random.Range(0.0f, 1.0f);
			
			if(percent <= 0.3f){
				instanceData[0] = 1;
			} else if(percent <= 0.6f){
				instanceData[0] = 2;
			} else if(percent <= 0.75f){
				instanceData[0] = 3;
			} else if(percent <= 0.90f){
				instanceData[0] = 4;
			}else{
				instanceData[0] = 5;
			}

            Vector3 dropPosition = this.transform.position;

            dropPosition = new Vector3(dropPosition.x + Random.Range(-4.0f, 4.0f), dropPosition.y+ 20f, dropPosition.z + Random.Range(-4.0f, 4.0f));

            PhotonNetwork.Instantiate("DroppedGun",dropPosition, Quaternion.identity,0,instanceData);
            percent = 0;
            hits++;
            
        }
    }
}
