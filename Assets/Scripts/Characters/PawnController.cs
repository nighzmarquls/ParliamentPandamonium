using UnityEngine;
using System.Collections;

public class PawnController : MonoBehaviour {

	private Pawn myPawn;
	public void RegisterPawn(Pawn input)
	{
		if (myPawn == null) {
			myPawn = input;
		}
	}
}
