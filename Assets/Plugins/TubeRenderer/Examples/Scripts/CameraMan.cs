/*
	CameraMan
	=========
	
	
	AUTHOR
	======
	Carl Emil Carlsen
	http://sixthsensor.dk
	May 2012 - Oct 2013
	
	This is a Unity Asset Store product.
	https://www.assetstore.unity3d.com/#/content/3281/
*/

using UnityEngine;
using System.Collections;

namespace TubeRendererExamples
{
	public class CameraMan : MonoBehaviour
	{
		public float speed = 4;
		public Vector3 focusPoint = Vector3.zero;
		public bool hover = false;
		public float hoverRange = 0.5f;
		
		Camera cam;
		
		
		void Awake()
		{
			cam = gameObject.GetComponentInChildren( typeof( Camera ) ) as Camera;
		}
		
		
		void LateUpdate()
		{
			// rotate camera for SUPER dramatic effect //
			transform.Rotate( Vector3.up, Time.deltaTime * speed );
			
			// hover camera for EXTRA sugar //
			if( hover ) transform.position = Vector3.up * Mathf.Sin( Time.time*0.5f ) * hoverRange;
			
			// focus //
			cam.transform.LookAt( focusPoint );
		}
	}
}

