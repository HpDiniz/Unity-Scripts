using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDroppedGun : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public WeaponStats [] weapons;
    public int currentGunIndex = 4;
    public WeaponStats currentWeapon;
    [HideInInspector] public PhotonView PV;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        currentGunIndex = (int)instantiationData[0];
        
    }

    void Awake()
	{   
        weapons = GetComponentsInChildren<WeaponStats>();
		PV = GetComponent<PhotonView>();
	}

    void Start()
    {   
        for (int i = 0; i < weapons.Length; i++)
        {   
            if(weapons[i].gunIndex == currentGunIndex){
                weapons[i].gameObject.SetActive(true);
                this.gameObject.name = weapons[i].gunName;
                currentWeapon = weapons[i];
            }else
                weapons[i].gameObject.SetActive(false);
        }
    }

    public void DisableGun()
    {
        this.gameObject.SetActive(false);
    }
    
    public WeaponStats ChangeWeapons(int oldGunIndex)
    {   
        WeaponStats oldWeapon = null;
        WeaponStats newWeapon = currentWeapon;

        for (int i = 0; i < weapons.Length; i++)
        {
            if(weapons[i].gunIndex == currentGunIndex){
                newWeapon.totalAmmo = weapons[i].totalAmmo;
                newWeapon.currentAmmo = weapons[i].currentAmmo;
                weapons[i].gameObject.SetActive(false);
            }

            if(weapons[i].gunIndex == oldGunIndex)
                oldWeapon = weapons[i];
        }

        if(oldWeapon == null || oldGunIndex <= 0){
            //DisableGun();
            return newWeapon;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if(weapons[i].gunIndex == oldWeapon.gunIndex){
                weapons[i].totalAmmo = oldWeapon.totalAmmo;
                weapons[i].currentAmmo = oldWeapon.currentAmmo;

                weapons[i].gameObject.SetActive(true);
                this.gameObject.name = weapons[i].gunName;
            }
        }


        currentGunIndex = oldWeapon.gunIndex;
        currentWeapon = oldWeapon;
        
        
        return newWeapon;
        
    }

}