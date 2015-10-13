//bl_Mono.cs - A simple base class to serve as an extension for the Photon.Monobehaviour default
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class bl_PhotonHelper : Photon.PunBehaviour {
    protected GameMode mGameMode = GameMode.FFA;

    protected virtual void Awake() {
        if (!PhotonNetwork.connected)
            return;      
    }

    public string myTeam {
        get {
            string t = (string)PhotonNetwork.player.customProperties[PropiertiesKeys.TeamKey];
            return t;
        }
    }

    /// Find a player gameobject by the viewID 
    public GameObject FindPlayerRoot(int view) {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
            return m_view.gameObject;
        else
            return null;
    }

    // Get a photonView by the viewID
    public PhotonView FindPlayerView(int view) {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
            return m_view;
        else
            return null;
    }

    public PhotonView GetPhotonView(GameObject go) {
        PhotonView view = go.GetComponent<PhotonView>();
        if (view == null)
            view = go.GetComponentInChildren<PhotonView>();
        return view;
    }

    public Transform Root {
        get {
            return transform.root;
        }
    }

    public Transform Parent {
        get {
            return transform.parent;
        }
    }

    /// It is true if the PhotonView is "mine" and can be controlled by this client.
    /// PUN has an ownership concept that defines who can control and destroy each PhotonView.
    /// True in case the owner matches the local PhotonPlayer.
    /// True if this is a scene photonview on the Master client.
    public bool isMine {
        get {
            return (this.photonView.ownerId == PhotonNetwork.player.ID) || (!this.photonView.isOwnerActive && PhotonNetwork.isMasterClient);
        }
    }

    /// Get Photon.connect
    public bool isConnected {
        get {
            return PhotonNetwork.connected;
        }
    }

    public GameObject FindPhotonPlayer(PhotonPlayer p) {
        GameObject player = GameObject.Find(p.name);
        if (player == null)
            return null;
          return player;
    }

    /// Get the team of players
    public string GetTeam(PhotonPlayer p) {
        if (p == null || !isConnected)
            return null;

        string t = (string)p.customProperties[PropiertiesKeys.TeamKey];
        return t;
    }

    /// Get the team of players
    public Team GetTeamEnum(PhotonPlayer p) {
        if (p == null || !isConnected)
            return Team.All;

        string t = (string)p.customProperties[PropiertiesKeys.TeamKey];
        
        switch (t) {
            case "Recon":
                return Team.Recon;
            case "Delta":
                return Team.Delta;
        }
        return Team.All;
    }

    /// Get current gamemode
    public GameMode GetGameMode {
        get {
            if (!isConnected || PhotonNetwork.room == null)
                return GameMode.TDM;
            if ((string)PhotonNetwork.room.customProperties[PropiertiesKeys.GameModeKey] == GameMode.FFA.ToString())
                mGameMode = GameMode.FFA;
            else if ((string)PhotonNetwork.room.customProperties[PropiertiesKeys.GameModeKey] == GameMode.TDM.ToString())
                mGameMode = GameMode.TDM;
            else if ((string)PhotonNetwork.room.customProperties[PropiertiesKeys.GameModeKey] == GameMode.CTF.ToString())
                mGameMode = GameMode.CTF;
            else
                mGameMode = GameMode.FFA;
            return mGameMode;
        }
    }

    public string LocalName {
        get {
            if (PhotonNetwork.player != null && isConnected) {
                string n = PhotonNetwork.player.name;
                return n;
            }
            else
                return "None";
        }
    }

    /// Get All Player in Room
    /// for get this hash 00xki8697
    public List<PhotonPlayer> AllPlayerList {
        get {
            List<PhotonPlayer> p = new List<PhotonPlayer>();

            foreach (PhotonPlayer pp in PhotonNetwork.playerList)
                p.Add(pp);
            return p;
        }
    }

    /// Get All Player in Room of a specific team
    public List<PhotonPlayer> GetPlayersInTeam(string team) {
        List<PhotonPlayer> p = new List<PhotonPlayer>();

        foreach (PhotonPlayer pp in PhotonNetwork.playerList) {
            if ((string)pp.customProperties[PropiertiesKeys.TeamKey] == team)
                p.Add(pp);
        }
        return p;
    }
}
