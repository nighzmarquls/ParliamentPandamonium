/*
	Herd
	======
	
	How to animate tubes.
	
	
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
	public class Herd : MonoBehaviour
	{
		const int CRITTER_COUNT = 20;
		const int POINT_COUNT = 50;
		const float POINT_INTERVAL = 0.003f;
		const float DISPERSION = 0.3f;

		Critter[] critters;
		

		void Start()
		{
			// fix the frame rate because we base animation on fame count
			Application.targetFrameRate = 60;
			
			// create tiled texture
			Texture2D texture = TubeRendererExamples.Helpers.CreateTileTexture( 12 );
			
			// create critters
			critters = new Critter[ CRITTER_COUNT ];
			for( int s = 0; s < critters.Length; s++ ){
				critters[ s ] = new Critter();
				critters[ s ].tube.transform.parent = gameObject.transform;
				critters[ s ].tube.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
			}
		}
		
		
		void Update()
		{
			// update all critters
			foreach( Critter critter in critters ) critter.Update();
		}
		
		
		class Critter
		{
			public TubeRenderer tube;
			
			
			public Critter()
			{
				// create game object and add TubeRenderer component
				tube = new GameObject( "Critter" ).AddComponent<TubeRenderer>();
				
				// optimise for realtime manipulation
				tube.MarkDynamic();
				
				// define uv mapping for the end caps
				tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
				
				// define points and radiuses
				tube.points = new Vector3[ POINT_COUNT ];
				tube.radiuses = new float[ POINT_COUNT ];
				for( int p = 0; p < POINT_COUNT; p++ ){
					tube.points[p] = SmoothRandom( - p * POINT_INTERVAL );
					tube.radiuses[p] = Mathf.Lerp( 0.2f, 0.05f, p/(POINT_COUNT-1f ) );
				}
			}
			
			
			public void Update()
			{
				// shift all points one step forward
				for( int p = tube.points.Length-1; p > 0; p-- ) tube.points[p] = tube.points[ p-1 ];
				
				// calculate new position and store it in the beginning of the tube
				tube.points[0] = SmoothRandom( Time.frameCount * POINT_INTERVAL );
				
				// overwrite point array reference to trigger mesh update
				tube.points = tube.points;
			}
			
			
			// cheap perlin-like randomness
			Vector3 SmoothRandom( float t )
			{
				Random.seed = tube.GetInstanceID();
				float x = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.51f ) ) ) ) * 5;
				float y = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.78f ) ) ) ) * 3;
				float z = Mathf.Sin( ( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + Mathf.PI * Mathf.Sin( Random.value*DISPERSION + t * 0.28f) ) ) ) * 5;
				return new Vector3( x, y, z );
			}
		}
	}
}