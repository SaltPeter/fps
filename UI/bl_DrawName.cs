//  bl_DrawName.cs
// Can be attached to a GameObject to show Player Name 
using UnityEngine;

public class bl_DrawName : MonoBehaviour {
    /// at what distance the name is hiding
    public float m_HideDistance;
   
    public Texture2D m_HideTexture;
    public GUIStyle m_Skin;

    [HideInInspector]
    public string m_PlayerName= string.Empty;

    public Transform m_Target;
    //Private
    private float m_dist;
    private Transform myTransform;

	void OnEnable() {
        this.myTransform = this.transform;
    }

    void OnGUI() {
        if (Camera.main == null)
            return;

            Vector3 vector = Camera.main.WorldToScreenPoint(this.m_Target.position);
            if (vector.z > 0) {
                if (this.m_dist < m_HideDistance)
                    GUI.Label(new Rect(vector.x - 5, (Screen.height - vector.y) - 7, 10, 11), m_PlayerName,m_Skin);
                else
                    GUI.DrawTexture(new Rect(vector.x - 5, (Screen.height - vector.y) - 7, 13, 13), this.m_HideTexture);
            }
    }

    void Update() {
        if (Camera.main == null)
            return;
        
		if (this.m_PlayerName == string.Empty)
			this.m_PlayerName = this.gameObject.name;
		this.m_dist = bl_UtilityHelper.GetDistance(this.myTransform.position, Camera.main.transform.position);
    }
}

