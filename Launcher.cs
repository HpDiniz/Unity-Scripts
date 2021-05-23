using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;

	[SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;
	[SerializeField] TMP_Text nickText;

	string[] randomNick = {"Corno", "Robson", "Cavala", "Ribs", "Naruto", "Hacker", "Noia"};

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
	}

	public void InsertNick()
	{	
		if(nickText != null){
			if(nickText.text != null)
				if(nickText.text.Length > 20)
					PhotonNetwork.NickName = nickText.text.Substring(0, 20);
				else
					PhotonNetwork.NickName = nickText.text;
		}
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		MenuManager.Instance.OpenMenu("title");
		Debug.Log("Joined Lobby");
		if(PhotonNetwork.NickName == null || PhotonNetwork.NickName == "" || (PhotonNetwork.NickName != null && PhotonNetwork.NickName.Length < 3))
			PhotonNetwork.NickName = randomNick[Random.Range(0, randomNick.Length-1)] + Random.Range(0, 1000).ToString("000");
	}

	public void CreateRoom()
	{
		if(string.IsNullOrEmpty(roomNameInputField.text))
		{
			return;
		}

		string title = "Armando";
		if(roomNameInputField.text.Length < 50)
			if(roomNameInputField.text.Length > 20)
				title = roomNameInputField.text.Substring(0, 20);
			else
				title = roomNameInputField.text;

		PhotonNetwork.CreateRoom(title);
		MenuManager.Instance.OpenMenu("loading");

		if(PhotonNetwork.NickName == null || PhotonNetwork.NickName == "" || (PhotonNetwork.NickName != null && PhotonNetwork.NickName.Length < 3))
			PhotonNetwork.NickName = randomNick[Random.Range(0, randomNick.Length-1)] + Random.Range(0, 1000).ToString("000");	
	}

	public override void OnJoinedRoom()
	{
		MenuManager.Instance.OpenMenu("room");
		roomNameText.text = PhotonNetwork.CurrentRoom.Name;

		Player[] players = PhotonNetwork.PlayerList;

		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for(int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		errorText.text = "Room Creation Failed: " + message;
		Debug.LogError("Room Creation Failed: " + message);
		MenuManager.Instance.OpenMenu("error");
	}

	public void StartGame()
	{	
		GameObject [] allButtons = GameObject.FindGameObjectsWithTag("Button");

		for(int i=0; i< allButtons.Length; i++){
			Button button = allButtons[i].GetComponent<Button>();
			TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
			button.interactable = false;
			if(allButtons[i].name == "StartButton")
				buttonText.text = "Loading...";
		}
		PhotonNetwork.LoadLevel(1);
	}

	public void LeaveRoom()
	{	
		PhotonNetwork.LeaveRoom();
		MenuManager.Instance.OpenMenu("loading");
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		MenuManager.Instance.OpenMenu("loading");
	}

	public override void OnLeftRoom()
	{
		MenuManager.Instance.OpenMenu("title");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach(Transform trans in roomListContent)
		{
			Destroy(trans.gameObject);
		}

		for(int i = 0; i < roomList.Count; i++)
		{
			if(roomList[i].RemovedFromList)
				continue;
			Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}
}