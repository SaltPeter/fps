using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class bl_PhotonConnection : Photon.MonoBehaviour {
    private LobbyState m_state = LobbyState.PlayerName;
    private string playerName;
    private string hostName; //Name of room
    [Header("Photon")]
    public string AppVersion = "1.0";
    public int Port = 5055;
    public float UpdateServerListEach = 2;
    [Header("OnGUI")]
    public GUISkin Skin;
    private float alpha = 2.0f;
    public bool ShowPhotonStatus;
    [Header("UGUI")]
    public GameObject RoomInfoPrefab;
    public Transform RoomListPanel;
    public List<GameObject> MenusUI = new List<GameObject>();
    public CanvasGroup CanvasGroupRoot = null;
    [Header("References")]
    public Text PhotonStatusText = null;
    public Text PlayerNameText = null;

    public Text MaxPlayerText = null;
    public Text RoundTimeText = null;
    public Text GameModeText = null;
    public Text MapNameText = null;
    public Text AntiStrpicText = null;
    public Text QualityText = null;
    public Text VolumenText = null;
    public Text SensitivityText = null;
    public Image MapPreviewImage = null;
    public InputField PlayerNameField = null;
    public InputField RoomNameField = null;
    //OPTIONS
    private int m_currentQuality = 3;
    private float m_volume = 1.0f;
    private float m_sensitive = 15;
    private string[] m_stropicOptions = new string[] { "Disable", "Enable", "Force Enable" };
    private int m_stropic = 0;
    private bool GamePerRounds = false;
    private bool AutoTeamSelection = false;
    [Header("Room Options")]
    public string[] GameModes;
    private int CurrentGameMode = 0;

    //Max players in game
    public int[] maxPlayers;
    private int players;
    //Room Time in seconds
    public int[] RoomTime;
    private int r_Time;
    [Space(5)]
    [Header("Effects")]
    public AudioClip a_Click;
    public AudioClip backSound;
    [Serializable]
    public class AllScenes {
        public string m_name;
        public string m_SceneName;
        public Sprite m_Preview;
    }
    [Header("Levels Manager")]
    public List<AllScenes> m_scenes = new List<AllScenes>();
    private List<GameObject> CacheRoomList = new List<GameObject>();
    private int CurrentScene = 0;

    void Awake() {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.autoJoinLobby = true;

        hostName = "LovattoRoom" + Random.Range(10, 999);
        RoomNameField.text = hostName;

        if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            // generate a name for this player, if none is assigned yet
            if (String.IsNullOrEmpty(playerName))
            {
                playerName = "Guest" + Random.Range(1, 9999);
                PlayerNameField.text = playerName;
            }
            ChangeWindow(0);
        }
        else
        {
            StartCoroutine(Fade(LobbyState.MainMenu, 1.2f));
            if (!PhotonNetwork.connected)
                ConnectPhoton();
            ChangeWindow(2, 1);
        }
        SetUpOptionsHost();
        InvokeRepeating("UpdateServerList", 1, UpdateServerListEach);
        GetPrefabs();
    }

    IEnumerator Fade(LobbyState t_state, float t = 2.0f)
    {
        alpha = 0.0f;
        m_state = t_state;
        while (alpha < t)
        {
            alpha += Time.deltaTime;
            CanvasGroupRoot.alpha = alpha;
            yield return null;
        }
    }

    void ConnectPhoton()
    {
        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (!PhotonNetwork.connected || PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            PhotonNetwork.AuthValues = null;
            PhotonNetwork.ConnectUsingSettings(AppVersion);
            ChangeWindow(3);
        }
    }

    void UpdateServerList() {
        ServerList();
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.connected)
        {
            if (ShowPhotonStatus) {
                PhotonStatusText.text = "<b><color=orange>STATUS:</color>  " + PhotonNetwork.connectionStateDetailed.ToString().ToUpper() + "</b>";
                PlayerNameText.text = "<b><color=orange>PLAYER:</color>  " + PhotonNetwork.player.name + "</b>";
            }
        }
    }

    public void ServerList()
    {
        //Removed old list
        if (CacheRoomList.Count > 0) {
            foreach (GameObject g in CacheRoomList)
                Destroy(g);
            CacheRoomList.Clear();
        }
        //Update List
        RoomInfo[] ri = PhotonNetwork.GetRoomList();
        if (ri.Length > 0)
        {
            //RoomListText.text = string.Empty;
            for (int i = 0; i < ri.Length; i++) {
                GameObject r = Instantiate(RoomInfoPrefab) as GameObject;
                CacheRoomList.Add(r);
                r.GetComponent<bl_RoomInfo>().GetInfo(ri[i]);
                r.transform.SetParent(RoomListPanel, false);
            }
        }
        else
        {
            // RoomListText.text = "There is no room created yet, create your one.";
        }
    }

    /// Menu For Enter Name for UI 4.6 WIP
    public void EnterName(InputField field = null) {
        if (field == null || string.IsNullOrEmpty(field.text)) return;

        playerName = field.text;
        playerName = playerName.Replace("\n", "");

        PlayAudioClip(a_Click, transform.position, 1.0f);
        StartCoroutine(Fade(LobbyState.MainMenu));

        PhotonNetwork.playerName = playerName;
        ConnectPhoton();
    }

    #region UGUI
    /// For button can call this 
    public void ChangeWindow(int id)
    {
        ChangeWindow(id, -1);
    }

    public void ChangeWindow(int id, int id2)
    {
        StartCoroutine(Fade(LobbyState.MainMenu, 3f));
        for (int i = 0; i < MenusUI.Count; i++)
        {
            if (i == id || i == id2)
                MenusUI[i].SetActive(true);
            else
            {
                if (i != 1)//1 = mainmenu buttons
                    MenusUI[i].SetActive(false);
                if (id == 6 || id == 8)
                    MenusUI[1].SetActive(false);
            }
        }
        if (a_Click != null)
            AudioSource.PlayClipAtPoint(a_Click, this.transform.position, 1.0f);
    }

    public void Disconect()
    {
        if (PhotonNetwork.connected) PhotonNetwork.Disconnect();
    }

    public void ChangeServerCloud(int id)
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
            Debug.LogWarning("Try again, because still not disconect");
            return;
        }
        if (PhotonNetwork.PhotonServerSettings.AppID != string.Empty)
        {
            switch (id)
            {
                case 0:
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.us, AppVersion);
                    break;
                case 1:
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.asia, AppVersion);
                    break;
                case 2:
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.au, AppVersion);
                    break;
                case 3:
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.eu, AppVersion);
                    break;
                case 4:
                    PhotonNetwork.ConnectToRegion(CloudRegionCode.jp, AppVersion);
                    break;
            }
            PlayAudioClip(a_Click, transform.position, 1.0f);
        }
        else
        {
            Debug.LogWarning("Need your AppId for changer server, please add it in inspector");
        }
    }

    public void ChangeMaxPlayer(bool plus)
    {
        if (plus)
        {
            if (players < maxPlayers.Length)
            {
                players++;
                if (players > (maxPlayers.Length - 1)) players = 0;
            }
        }
        else
        {
            if (players < maxPlayers.Length)
            {
                players--;
                if (players < 0) players = maxPlayers.Length - 1;
            }
        }
        MaxPlayerText.text = maxPlayers[players] + " Players";
    }

    public void ChangeRoundTime(bool plus)
    {
        if (!plus)
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time--;
                if (r_Time < 0)
                    r_Time = RoomTime.Length - 1;
            }
        }
        else
        {
            if (r_Time < RoomTime.Length)
            {
                r_Time++;
                if (r_Time > (RoomTime.Length - 1))
                    r_Time = 0;
            }
        }
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
    }

    public void ChangeGameMode(bool plus)
    {
        if (plus)
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode++;
                if (CurrentGameMode > (GameModes.Length - 1))
                    CurrentGameMode = 0;
            }
        }
        else
        {
            if (CurrentGameMode < GameModes.Length)
            {
                CurrentGameMode--;
                if (CurrentGameMode < 0)
                    CurrentGameMode = GameModes.Length - 1;
            }
        }
        GameModeText.text = GameModes[CurrentGameMode];
    }

    public void ChangeMap(bool plus)
    {
        if (!plus)
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene--;
                if (CurrentScene < 0)
                    CurrentScene = m_scenes.Count - 1;
            }
        }
        else
        {
            if (CurrentScene < m_scenes.Count)
            {
                CurrentScene++;
                if (CurrentScene > (m_scenes.Count - 1))
                    CurrentScene = 0;
            }
        }
        MapNameText.text = m_scenes[CurrentScene].m_name;
        MapPreviewImage.sprite = m_scenes[CurrentScene].m_Preview;
    }

    public void ChangeAntiStropic(bool plus) {
        if (!plus) {
            if (m_stropic < m_stropicOptions.Length) {
                m_stropic--;
                if (m_stropic < 0)
                    m_stropic = m_stropicOptions.Length - 1;
            }
        }
        else {
            if (m_stropic < m_stropicOptions.Length) {
                m_stropic++;
                if (m_stropic > (m_stropicOptions.Length - 1))
                    m_stropic = 0;
            }
        }
        AntiStrpicText.text = m_stropicOptions[m_stropic];
    }

    public void ChangeQuality(bool plus) {
        if (!plus) {
            if (m_currentQuality < QualitySettings.names.Length) {
                m_currentQuality--;
                if (m_currentQuality < 0)
                    m_currentQuality = QualitySettings.names.Length - 1;
            }
        }
        else {
            if (m_currentQuality < QualitySettings.names.Length) {
                m_currentQuality++;
                if (m_currentQuality > (QualitySettings.names.Length - 1))
                    m_currentQuality = 0;
            }
        }
        QualityText.text = QualitySettings.names[m_currentQuality];
    }

    public void QuitGame(bool b) {
        if (b) {
            Application.Quit();
            Debug.Log("Game Exit, this only work in standalone version");
        }
        else {
            StartCoroutine(Fade(LobbyState.MainMenu, 3.2f));
            ChangeWindow(2, 1);
        }
    }

    public void ChangeAutoTeamSelection(bool b) { AutoTeamSelection = b; }
    public void ChangeGamePerRound(bool b) { GamePerRounds = b; }
    public void ChangeRoomName(string t) { hostName = t; }
    public void ChangeVolume(float v) { m_volume = v; VolumenText.text = (m_volume * 100).ToString("00") + "%"; }
    public void ChangeSensitivity(float s) { m_sensitive = s; SensitivityText.text = m_sensitive.ToString("00") + "%"; }

    void SetUpOptionsHost() {
        MaxPlayerText.text = maxPlayers[players] + " Players";
        RoundTimeText.text = (RoomTime[r_Time] / 60) + " Minutes";
        GameModeText.text = GameModes[CurrentGameMode];
        MapNameText.text = m_scenes[CurrentScene].m_name;
        MapPreviewImage.sprite = m_scenes[CurrentScene].m_Preview;
        AntiStrpicText.text = m_stropicOptions[m_stropic];
        SensitivityText.text = m_sensitive.ToString("00") + "%";
        VolumenText.text = (m_volume * 100).ToString("00") + "%";
        QualityText.text = QualitySettings.names[m_currentQuality];
    }
    public void Save() {
        PlayerPrefs.SetFloat("volumen", m_volume);
        PlayerPrefs.SetFloat("sensitive", m_sensitive);
        PlayerPrefs.SetInt("quality", m_currentQuality);
        PlayerPrefs.SetInt("anisotropic", m_stropic);
        Debug.Log("Save Done!");
    }
    #endregion

    public void CreateRoom() {
        PhotonNetwork.player.name = playerName;
        //Save Room properties for load in room
        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropiertiesKeys.TimeRoomKey] = RoomTime[r_Time];
        roomOption[PropiertiesKeys.GameModeKey] = GameModes[CurrentGameMode];
        roomOption[PropiertiesKeys.SceneNameKey] = m_scenes[CurrentScene].m_SceneName;
        roomOption[PropiertiesKeys.RoomRoundKey] = GamePerRounds ? "1" : "0";
        roomOption[PropiertiesKeys.TeamSelectionKey] = AutoTeamSelection ? "1" : "0";

        string[] properties = new string[5];
        properties[0] = PropiertiesKeys.TimeRoomKey;
        properties[1] = PropiertiesKeys.GameModeKey;
        properties[2] = PropiertiesKeys.SceneNameKey;
        properties[3] = PropiertiesKeys.RoomRoundKey;
        properties[4] = PropiertiesKeys.TeamSelectionKey;

        PhotonNetwork.CreateRoom(hostName, new RoomOptions()
        {
            maxPlayers = (byte)maxPlayers[players],
            isVisible = true,
            isOpen = true,
            customRoomProperties = roomOption,
            cleanupCacheOnLeave = true,
            customRoomPropertiesForLobby = properties
        }, null);
    }

	AudioSource PlayAudioClip(AudioClip clip, Vector3 position, float volume) {
        GameObject go = new GameObject("One shot audio");
        go.transform.position = position;
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        Destroy(go, clip.length);
        return source;
    }

    private IEnumerator MoveToGameScene() {
        //Wait for check
        while (PhotonNetwork.room == null)
            yield return 0;
        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel((string)PhotonNetwork.room.customProperties[PropiertiesKeys.SceneNameKey]);
    }
    // LOBBY EVENTS

    void OnJoinedLobby() {
        Debug.Log("We joined the lobby.");
        StartCoroutine(Fade(LobbyState.MainMenu));
        ChangeWindow(2, 1);
    }

    void OnLeftLobby() {
        Debug.Log("We left the lobby.");
    }

    // ROOMLIST
    void OnReceivedRoomList() {
        Debug.Log("We received a new room list, total rooms: " + PhotonNetwork.GetRoomList().Length);
    }

    void OnReceivedRoomListUpdate() {
        Debug.Log("We received a room list update, total rooms now: " + PhotonNetwork.GetRoomList().Length);
    }

    void OnJoinedRoom() {
        Debug.Log("We have joined a room.");
        StartCoroutine(MoveToGameScene());
    }
    void OnFailedToConnectToPhoton(DisconnectCause cause) {
        Debug.LogWarning("OnFailedToConnectToPhoton: " + cause);
    }
    void OnConnectionFail(DisconnectCause cause) {
        Debug.LogWarning("OnConnectionFail: " + cause);
    }

    void GetPrefabs() {
        if (PlayerPrefs.HasKey("volumen")) {
            m_volume = PlayerPrefs.GetFloat("volumen");
            AudioListener.volume = m_volume;
        }
        if (PlayerPrefs.HasKey("sensitive")) {
            m_sensitive = PlayerPrefs.GetFloat("sensitive");
        }
        if (PlayerPrefs.HasKey("quality"))
            m_currentQuality = PlayerPrefs.GetInt("quality");
        if (PlayerPrefs.HasKey("anisotropic"))
            m_stropic = PlayerPrefs.GetInt("anisotropic");
    }

    private int GetButtonSize(LobbyState t_state) {
        if (m_state == t_state)
            return 55;
        else
            return 40;
    }

    [System.Serializable]
    public enum LobbyType {
        UGUI,
        OnGUI,
    }
}