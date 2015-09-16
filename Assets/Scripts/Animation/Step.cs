using UnityEngine;
using System.Collections;

public class Step : MonoBehaviour {

	public Transform m_kneePoint;
	public Transform m_footPoint;

	private Vector3 m_footOffset;
	private Vector3 m_kneeOffset;
	private bool unInitialized = true;

	private float footKneeDistance = 0;

	void Init () {
		if(m_footPoint != null && m_kneePoint != null)
		{

			GameObject tempFootObject = m_footPoint.gameObject;

			if(tempFootObject != null)
			{
				m_footOffset = m_footPoint.position - transform.position;
				m_kneeOffset = m_kneePoint.position - transform.position;
				footKneeDistance = Vector3.Distance(m_footPoint.position,m_kneePoint.position);
				m_footPoint.SetParent(null);
				unInitialized = false;
			}

		}
	}

	void KneeMove()
	{
		Vector3 offset = transform.position+ m_kneeOffset  + m_kneePoint.forward*(Mathf.Sin (Time.time*2)*1f);

		m_kneePoint.position = offset;
	}

	void FootMove()
	{
		Vector3 offset = transform.position + m_footOffset + m_kneePoint.forward*(Mathf.Sin (Time.time*2)*1.5f);

		m_footPoint.position = offset;
	}

	// Update is called once per frame
	void Update () {
		if (unInitialized) {
			Init ();
		} else {
			KneeMove ();
			FootMove ();
		}
	}

	void LateUpdate()
	{
	
	}
}
