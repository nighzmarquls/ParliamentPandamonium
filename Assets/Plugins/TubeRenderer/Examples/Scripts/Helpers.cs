/*
	Helpers
	======
	
	Helpter methods for examples.
	
	
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

namespace TubeRendererExamples
{	
	public class Helpers
	{
		
		public static Material CreateVertexColorMaterial()
		{
			Material material = new Material(
				"Shader \"Texture VertexRGBA\" {\n" +
					"Properties {\n" +
						"_MainTex (\"Base\", 2D) = \"white\" {}\n" + 
					"}\n" +
					"SubShader {\n" +
						"ZWrite On\n" +
						"Blend SrcAlpha OneMinusSrcAlpha\n" +
						"Lighting Off\n" +
						"BindChannels {\n" +
						   "Bind \"Vertex\", vertex\n" +
						   "Bind \"texcoord\", texcoord\n" +
						   "Bind \"Color\", color\n" +
						"}\n" +
						"Pass { SetTexture[_MainTex] { combine texture * primary, texture * primary } }\n" +
					"}\n" +
				"}"
			);
			
			
			material.name = "Vertex Colors";
			material.hideFlags = HideFlags.HideAndDontSave;
			return material;
		}
		
		
		public static Texture2D CreateTileTexture( int sqrTileCount )
		{
			Texture2D texture = new Texture2D( 256, 256 );
			Color32[] px = new Color32[ texture.width * texture.height ];
			int p = 0;
			for( int y=0; y<texture.height; y++ ){
				float yNorm = y / (float) texture.height;
				for( int x=0; x<texture.width; x++ ){
					float xNorm = x / (float) texture.width;
					bool isWhite = (int) (yNorm*sqrTileCount) % 2 == 0;
					if( (int) (xNorm*sqrTileCount) % 2 == 0 ) isWhite = !isWhite;
					px[p++] = isWhite ? new Color32(255,255,255,255) : new Color32(0,0,0,255);
				}
			}
			texture.SetPixels32(px);
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.Apply();
			texture.hideFlags = HideFlags.HideAndDontSave;
			return texture;
		}
	}
}
