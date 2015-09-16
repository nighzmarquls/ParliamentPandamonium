using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshBody : MonoBehaviour {

	public Transform targetSkeleton;
	private List<List<Transform>> Limbs;
	public List<MeshLimb> Meshes;
	public GameObject BaseMesh;

	private bool unInitialized = true;
	// Use this for initialization
	void Init () {

		if (unInitialized) {
			if (targetSkeleton != null) {
				Limbs = new List<List<Transform>> ();
				Meshes = new List<MeshLimb> ();

				DiscoverLimbs (targetSkeleton);

				CreateLimbMeshes ();
			}
			unInitialized = false;
		}
	}

	void CreateLimbMeshes()
	{
		for (int i = 0; i < Limbs.Count; i ++) {
			GameObject tempObject = Instantiate(BaseMesh);
			MeshLimb tempMesh = tempObject.AddComponent<MeshLimb>();
			tempMesh.m_bones = new Transform[Limbs[i].Count];
			for(int ib = 0; ib < Limbs[i].Count; ib++)
			{
				tempMesh.m_bones[ib] = Limbs[i][ib];
			}
			Meshes.Add (tempMesh);

			tempObject.transform.position = Vector3.zero;
		}
	}

	void DiscoverLimbs(Transform input){
		if (input.childCount > 0) {
			Transform root = input;
			for(int i = 0; i < input.childCount; i++)
			{
				List<Transform> NewLimb = new List<Transform> ();
				NewLimb.Add (root);
				Transform child = input.GetChild(i);
				bool search = true;
				do
				{
					if(child != null)
					{
						NewLimb.Add(child);
						if(child.childCount > 1)
						{
							DiscoverLimbs(child);
							search = false;
							break;
						}
						else if(child.childCount == 0)
						{
							search = false;
							break;
						}
						child = child.GetChild(0);
					}
					else
					{

						search = false;
						break;
					}

				}while(search);
				Limbs.Add (NewLimb);


			}
		}

	}

	
	// Update is called once per frame
	void Update () {
		Init ();
	}
}
