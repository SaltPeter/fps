//bl_GameManager.cs - place this in a scena for Spawn Players in Room
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables

public class bl_GameManager : bl_PhotonHelper {
    public static int m_view = -1;
    public static bool isAlive = false;
    public static int SuicideCount = 0;
    [HideInInspector]
    public GameObject OurPlayer;
    [Header("Global")]
    public string OnDisconnectReturn = "MainMenu";
    [Header("References")]
    /// Player Prefabs for Team 1
    public GameObject Player_Team_1;
    /// Player Prefabs for Team 2
    public GameObject Player_Team_2;
    /// List with all Players in Current Room
	public List<PhotonPlayer> connectedPlayerList = new List<PhotonPlayer>();
    /// Camera Preview
    public Camera m_RoomCamera;
    public Transform[] AllSpawnPoints;    /// Spawn Points For FFA
    public Transform[] ReconSpawnPoint;   /// Spawn Points for TDM Team1
    public Transform[] DeltaSpawnPoint; // Spawn Points for TDM Team2

    protected override void Awake() {
        base.Awake();
        PhotonNetwork.isMessageQueueRunning = true;
        SuicideCount = 0;
    }

    public void SpawnPlayer(Team t_team) {
        if (!this.GetComponent<bl_RoomMenu>().SpectatorMode) {
            if (OurPlayer != null)
                PhotonNetwork.Destroy(OurPlayer);

            Hashtable PlayerTeam = new Hashtable();
            PlayerTeam.Add("Team", t_team.ToString());
            PhotonNetwork.player.SetCustomProperties(PlayerTeam);

            if (t_team == Team.Recon)
                OurPlayer = PhotonNetwork.Instantiate(Player_Team_1.name, GetSpawn(ReconSpawnPoint), Quaternion.identity, 0);
            else if (t_team == Team.Delta)
                OurPlayer = PhotonNetwork.Instantiate(Player_Team_2.name, GetSpawn(DeltaSpawnPoint), Quaternion.identity, 0);
            else
                OurPlayer = PhotonNetwork.Instantiate(Player_Team_1.name, GetSpawn(AllSpawnPoints), Quaternion.identity, 0);

            this.GetComponent<bl_ChatRoom>().AddLine("Spawn in " + t_team.ToString() + " Team");
            this.GetComponent<bl_ChatRoom>().Refresh();
            m_RoomCamera.gameObject.SetActive(false);
            StartCoroutine(bl_RoomMenu.FadeOut(1));
            bl_UtilityHelper.LockCursor(true);
        }
        else
            this.GetComponent<bl_RoomMenu>().WaitForSpectator = true;
	}

    /// If Player exist, them destroy
    public void DestroyPlayer() {
        if (OurPlayer != null)
            PhotonNetwork.Destroy(OurPlayer);
    }

    public Vector3 GetSpawn(Transform[] list) {
       int random = Random.Range(0, list.Length);
       Vector3 s = Random.insideUnitSphere * list[random].GetComponent<bl_SpawnPoint>().SpawnSpace; // Errors when spawning as 4th class in 2nd team
       Vector3 pos = list[random].position + new Vector3(s.x, 0, s.z);
       return pos;
    }

    //This is called only when the current gameobject has been Instantiated via PhotonNetwork.Instantiate
    public override void OnPhotonInstantiate(PhotonMessageInfo info) {
        base.OnPhotonInstantiate(info);
        Debug.Log("New object instantiated by " + info.sender);
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMaster) {
        base.OnMasterClientSwitched(newMaster);
        Debug.Log("The old masterclient left, we have a new masterclient: " + newMaster);
        this.GetComponent<bl_ChatRoom>().AddLine("We have a new masterclient: " + newMaster);
    }

    public override void OnDisconnectedFromPhoton() {
        base.OnDisconnectedFromPhoton();
        Debug.Log("Clean up a bit after server quit");
 
        // To reset the scene we'll just reload it:
        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel(OnDisconnectReturn);
    }
    //PLAYER EVENTS
    public override void OnPhotonPlayerConnected(PhotonPlayer player) {
        base.OnPhotonPlayerConnected(player);
        Debug.Log("Player connected: " + player);
    }

    public override void OnReceivedRoomListUpdate() {
        base.OnReceivedRoomListUpdate();
    }
    public override void OnPhotonPlayerDisconnected(PhotonPlayer player) {
        base.OnPhotonPlayerDisconnected(player);
        Debug.Log("Player disconnected: " + player);
    }
    public override void OnFailedToConnectToPhoton(DisconnectCause Cause) {
        base.OnFailedToConnectToPhoton(Cause);
        Debug.Log("OnFailedToConnectToPhoton "+Cause);

        // back to main menu or fisrt scene       
        Application.LoadLevel(OnDisconnectReturn);
    }
}		
