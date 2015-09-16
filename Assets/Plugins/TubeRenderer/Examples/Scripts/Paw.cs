/*
	Paw
	===
	
	How to clone tubes.
	
	
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
	public class Paw : MonoBehaviour
	{
		const int TUBE_COUNT = 5;
		const int POINT_COUNT = 24;
		const float RADIUS = 1;
		
		TubeRenderer tube;
		
		
		void Start()
		{
			// add TubeRenderer component
			tube = gameObject.AddComponent<TubeRenderer>();
			
			// optimise for realtime manipulation
			tube.MarkDynamic();
			
			// no caps please
			tube.caps = TubeRenderer.CapMode.None;
			
			// create point and radius arrays
			tube.points = new Vector3[POINT_COUNT];
			tube.radiuses = new float[POINT_COUNT];
			
			// define radiuses //
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				tube.radiuses[p] = Mathf.Cos( norm * Mathf.PI * 0.5f ) * 0.4f;
			}
			
			// create tiled texture and assign it to the tube
			tube.GetComponent<Renderer>().sharedMaterial.mainTexture = TubeRendererExamples.Helpers.CreateTileTexture( 12 );
			
			// position //
			tube.transform.position = -Vector3.forward * RADIUS;
			
			// create a bunch of other objects and share the tube mesh
			for( int t=1; t<TUBE_COUNT; t++ ){
				float angle = ( t / (float) TUBE_COUNT ) * 360;
				GameObject cloneTube = new GameObject( "Clone Tube" );
				cloneTube.transform.rotation = Quaternion.Euler( 0, angle, 0 );
				cloneTube.transform.position = cloneTube.transform.rotation * -Vector3.forward * RADIUS;
				cloneTube.AddComponent<MeshFilter>().sharedMesh = tube.mesh;
				cloneTube.AddComponent<MeshRenderer>().sharedMaterial = tube.GetComponent<Renderer>().sharedMaterial;
			}
		}
		
		
		void Update()
		{
			// rotate forward angle slowly
			tube.forwardAngleOffset += Time.deltaTime * 60;

			// calculate an animated angle
			float angleStep = Mathf.Lerp( -0.1f, 0.25f, Mathf.Pow( Mathf.Sin( Time.time*0.1f + Mathf.Sin(Time.time*0.8f)*Mathf.PI )*0.5f+0.5f, 1.6f ) );
			
			// update points
			for( int p=1; p<POINT_COUNT; p++ ){
				float stepLength = Mathf.Lerp( 0.1f, 0.001f, p / (POINT_COUNT-1f) );
				float angle = angleStep * p;
				tube.points[p] = tube.points[p-1] + new Vector3( 0, Mathf.Cos( angle ), Mathf.Sin( angle ) ) * stepLength;
			}
			
			// overwrite point array reference to trigger mesh update
			tube.points = tube.points;
		}
		
	}
	
}