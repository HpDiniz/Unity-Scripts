using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDroppedGun : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public WeaponStats [] weapons;
    public int currentGunIndex = 4;
    public WeaponStats currentWeapon;
    //public List<WeaponStats> weapons = new List<WeaponStats>();

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   
        
        object[] instantiationData = info.photonView.InstantiationData;

        currentGunIndex = (int)instantiationData[0];
        
    }

    void Start()
    {   
        weapons = GetComponentsInChildren<WeaponStats>();

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
    
    public WeaponStats ChangeWeapons(WeaponStats oldWeapon)
    {   
        WeaponStats newWeapon = currentWeapon;

        Debug.Log("Current: " + currentGunIndex + "   Old:" + oldWeapon.gunIndex);

        for (int i = 0; i < weapons.Length; i++)
        {
            if(weapons[i].gunIndex == currentGunIndex){
                newWeapon.totalAmmo = weapons[i].totalAmmo;
                newWeapon.currentAmmo = weapons[i].currentAmmo;
                weapons[i].gameObject.SetActive(false);
            }
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