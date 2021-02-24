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
    public Transform gunIcon;

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
        StartCoroutine(MoveSphere());
    }

    public void DisableGun()
    {
        this.gameObject.SetActive(false);
    }
    
    public WeaponStats ChangeWeapons(int oldGunIndex)
    {   
        WeaponStats oldWeapon = null;
        WeaponStats newWeapon = currentWeapon;
        gunIcon.position = currentWeapon.transform.position;

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

        gunIcon.position = currentWeapon.transform.position;
        currentGunIndex = oldWeapon.gunIndex;
        currentWeapon = oldWeapon;
        StartCoroutine(MoveSphere());
        
        return newWeapon;
        
    }

    IEnumerator MoveSphere()
    {   
        for (int i = 0; i < 20; i++)
        {
            gunIcon.position = currentWeapon.transform.position;
            yield return new WaitForSeconds(0.25f);
        }
    } 

}