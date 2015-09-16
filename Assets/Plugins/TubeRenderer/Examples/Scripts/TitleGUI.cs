/*
	TiitleGUI
	======
	
	Used in examples.
	
	
	AUTHOR
	======
	Carl Emil Carlsen
	http://sixthsensor.dk
	Oct 2012 - Oct 2013
	
	This is a Unity Asset Store product.
	https://www.assetstore.unity3d.com/#/content/3281/
*/

using UnityEngine;
using System.Collections;


namespace TubeRendererExamples
{
	public class TitleGUI : MonoBehaviour
	{
		public string title;	
		
		
		void OnGUI()
		{
			GUI.Label( new Rect( 10, 4, 300, 20 ), title );
		}
	}

}

