////////////////////////////////bl_AFKDectect.cs/////////////////////////////////
//Use this to prevent AFK players in your game,and avoids a power level (if any)/
using UnityEngine;
using System.Collections;

public class bl_AFKDectect : bl_PhotonHelper {
    public float m_AFKTime = 40f; /// maximum time that players can be AFK
	public GUISkin Skin;
    //private
    private float m_lastInputTime;
    private Vector3 oldMousePosition = Vector3.zero;
    private bool m_showAFKmsn;

    void Update() {
        //if no movement or action of the player is detected, then start again
        if ((PhotonNetwork.player == null || Input.anyKey) || ((this.oldMousePosition != Input.mousePosition) ))
            this.m_lastInputTime = Time.time;
        this.oldMousePosition = Input.mousePosition;
        //show message now?
        this.m_showAFKmsn = ((this.m_lastInputTime + m_AFKTime) - 10f) < Time.time;
        //If the maximum time is AFK then meets back to the lobby.
        if ((this.m_lastInputTime + m_AFKTime) < Time.time) {
            Debug.Log("AFK Detect");
            bl_UtilityHelper.LockCursor(false);
            if (PhotonNetwork.connected)
                PhotonNetwork.LeaveRoom();
            else
                Application.LoadLevel(0);
        }
    }

    void OnGUI() {
		GUI.skin = this.Skin;
		if (m_showAFKmsn) {
            string text = "<color=orange>AFK detected.</color> Returning to Lobby in " + ((m_AFKTime - (Time.time - this.m_lastInputTime))).ToString("0.0");
            bl_UtilityHelper.ShadowLabel(new Rect(((Screen.width / 2) - 180), ((Screen.height / 2) - 100), 360f, 30f), text);
        }
    }
}