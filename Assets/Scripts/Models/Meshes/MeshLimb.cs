using UnityEngine;
using System.Collections;

public class MeshLimb : MonoBehaviour {

	public Transform[] m_bones;
	public float m_radius = 1;
	private TubeRenderer m_tube;

	// Use this for initialization
	void Awake () {

		m_tube = gameObject.AddComponent<TubeRenderer>();

	}

	public void CreateMesh(){

		m_tube.points = new Vector3[ m_bones.Length ];
		m_tube.radiuses = new float[m_bones.Length];
		float increment = m_radius / m_bones.Length;
		// define points
		for( int p=0; p<m_bones.Length; p++ ){

			m_tube.points[p] = m_bones[p].position;//gameObject.transform.parent.TransformVector(m_bones[p].position);
			m_tube.radiuses[p] = m_radius - (increment*(p+1));
		}
		
		// generate the mesh immediately
		m_tube.ForceUpdate();
	}

	// Update is called once per frame
	void Update () {
		CreateMesh ();
	}
}
