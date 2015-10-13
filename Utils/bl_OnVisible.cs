﻿// bl_OnVisible.cs
// This script helps to optimize the game as it makes the players remote script
// only run when they are viewed by our player.
using UnityEngine;

public class bl_OnVisible : MonoBehaviour {

    public bl_PlayerAnimations NPA;
    public bl_UpperAnimations Upper;

    void OnBecameInvisible() {
        NPA.m_Update = false;
        Upper.m_Update = false;
    }

    void OnBecameVisible() {
        NPA.m_Update = true;
        Upper.m_Update = true;
    }
}
