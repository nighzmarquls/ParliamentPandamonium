/*
	Flower
	======
	
	How to combine tubes into one mesh.
	
	
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
	public class Flower : MonoBehaviour
	{
		const int HAIR_COUNT = 800;
		const int POINT_COUNT = 6;
		const float HAIR_RADIUS = 0.045f; 
		
		
		void Start()
		{
			// create an array of CombineInstances to hold tube meshes temporarily
			CombineInstance[] combineInstances = new CombineInstance[ HAIR_COUNT ];
			
			// temporary variables for calculating positions
			float revolutions = 0;
			float positionRadius = HAIR_RADIUS;
			
			// create tubes //
			for( int h = 0; h < HAIR_COUNT; h++ )
			{
				// create game object and add TubeRenderer component
				TubeRenderer tube = new GameObject().AddComponent<TubeRenderer>();
				
				// position object
				revolutions += (HAIR_RADIUS*2) / ( positionRadius * Mathf.PI * 2 );
				positionRadius = revolutions * (HAIR_RADIUS*2);
				tube.transform.position = Quaternion.Euler( 0, revolutions*360, 0 ) * Vector3.forward * positionRadius;
				
				// no caps please
				tube.caps = TubeRenderer.CapMode.None;
				
				// toggle between two different uv mappings
				tube.uvRect = h%2 == 0 ? new Rect( 0.01f, 0.01f, 0.48f, 0.48f ) : new Rect( 0, 0.51f, 0.48f, 0.48f );
				
				// create points and radiuses arrays
				tube.points = new Vector3[ POINT_COUNT ];
				tube.radiuses = new float[ POINT_COUNT ];
				
				// calculate height
				float hairNorm = h / (HAIR_COUNT-1f);
				float height = Mathf.Pow( 1-hairNorm, 0.8f ) * 0.5f;
				
				// define points and radiuses
				tube.points[0] = Vector3.zero;
				tube.radiuses[0] = HAIR_RADIUS;
				for( int p = 1; p < POINT_COUNT; p++ ){
					float norm = (p-1) / (POINT_COUNT-2f);
					float angle = norm*Mathf.PI*0.5f;
					tube.radiuses[ p ] = Mathf.Cos( angle ) * HAIR_RADIUS;
					float y = height + Mathf.Sin( angle ) * HAIR_RADIUS;
					tube.points[ p ] = new Vector3( 0, y, 0 );
				}
				
				// force update to generate the tube mesh immediately
				tube.ForceUpdate();
				
				// add the tube mesh to the combine instances
				combineInstances[h].mesh = tube.mesh;
				combineInstances[h].transform = tube.transform.localToWorldMatrix;
				
				// destroy the TubeRenderer we just used to build the mesh
				Destroy( tube.gameObject );
			}
			
			// add mesh rendering components and combine tubes
			gameObject.AddComponent<MeshRenderer>();
			MeshFilter filter = gameObject.AddComponent<MeshFilter>();
			filter.mesh.CombineMeshes( combineInstances );
			
			// add a material and a texture
			GetComponent<Renderer>().material = new Material( Shader.Find( "Diffuse" ) );
			GetComponent<Renderer>().material.mainTexture = Helpers.CreateTileTexture(2);
		}
	}
}