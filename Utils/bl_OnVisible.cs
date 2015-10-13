// bl_OnVisible.cs - Helps optimize the game by making the player's remote script only run when they are out of the frustum
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
