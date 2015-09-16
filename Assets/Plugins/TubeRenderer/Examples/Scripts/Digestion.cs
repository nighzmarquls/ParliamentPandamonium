/*
	Digestion
	=========
	
	How to post process tube.
	
	
	AUTHOR
	======
	Carl Emil Carlsen
	http://sixthsensor.dk
	Oct 2013 - Oct 2013
	
	This is a Unity Asset Store product.
	https://www.assetstore.unity3d.com/#/content/3281/
*/

using UnityEngine;
using System.Collections;
using TubeRendererExamples;

namespace TubeRendererExamples
{
	public class Digestion : MonoBehaviour
	{
		const int POINT_COUNT = 61;
		const float DISTORTION_OFFSET = 0.7f;

		TubeRenderer tube;
		float[] normalizedRadiuses;
		
		
		void Start()
		{
			// add TubeRendeder component
			tube = gameObject.AddComponent<TubeRenderer>();

			// optimise for realtime manipulation
			tube.MarkDynamic();
			
			// create point and radius arrays
			tube.points = new Vector3[POINT_COUNT];
			tube.radiuses = new float[POINT_COUNT];
			
			// define points
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				float x = Mathf.Lerp( -1.5f, 1.5f, norm );
				float y = Mathf.Lerp( -1f, 1f, Mathf.PerlinNoise( 0, norm ) );
				float z = Mathf.Lerp( -1f, 1f, Mathf.PerlinNoise( norm*2, 0 ) );
				tube.points[p] = new Vector3( x, y, z );
			}
			
			// set a texture and a uv mapping
			tube.GetComponent<Renderer>().material.mainTexture = Helpers.CreateTileTexture(12);
			tube.GetComponent<Renderer>().material.mainTexture.wrapMode = TextureWrapMode.Repeat;
			tube.uvRect = new Rect( 0, 0, 6, 1 );
			tube.uvRectCap = new Rect( 0, 0, 4/12f, 4/12f );
			
			// create an array to hold animated radiuses
			normalizedRadiuses = new float[ POINT_COUNT ];
			
			// enable post processing by assigning a callback method
			tube.AddPostprocess( Distort );
			
			// display mesh gizmos for debugging. this is convinient when you write your own post process method
			tube.meshGizmos = true;
		}
		
		
		void Update()
		{
			// animate radiuses
			for( int p=0; p<POINT_COUNT; p++ ){
				float norm = p / (POINT_COUNT-1f);
				normalizedRadiuses[p] = Mathf.Max( 0, Mathf.Lerp( -0.5f, 1f, Mathf.PerlinNoise( 0, -norm*2 + Time.time*0.5f ) ) );
				tube.radiuses[p] = Mathf.Lerp( 0.05f, 0.3f, normalizedRadiuses[p] );
			}
			
			// overwrite radius array reference to trigger mesh update
			tube.radiuses = tube.radiuses;

			// change normal mode every third second
			tube.normalMode = (TubeRenderer.NormalMode) ( Mathf.FloorToInt( Time.time / 3f ) % 3 );
		}
		
		
		// this method will be called by TubeRenderer, just before the mesh data is uploaded
		void Distort( Vector3[] vertices, Vector3[] normals, Vector4[] tangents )
		{
			int v = 0;
			Random.seed = 0; // use repetative random lookup

			// so this is where is gets a bit hairy...
			// the vertices are placed in this order: 1) tube, 2) begin cap, and 3) end cap.
			// caps are mapped in order 1) circle vertices and 2) center point.
			// tube vertices are mapped differently depending on the 'normalMode'.

			switch( tube.normalMode ){

			case TubeRenderer.NormalMode.Smooth:

				// SMOOTH normal mode:

				// for every point (including last point) every edge will have a vertex, plus an extra vertex for uv wrapping
				for( int p=0; p<tube.points.Length; p++ ){
					for( int e=0; e<tube.edgeCount; e++ ){
						// add random offset to vertex along the direction of it's normal
						vertices[v] += Mathf.Pow( normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
						v++;
					}
					// copy to uv wrapping
					vertices[v] = vertices[v-tube.edgeCount];
					v++;
				}
				break;

			case TubeRenderer.NormalMode.Hard:

				// HARD normal mode:

				// for every point that is not the last point every edge will have a quad.
				for( int p=0; p<tube.points.Length-1; p++ ){
					for( int e=0; e<tube.edgeCount; e++ ){
						for( int q=0; q<4; q++ ){
							// add random offset to vertex along the direction of it's normal
							vertices[v] += Mathf.Pow( normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
							v++;
						}
					}
				}

				break;

			case TubeRenderer.NormalMode.HardEdges:
				
				// HARD EDGES normal mode:

				// for every point every edge will have two vertices.
				for( int p=0; p<tube.points.Length; p++ ){
					for( int e=0; e<tube.edgeCount; e++ ){
						for( int i=0; i<2; i++ ){
							// add random offset to vertex along the direction of it's normal
							vertices[v] += Mathf.Pow( normalizedRadiuses[p], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
							v++;
						}
					}
				}
				break;
			}

			// add random offset to the vertices of the begin cap along their normals
			if( tube.caps == TubeRenderer.CapMode.Both || tube.caps == TubeRenderer.CapMode.Begin ){
				for( int e=0; e<tube.edgeCount+2; e++ ){
					vertices[v] += Mathf.Pow( normalizedRadiuses[0], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
					v++;
				}
			}
			// add random offset to the vertices of the end cap along their normals
			if( tube.caps == TubeRenderer.CapMode.Both || tube.caps == TubeRenderer.CapMode.End ){
				for( int e=0; e<tube.edgeCount+2; e++ ){
					vertices[v] += Mathf.Pow( normalizedRadiuses[tube.points.Length-1], 2 ) * normals[v] * DISTORTION_OFFSET * Random.value;
					v++;
				}
			}
		}

	}
}