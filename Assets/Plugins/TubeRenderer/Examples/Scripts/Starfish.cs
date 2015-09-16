/*
	Starfish
	========
	
	How to animate vertex colors.
	
	
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
	public class Starfish : MonoBehaviour
	{
		const int ARM_COUNT = 5;
		const float ARM_OFFSET = 0.1f;
		const int POINT_COUNT = 20;
		
		TubeRenderer[] armTubes;
		
		
		void Start()
		{	
			// create arms
			armTubes = new TubeRenderer[ARM_COUNT];
			for( int a=0; a<ARM_COUNT; a++ )
			{
				// create and new game object and TubeRendeder component
				armTubes[a] = new GameObject( "Arm" + a ).AddComponent<TubeRenderer>();
				armTubes[a].transform.parent = transform;
				
				// simplify mesh
				armTubes[a].edgeCount = 6;
				
				// no caps
				armTubes[a].caps = TubeRenderer.CapMode.None;
				
				// define uv mapping
				armTubes[a].uvRect = new Rect( 0, 0, 4, 1 );
				
				// optimise for realtime manipulation
				armTubes[a].MarkDynamic();
				
				// create point, radius and color arrays
				armTubes[a].points = new Vector3[POINT_COUNT];
				armTubes[a].radiuses = new float[POINT_COUNT];
				armTubes[a].colors = new Color32[POINT_COUNT];
				
				// define radiuses
				for( int p=0; p<POINT_COUNT; p++ ) armTubes[a].radiuses[p] = (1-(p/(POINT_COUNT-1f))) * ARM_OFFSET*1.49f;
				
				// rotate and position
				float armNorm = a / (float) ARM_COUNT;
				armTubes[a].transform.Rotate( 0, armNorm * 360, 0 );
				armTubes[a].transform.Translate( ARM_OFFSET, 0, 0, Space.Self );
			}
			
			// create head (beware of ugly hard-coding)
			TubeRenderer headTube = new GameObject("Head").AddComponent<TubeRenderer>();
			headTube.transform.parent = transform;
			headTube.edgeCount = ARM_COUNT;
			headTube.caps = TubeRenderer.CapMode.End;
			headTube.uvRectCap = new Rect( 0, 0, 1/6f, 1/6f );
			headTube.radius = ARM_OFFSET*1.24f;
			headTube.points = new Vector3[]{ Vector3.zero, Vector3.up * ARM_OFFSET*1.29f };
			headTube.colors = new Color32[]{ Color.black, Color.black };
			
			// rotate the head tube around it's forward direction to match the arms (could use transform.Rotate instead)
			headTube.forwardAngleOffset = -18;
			
			// add a material with a shader that takes vertex colors
			Material mat = Helpers.CreateVertexColorMaterial();
			mat.mainTexture = Helpers.CreateTileTexture(6);
			mat.mainTexture.wrapMode = TextureWrapMode.Repeat;
			headTube.GetComponent<Renderer>().sharedMaterial = mat;
			for( int a=0; a<ARM_COUNT; a++ ) armTubes[a].GetComponent<Renderer>().sharedMaterial = mat;
		}
		
		
		void Update()
		{
			// calculate colors for one arm
			for( int p=0; p<POINT_COUNT; p++ ) armTubes[0].colors[p] = PerlinColorAtOffset( -p*0.1f );
			// overwite to trigger mesh update
			armTubes[0].colors = armTubes[0].colors;
			
			// reuse colors for the rest of the arms
			for( int a=1; a<armTubes.Length; a++ ) armTubes[a].colors = armTubes[0].colors;
			
			// make freaky arm animation ...
			for( int a=0; a<armTubes.Length; a++ ){
				float angleY = 0;
				float angleZ = 0;
				for( int p=0; p<POINT_COUNT; p++ ){
					float norm = p / (POINT_COUNT-1f);
					Vector3 offset = Vector3.right * Mathf.Lerp( 0.1f, 0.01f, norm );
					if( p > 1 ){
						angleY += (Mathf.PerlinNoise( norm*4 , a*10 - Time.time*0.8f )*2-1 ) * 60 * norm;
						angleZ += (Mathf.PerlinNoise( norm*4 , 500+a*10 - Time.time*0.3f )*2-1 ) * 60 * norm;
						offset = Quaternion.Euler( 0, angleY, angleZ ) * offset;
					}
					if( p > 0 ) armTubes[a].points[p] = armTubes[a].points[p-1] + offset;
				}
				armTubes[a].points = armTubes[a].points;
			}
		}
		
		
		Color PerlinColorAtOffset( float offset ){
			return Color.Lerp( Color.black, Color.white, Mathf.PerlinNoise( offset + Time.time*2.2f, 0  )*3-1 );
		}
	}
}