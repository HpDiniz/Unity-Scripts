using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using TMPro;
using Photon.Pun.UtilityScripts;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
	public static List<bool> updateRequest = new List<bool>();
	public static GameManager Instance;
	public static List<PlayerMovement> players = new List<PlayerMovement>();
	public TMP_Text [] playerRanking;
	public TMP_Text winnerText;
	public Image gamePanel;

	public static readonly byte restartGameEventCode = 1; 

	void Awake()
	{
		SetScoreText();
		Instance = this;
	}

	IEnumerator DisplayMessage(string message)
	{
		winnerText.text = message;
		yield return new WaitForSeconds(5);
		winnerText.text = "";
	}

	void Update()
	{	

		if(updateRequest.Count > 0){
			SetScoreText();
			updateRequest.RemoveAt(0);
			/*
			if(players[0].deathCounter >= 1){
				StartCoroutine(DisplayMessage(players[0].Nickname + " é o vencedor!"));

				for(int i=0; i< playerRanking.Length; i++){
					playerRanking[i].text = "";
				}
				
				foreach (PlayerMovement player in players)
				{
					player.waitingForSpawn = true;
					player.Reset();
				}

				//if(PhotonNetwork.IsMasterClient)
				//	StartCoroutine(RestartGame());
			} */	
		}
	}

	public static void UpdatePlayer(PlayerMovement player)
	{
		for (int i = 0; i < players.Count; i++)
		{
			if(players[i].PV.InstantiationId == player.PV.InstantiationId)
				players[i] = player;
		}

	}


	void SetScoreText()
	{	
		InsertionSort();

		int i = 0;
		foreach (PlayerMovement player in players)
		{
			if(i > 2)
                break;
            playerRanking[i].gameObject.SetActive(true);
            playerRanking[i].text = player.Nickname + " " + player.killCounter.ToString() + "/" + player.deathCounter.ToString();
			i++;
		}
	}

	void InsertionSort()
    {
        int i, j;
        int N = players.Count;

        for (j=1; j<N; j++) {
            for (i=j; i>0 && players[i].killCounter > players[i-1].killCounter; i--) {
                PlayerMovement temporary;
                temporary = players [i];
                players [i] = players [i - 1];
                players [i - 1] = temporary;
            }
        }
    }

	IEnumerator RestartGame() 
	{	

		yield return new WaitForSeconds(3);
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
		SendOptions sendOptions = new SendOptions {Reliability = true};
		PhotonNetwork.RaiseEvent(restartGameEventCode, null , raiseEventOptions, sendOptions);
		//gamePanel.color = new Color(gamePanel.color.r,gamePanel.color.g,gamePanel.color.b,0f);
	}

}