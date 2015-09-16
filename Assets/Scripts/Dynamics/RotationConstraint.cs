using UnityEngine;
using System.Collections;

public class RotationConstraint : MonoBehaviour {
	public Transform Driver;
	public Transform Driven;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Driver != null && Driven != null) {
			Driven.forward = Driver.forward;
		
		}
	
	}
}
