﻿//bl_GunManager.cs - Use this to manage all weapons Player
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_GunManager : MonoBehaviour {
	public List<bl_Gun> AllGuns = new List<bl_Gun>(); /// all the Guns of game			  
	public List<bl_Gun> PlayerEquip = new List<bl_Gun>(); /// weapons that the player take equipped

	public int m_Current = 0; /// ID the weapon to take to start

	public bl_Gun CurrentGun; /// weapon used by the player currently				  
	public Transform TrowPoint = null; /// Point where guns instantiate when trow			   
	public float SwichTime = 1; /// time it takes to switch weapons

	public float PickUpTime = 0.4f;
    public bool CanSwich; // Can Switch Now?
	[Space(5)]
    public Animator m_HeatAnimator;

    void Awake() {
        //when player instance select player class select in bl_RoomMenu
        switch (bl_RoomMenu.m_playerclass)
        {
            case PlayerClass.Assault:
                PlayerEquip[0] = AllGuns[m_AssaultClass.primary];
                PlayerEquip[1] = AllGuns[m_AssaultClass.secondary];
                PlayerEquip[2] = AllGuns[m_AssaultClass.Knife];
                PlayerEquip[3] = AllGuns[m_AssaultClass.Special];
                break;
            case PlayerClass.Recon:
                PlayerEquip[0] = AllGuns[m_ReconClass.primary];
                PlayerEquip[1] = AllGuns[m_ReconClass.secondary];
                PlayerEquip[2] = AllGuns[m_ReconClass.Knife];
                PlayerEquip[3] = AllGuns[m_ReconClass.Special];
                break;
            case PlayerClass.Engineer:
                PlayerEquip[0] = AllGuns[m_EngineerClass.primary];
                PlayerEquip[1] = AllGuns[m_EngineerClass.secondary];
                PlayerEquip[2] = AllGuns[m_EngineerClass.Knife];
                PlayerEquip[3] = AllGuns[m_EngineerClass.Special];
                break;
            case PlayerClass.Support:
                PlayerEquip[0] = AllGuns[m_SupportClass.primary];
                PlayerEquip[1] = AllGuns[m_SupportClass.secondary];
                PlayerEquip[2] = AllGuns[m_SupportClass.Knife];
                PlayerEquip[3] = AllGuns[m_SupportClass.Special];
                break;
        }
        //Desactive all weapons in children and take the firts
        foreach (bl_Gun guns in AllGuns)
            guns.gameObject.SetActive(false);
        TakeWeapon(PlayerEquip[m_Current].gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && CanSwich && m_Current != 0)
        {
            StartCoroutine(ChangeGun(PlayerEquip[m_Current].gameObject,PlayerEquip[0].gameObject));
             m_Current = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && CanSwich && m_Current != 1)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject),PlayerEquip[1].gameObject));
            m_Current = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && CanSwich && m_Current != 2)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[2].gameObject));
            m_Current = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && CanSwich && m_Current != 3)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[3].gameObject));
            m_Current = 3;
        }
        //change gun with Scroll mouse
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[(this.m_Current + 1) % this.PlayerEquip.Count].gameObject));
            m_Current = (this.m_Current + 1) % this.PlayerEquip.Count;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (this.m_Current != 0)
            {
                StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[(this.m_Current - 1) % this.PlayerEquip.Count].gameObject));
                this.m_Current = (this.m_Current - 1) % this.PlayerEquip.Count;
            }
            else
            {
                StartCoroutine(ChangeGun((PlayerEquip[m_Current].gameObject), PlayerEquip[this.PlayerEquip.Count - 1].gameObject));
                this.m_Current = this.PlayerEquip.Count - 1;
            }

        }
        CurrentGun = PlayerEquip[m_Current];
    }

    void TakeWeapon(GameObject t_weapon)
    {
        t_weapon.SetActive(true);
        CanSwich = true;
    }

    public bl_Gun GetCurrentWeapon()
    {
        if (CurrentGun == null)
            return PlayerEquip[m_Current];
        else
            return CurrentGun;
    }
    /// Coroutine to Change of Gun
   public IEnumerator ChangeGun(GameObject t_current,GameObject t_next)
    {
        CanSwich = false;
        if (m_HeatAnimator != null)
            m_HeatAnimator.SetBool("Swicht", true);
        t_current.GetComponent<bl_Gun>().DisableWeapon();
        yield return new WaitForSeconds(SwichTime);
        foreach (bl_Gun guns in AllGuns) {
            if (guns.gameObject.activeSelf == true)
                guns.gameObject.SetActive(false);
        }
        TakeWeapon(t_next);
        if (m_HeatAnimator != null)
            m_HeatAnimator.SetBool("Swicht", false);
    }

   public void heatReloadAnim(int m_state)
   {
       if (m_HeatAnimator == null)
           return;

       switch (m_state)
       {
           case 0:
               m_HeatAnimator.SetInteger("Reload", 0);
               break;
           case 1:
               m_HeatAnimator.SetInteger("Reload", 1);
               break;
           case 2:
               m_HeatAnimator.SetInteger("Reload", 2);
               break;
       }
   }
    
   [System.Serializable]
   public class AssaultClass
   {
       //ID = the number of Gun in the list AllGuns
       public int primary = 0; /// the ID of the first gun Equipped
		public int secondary = 1; /// the ID of the secondary Gun Equipped
		public int Knife = 3;
       public int Special = 2; /// the ID the a special weapon
	}
	public AssaultClass m_AssaultClass;

   [System.Serializable]
   public class EngineerClass
   {
       //ID = the number of Gun in the list AllGuns
       public int primary = 0; /// the ID of the first gun Equipped
		public int secondary = 1; /// the ID of the secondary Gun Equipped
		public int Knife = 3;
       public int Special = 2; /// the ID the a special weapon
	}
	public EngineerClass m_EngineerClass;

   [System.Serializable]
   public class ReconClass
   {
       //ID = the number of Gun in the list AllGuns
       public int primary = 0; /// the ID of the first gun Equipped
		public int secondary = 1; /// the ID of the secondary Gun Equipped
		public int Knife = 3;
       public int Special = 2; /// the ID the a special weapon
	}
	public ReconClass m_ReconClass;

   [System.Serializable]
   public class SupportClass
   {
       //ID = the number of Gun in the list AllGuns
		public int primary = 0; /// the ID of the first gun Equipped
		public int secondary = 1; /// the ID of the secondary Gun Equipped
		public int Knife = 3;
		public int Special = 2; /// the ID the a special weapon
	}
	public SupportClass m_SupportClass;
}
