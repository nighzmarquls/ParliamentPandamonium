/*
	Volume
	======
	
	How to add thickness to tube.
	
	
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
	public class Volume : MonoBehaviour
	{
		const int POINT_COUNT = 16;
		
		TubeRenderer outerTube, innerTube;
		
		
		void Start()
		{	
			// create tubes for outer and inner surface
			outerTube = new GameObject( "Outer Tube" ).AddComponent<TubeRenderer>();
			innerTube = new GameObject( "Inner Tube" ).AddComponent<TubeRenderer>();
			outerTube.transform.parent = transform;
			innerTube.transform.parent = transform;
			
			// optimise for realtime manipulation
			outerTube.MarkDynamic();
			innerTube.MarkDynamic();
			
			// invert the mesh of the inner tube
			innerTube.invertMesh = true;
			
			// only cap the beginning of the tubes
			outerTube.caps = TubeRenderer.CapMode.Begin;
			innerTube.caps = TubeRenderer.CapMode.Begin;
			
			// create point and radius arrays
			outerTube.points = new Vector3[ POINT_COUNT ];
			outerTube.radiuses = new float[ POINT_COUNT ];
			innerTube.points = new Vector3[ POINT_COUNT+1 ];
			innerTube.radiuses = new float[ POINT_COUNT+1 ];
			
			// define points //
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				outerTube.points[p] = Vector3.right * Mathf.Lerp( 0.6f, -0.4f, norm );
				innerTube.points[p] = outerTube.points[p];
			}
			innerTube.points[POINT_COUNT] = innerTube.points[POINT_COUNT-1]; // double last point
			
			// add a texutre and adjust the uv mapping of the caps
			outerTube.GetComponent<Renderer>().sharedMaterial.mainTexture = TubeRendererExamples.Helpers.CreateTileTexture(12);
			innerTube.GetComponent<Renderer>().sharedMaterial.mainTexture = outerTube.GetComponent<Renderer>().sharedMaterial.mainTexture;
			outerTube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			innerTube.uvRectCap = outerTube.uvRectCap;
		}
		
		
		void Update()
		{
			// animate radiuses //
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float shapeRadius = Mathf.Lerp( 0.25f, 0.8f, Mathf.Pow( norm, 3 ) );
				float loudnessRadius = norm * Mathf.PerlinNoise( norm*1.5f - Time.time*12f, 0 ) * 0.2f;
				outerTube.radiuses[p] = shapeRadius + loudnessRadius;
				innerTube.radiuses[p] = outerTube.radiuses[p] - 0.15f;
			}
			innerTube.radiuses[POINT_COUNT] = outerTube.radiuses[POINT_COUNT-1];
			
			// overwrite radius array reference to trigger mesh updates
			innerTube.radiuses = innerTube.radiuses;
			outerTube.radiuses = outerTube.radiuses;
		}
	}
}