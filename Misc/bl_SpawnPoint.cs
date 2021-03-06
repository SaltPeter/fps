﻿using UnityEngine;
using System.Collections;

public class bl_SpawnPoint : MonoBehaviour {
    public Team m_Team = Team.All;
    public float SpawnSpace = 3f;

	void Start () {
        if (this.transform.GetComponent<Renderer>() != null)
            this.GetComponent<Renderer>().enabled = false;
	}
    //Debug Spawn Spcae
    void OnDrawGizmos()
    {
        Color c = (m_Team == Team.Recon) ? ColorKeys.ReconColor : ColorKeys.DeltaColor;
        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, SpawnSpace);
        Gizmos.color = new Color(c.r,c.g,c.b,0.4f);
        Gizmos.DrawSphere(transform.position, SpawnSpace);
    }
}
