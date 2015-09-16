/*
	Slide
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
	public class Slide : MonoBehaviour
	{
		Material tileMaterial;
		
		int POINT_COUNT = 64;
		
		
		void Start()
		{
			// add a TubeRenderer component
			TubeRenderer tube = gameObject.AddComponent<TubeRenderer>();
			
			// define uv mapping
			tube.uvRect = new Rect( 0, 0, 4, 1 );
			tube.uvRectCap = new Rect( 0.543f, 0, 0.33f, 0.33f );
			
			// set a global radius for the tube
			tube.radius = 0.5f;
			
			// reduce tube mesh to three edges
			tube.edgeCount = 3;

			// set normal mode to hard edges
			tube.normalMode = TubeRenderer.NormalMode.HardEdges;
			
			// create point array
			tube.points = new Vector3[ POINT_COUNT ];
			
			// define points
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float angle = norm * Mathf.PI*2 * 0.7f;
				float radius = Mathf.Lerp( 2, 0.8f, norm );
				float y = Mathf.Lerp( 2, 0, norm );
				tube.points[p] = new Vector3( Mathf.Cos(angle)*radius, y, Mathf.Sin(angle)*radius );
			}
			
			// IMPORTANT! call ForceUpdate to generate the mesh immediately, before adding the MeshCollder
			tube.ForceUpdate();
			
			// add MeshCollider. the reference to the tube mesh is set automatically
			gameObject.AddComponent<MeshCollider>();
			
			// create a material at apply it to the tube
			tileMaterial = new Material( Shader.Find( "Diffuse" ) );
			tileMaterial.mainTexture = Helpers.CreateTileTexture(6);
			tileMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
			tube.GetComponent<Renderer>().sharedMaterial = tileMaterial;
			
			// destroy the TubeRenderer component to free up memory
			Destroy( tube );
			
			// start the rain
			StartCoroutine( RainCoroutine() );
		}
		
		
		IEnumerator RainCoroutine()
		{
			while( true )
			{
				// generate ball
				GameObject ball = GameObject.CreatePrimitive( PrimitiveType.Sphere );
				ball.transform.position = new Vector3( 1.5f, 2f, 0 );
				ball.transform.localScale = Vector3.one * 0.5f;
				ball.AddComponent<Rigidbody>();
				ball.GetComponent<Rigidbody>().mass = 1f;
				ball.GetComponent<Rigidbody>().drag = 0.01f;
				ball.GetComponent<Rigidbody>().angularDrag = 0.05f;
				ball.GetComponent<Rigidbody>().AddForce( new Vector3( 0, -200, 800 ) );
				ball.GetComponent<Renderer>().sharedMaterial = tileMaterial;
				
				// you are dying from the moment you are born
				StartCoroutine( WaitAndDestroy( ball ) );
				
				// wait before we generate next ball
				yield return new WaitForSeconds( 0.3f );
			}
		}
		
		
		IEnumerator WaitAndDestroy( GameObject go )
		{
			yield return new WaitForSeconds( 3 );
			Destroy( go );
		}
	}
}