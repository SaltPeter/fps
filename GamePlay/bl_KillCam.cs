﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bl_KillCam : MonoBehaviour {
	public Transform target = null; /// Target to follow
    public string TagTargets = ""; /// Enemys Tag
	public float distance = 10.0f; /// Distance from camera to target

	/// list with al enemys with same tag
	public List<Transform> OtherList = new List<Transform>();

	public string Follow = ""; /// target name to follow
	public Transform Provide; /// in case targets is null see this
	public float distanceMax = 15f; /// Maxime Distance to target
	public float distanceMin = 0.5f; /// Min Distance to target
	public float xSpeed = 120f; /// X vector speed
	public float yMaxLimit = 80f; /// maxime y vector Limit
	public float yMinLimit = -20f; /// minime Y vector limit
	public float ySpeed = 120f; /// Y vector speed
	public bool Can_See_Other = true;
    public bool SmoothMovement = true;

    public float SpeedSmooth = 5; /// Smooth motion speed
	public string KillCamTitle = "";
    [Range(0.01f,5f)]
    public float m_wait;
    public bool isLocal = false;
    [System.Serializable]
    public class UI_
    {
        public GUISkin Skin;
        public Texture2D KillCamEffect;
        public Color UIColor;
        public Texture2D InfoBackGround;
        public GUIStyle TextStyle;
    }
    public UI_ UI;
    
    private int CurrentOther = 0;
    private bool isSearch = false;
    private bool isWait = false;
    private float x;
    private float y;
    private string Killer;

    void OnEnable() {
        this.x = 30f;
        this.y = 30f;
        if (GetComponent<Rigidbody>() != null)
            GetComponent<Rigidbody>().freezeRotation = true;
        if (Can_See_Other)
            InvokeRepeating("UpdateOtherList", 1, 1);
        if (m_wait > 0)
        {
            isWait = true;
            StartCoroutine(Wait());
        }
        else
            this.transform.parent = null;
    }

    void OnDisable()
    {
        CancelInvoke("UpdateOtherList");
    }

    /// update list of targets
    void UpdateOtherList()
    {
        OtherList.Clear();
        GameObject[] others = GameObject.FindGameObjectsWithTag(TagTargets);
        foreach (GameObject ot in others)
            OtherList.Add(ot.transform);
    }

    /// resfresh all
    void Refresh() {
		if (GameObject.Find(Follow) != null)
			target = GameObject.Find(Follow).transform;
		else {
			if (!Can_See_Other)
				target = Provide;
			else {
				OtherList.Clear();
				GameObject[] others = GameObject.FindGameObjectsWithTag(TagTargets);
				foreach (GameObject ot in others)
					OtherList.Add(ot.transform);
			}
		}
    }

	void LateUpdate () {
        if (!isWait) {
            CamMovements();
            UpdateTarget();
        }
	}
    
    /// update camera movements
    void CamMovements()
    {
        if (this.target != null)
        {
            RaycastHit hit;
            this.x += ((Input.GetAxis("Mouse X") * this.xSpeed) * this.distance) * 0.02f;
            this.y -= (Input.GetAxis("Mouse Y") * this.ySpeed) * 0.02f;
            this.y = ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
            Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0f);
            this.distance = Mathf.Clamp(this.distance - (Input.GetAxis("Mouse ScrollWheel") * 5f), distanceMin, distanceMax);
            if ((Physics.Linecast(target.position, transform.position, out hit) && !hit.transform.IsChildOf(target)) && (target.transform != hit.transform))
            {
                distance = Mathf.Clamp(this.distance - (Input.GetAxis("Mouse ScrollWheel") * 5f), 0f, distanceMax);
                distance = hit.distance - 1.5f;
            }
            Vector3 vector = new Vector3(0f, 0f, -distance);
            Vector3 vector2 = target.position;
            vector2.y = target.position.y + 1f;
            Vector3 vector3 = (quaternion * vector) + vector2;
            transform.rotation = quaternion;
            if (!SmoothMovement)
                transform.position = vector3;
            else
              transform.position = Vector3.Lerp(transform.localPosition,vector3, Time.deltaTime * SpeedSmooth);
        }
    }

    /// Update the camera to follow 
    void UpdateTarget()
    {
		if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space) && OtherList.Count > 0)
		{
			isSearch = true;
			if (CurrentOther <= OtherList.Count  && CurrentOther >= 0)
				CurrentOther ++;
			if ( CurrentOther >= OtherList.Count)
				CurrentOther = 0;
		}

        if (!isSearch)
        {
            if (GameObject.Find(Follow) != null)
                target = GameObject.Find(Follow).transform;
            else
                target = Provide;
        }
        else
        {
            if (OtherList.Count > 0 && Can_See_Other)
                target = OtherList[CurrentOther];
            else
                target = Provide;
        }
    }

	void OnGUI(){
		GUI.skin = UI.Skin;
		GUI.color = UI.UIColor;
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), UI.KillCamEffect);
		GUI.DrawTexture(new Rect(0, Screen.height - 175, 400, 75), UI.InfoBackGround);
		GUI.color = Color.white;
		GUI.Label(new Rect(Screen.width / 2 - 65, 15, 150, 35), "<size=35>" + KillCamTitle + "</size>", UI.TextStyle);
		if (target)
			GUI.Label(new Rect(75, Screen.height - 175, 250, 35), "<size=20><color=#ff0000ff>Spectating:</color></size> " + target.name, UI.TextStyle);
		GUI.Label(new Rect(75, Screen.height - 145, 250, 35), "<size=20><color=#ff0000ff>Killed by:</color></size> " + Killer, UI.TextStyle);
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
			angle += 360f;
		if (angle > 360f)
			angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}
    
    /// recive target name to camera follow
    /// <param name="t_target"> name of target</param>
    public void Send_Target(string t_target)
    {
        Follow = t_target;
        Killer = t_target;
        Refresh();
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(m_wait);
        this.transform.parent = null;
        isWait = false;
    }
}