using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Pawn : NetworkBehaviour {

	PawnController myController;
	// Use this for initialization
	void Awake () {
		myController = GetComponent<PawnController> ();
		if (myController != null) {

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
