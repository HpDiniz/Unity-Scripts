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

public class RoomManager : MonoBehaviourPunCallbacks
{
	public static RoomManager Instance;

	void Awake()
	{
		if(Instance)
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
		Instance = this;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if(scene.buildIndex == 1) // We're in the game scene
		{
			if(PhotonNetwork.IsMasterClient){
				InstantiateGuns(250);
			}

			PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);

		}
	}

	void InstantiateGuns(int amount)
	{	
		GameObject terrain = GameObject.Find("Terrain");
		Terrain worldTerrain = terrain.GetComponent<Terrain>();

		float terrainLeft = worldTerrain.transform.position.x;
		float terrainBottom = worldTerrain.transform.position.z;
		float terrainWidth = worldTerrain.terrainData.size.x;
		float terrainLength = worldTerrain.terrainData.size.z;
		float terrainRight = terrainLeft + terrainWidth;
		float terrainTop = terrainBottom + terrainLength;

		int i = 0;
		float terrainHeight = 0f;
		RaycastHit hit;
		float randomPositionX, randomPositionY, randomPositionZ;
		Vector3 randomPosition = Vector3.zero;
		object[] instanceData = new object[1];

		do {
			i++;

			int iterations = 0;
			bool setRandomPosition = false;

			while(!setRandomPosition){
				iterations++;
				randomPositionX = Random.Range(terrainLeft, terrainRight);
				randomPositionZ = Random.Range(terrainBottom, terrainTop);
				if(Physics.Raycast(new Vector3(randomPositionX, 9999f, randomPositionZ), Vector3.down, out hit, Mathf.Infinity, terrain.layer)){
					terrainHeight = hit.point.y;
				}
				randomPositionY = terrainHeight + 40f;

				randomPosition = new Vector3(randomPositionX, randomPositionY, randomPositionZ);

				ChangeDroppedGun [] guns =  FindObjectsOfType<ChangeDroppedGun>();

				float nearDistance = 5000f;
			
				foreach (ChangeDroppedGun gun in guns)
				{
					float gunDistance = Vector3.Distance (gun.transform.position, randomPosition);
					if(gunDistance < nearDistance)
						nearDistance = gunDistance;
				}

				if(nearDistance > 30 || iterations > 20)
					setRandomPosition = true;
			}

			float percent = Random.Range(0.0f, 1.0f);

			/*
			if(percent <= 0.33f)
				instanceData[0] = 5;
			else if(percent <= 0.66f){
				instanceData[0] = 4;
			} else {
				instanceData[0] = 3;
			}*/

			/*
			30% de pistola
			30% de submachinegun
			25% de assaultRifle
			15% de lightmachine
			10% de sniper
			0% de shotgun
			*/
			
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

			PhotonNetwork.Instantiate("DroppedGun", randomPosition, Quaternion.identity,0,instanceData);

		} while ( i < amount);

		// INSTANCIA A BOMBA //
		/*
		randomPositionX = Random.Range(terrainLeft, terrainRight);
		randomPositionZ = Random.Range(terrainBottom, terrainTop);
		if(Physics.Raycast(new Vector3(randomPositionX, 9999f, randomPositionZ), Vector3.down, out hit, Mathf.Infinity, terrain.layer)){
			terrainHeight = hit.point.y;
		}
		randomPositionY = terrainHeight + 40f;

		randomPosition = new Vector3(randomPositionX, randomPositionY, randomPositionZ);

		PhotonNetwork.Instantiate("Bomb", randomPosition, Quaternion.identity,0,instanceData);
		*/

	}

}