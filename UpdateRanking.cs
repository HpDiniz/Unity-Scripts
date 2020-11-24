using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateRanking : MonoBehaviourPunCallbacks  
{      
    
    public TMP_Text [] playerRanking;
    PlayerMovement[] players;
    PhotonView PV;

    void Awake()
	{   
		PV = GetComponent<PhotonView>();
	}

    // Start is called before the first frame update
    void Start()
    {
        //playerRanking = this.GetComponentsInChildren<Text>();
        UpdatePlayers();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePlayers()
    {   
        players = FindObjectsOfType<PlayerMovement>();
        PV.RPC("UpdateCounter",RpcTarget.AllBuffered);
    }
    
    [PunRPC]
    public void UpdateCounter()
    {   
        InsertionSort();
        for (int i = 0; i < players.Length; i++)
        {   
            if(i > 2)
                break;
            playerRanking[i].gameObject.SetActive(true);
            playerRanking[i].text = "   " + players[i].Nickname + " " + players[i].killCounter.ToString() + "/" + players[i].deathCounter.ToString();
        }
    }

    void InsertionSort()
    {
        int i, j;
        int N = players.Length;

        for (j=1; j<N; j++) {
            for (i=j; i>0 && players[i].killCounter > players[i-1].killCounter; i--) {
                PlayerMovement temporary;
                temporary = players [i];
                players [i] = players [i - 1];
                players [i - 1] = temporary;
            }
        }
    }

}

