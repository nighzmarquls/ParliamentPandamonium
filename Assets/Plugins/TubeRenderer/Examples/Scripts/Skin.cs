/*
	Skin
	=====
	
	How to add collider to tube.
	
	
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
	public class Skin : MonoBehaviour
	{
		const int POINT_COUNT = 64;

		Transform[] bones;

		
		void Start()
		{
			// add a TubeRenderer component
			TubeRenderer tube = gameObject.AddComponent<TubeRenderer>();
			
			// define uv mapping
			tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			
			// create point array
			tube.points = new Vector3[ POINT_COUNT ];
			
			// define points
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float x = Mathf.Lerp( 0.5f, -0.5f, norm );
				tube.points[p] = new Vector3( x, 0, 0 );
			}
			
			// generate the mesh immediately
			tube.ForceUpdate();

			// create bones
			bones = new Transform[2];
			bones[0] = new GameObject( "Base Bone" ).transform;
			bones[1] = new GameObject( "End Bone" ).transform;
			bones[0].position = tube.points[0];
			bones[1].position = tube.points[POINT_COUNT-1];
			foreach( Transform b in bones ) b.parent = transform;

			// create and add bones weight information to mesh.
			// we will interpolate from bone0 (base) to bone1 (tail) along the length of the tube.
			// see 'Digestion' example for information about vertex order.
			BoneWeight[] boneWeights = new BoneWeight[tube.mesh.vertexCount];
			int v = 0;
			// first, the tube
			for( int p=0; p<POINT_COUNT; p++ ){
				float pNorm = p / (POINT_COUNT-1f);
				for( int e=0; e<tube.edgeCount+1; e++ ){ // + 1 for hidden uv wrapping vertex
					boneWeights[v].boneIndex0 = 0;
					boneWeights[v].boneIndex1 = 1;
					boneWeights[v].weight0 = 1 - pNorm;
					boneWeights[v].weight1 = pNorm;
					v++;
				}
			}
			// then the begin cap
			for( int e=0; e<tube.edgeCount+2; e++ ){ // + 2 for hidden uv wrapping vertex and center vertex
				boneWeights[v].boneIndex0 = 0;
				boneWeights[v].boneIndex1 = 1;
				boneWeights[v].weight0 = 1;
				boneWeights[v].weight1 = 0;
				v++;
			}
			// then the end cap
			for( int e=0; e<tube.edgeCount+2; e++ ){ // ditto
				boneWeights[v].boneIndex0 = 0;
				boneWeights[v].boneIndex1 = 1;
				boneWeights[v].weight0 = 0;
				boneWeights[v].weight1 = 1;
				v++;
			}
			tube.mesh.boneWeights = boneWeights;

			// create and add bindpose information to mesh.
			// the bind pose is "the inverse of inverse transformation matrix of the bone".
			Matrix4x4[] bindposes = new Matrix4x4[2];
			bindposes[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;
			bindposes[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;
			tube.mesh.bindposes = bindposes;

			// get the mesh before we ...
			Mesh mesh = tube.mesh;
			
			// destroy the components that are no longer needed
			Destroy( tube );
			Destroy( GetComponent<MeshFilter>() );
			Destroy( GetComponent<MeshRenderer>() );

			// add skin and apply the goods
			SkinnedMeshRenderer skin = gameObject.AddComponent<SkinnedMeshRenderer>();
			skin.sharedMesh = mesh;
			skin.bones = bones;
			skin.quality = SkinQuality.Bone2; // this is apparently very important for procedural skinned meshes

			// create a tiled texture and apply it
			skin.GetComponent<Renderer>().sharedMaterial.mainTexture = Helpers.CreateTileTexture(12);
		}


		void Update()
		{
			// create animated values
			float angleOffset = Mathf.Sin( Time.time * Mathf.PI ) * 40;
			float scaleMult = Mathf.Sin( Mathf.PI * 0.5f + Time.time * Mathf.PI * 2 ) * 0.5f + 0.5f;

			// wiggle the tail bone
			bones[1].localRotation = Quaternion.AngleAxis( angleOffset, Vector3.up );
			bones[1].localScale = Vector3.one * Mathf.Lerp( 0.8f, 1.5f, scaleMult );

			// mirror base bone
			bones[0].localRotation = Quaternion.AngleAxis( -angleOffset, Vector3.up );
			bones[0].localScale = Vector3.one * Mathf.Lerp( 1.5f, 0.8f, scaleMult );
		}
	}
	
}