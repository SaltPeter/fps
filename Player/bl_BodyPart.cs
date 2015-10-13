// bl_BodyPart.cs - This script receives the information of the damage done by another player
// place it on a gameobject containing a collider in the hierarchy of the remote player
// use "bl_BodyPartManager.cs" to automatically configure                       
using UnityEngine;

public class bl_BodyPart : MonoBehaviour {
    public float multiplier = 1f; // Damage multiplier, multiplies damage taken
    public bl_PlayerDamageManager HealtScript;
    public bool TakeHeatShot = false; // Is it the head?

    /// Use this for recive damage local and sync for all other
    public void GetDamage(float t_damage, string t_from, string t_weapon,Vector3 t_direction,int weapon_ID = 0)
    {
        float m_TotalDamage = t_damage + multiplier;

        bl_OnDamageInfo e = new bl_OnDamageInfo();
        e.mDamage = m_TotalDamage;
        e.mDirection = t_direction;
        e.mWeapon = t_weapon;
        e.mHeatShot = TakeHeatShot;
        e.mWeaponID = weapon_ID;
        e.mFrom = t_from;

        if (HealtScript != null)
            HealtScript.GetDamage(e);
    }
}
