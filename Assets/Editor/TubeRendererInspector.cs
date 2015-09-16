/*
	TubeRendererInspector.cs
	Carl Emil Carlsen
	http://sixthsensor.dk
	Oct 2013 - Oct 2013
	
	Part of the TubeRenderer AssetStore files.
*/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(TubeRenderer))]
public class TubeRendererInspector : Editor
{
	TubeRenderer tube;
	bool pointsFoldout, radiusesFoldout, colorsFoldout, uvFoldout;
	Color disabledColor = new Color( 0.7f, 0.7f, 0.7f );
	
	
	void Awake()
	{
		tube = target as TubeRenderer;
	}
	
	
	public override void OnInspectorGUI()
	{
		bool dirty = false;
		
		EditorGUILayout.LabelField( "Stats: ", tube.points.Length.ToString() + " points, " + tube.mesh.vertexCount.ToString() + " verts." );
		
		// points //
		pointsFoldout = EditorGUILayout.Foldout( pointsFoldout, "Points" );
		GUI.changed = false;
		
		if( pointsFoldout ){			
			EditorGUI.indentLevel += 2;
			
			int newArrayLength = ArrayCountField( "Count", tube.points.Length );
			if( GUI.changed && newArrayLength != 1 ){
				Vector3[] newPoints = new Vector3[newArrayLength];
				if( tube.points.Length == 0 ){
					for( int p=0; p<newArrayLength; p++ ) newPoints[p] = Vector3.up * ( p / (newArrayLength-1f) );
				} else {
					System.Array.Copy( tube.points, newPoints, Mathf.Min( tube.points.Length, newArrayLength ) );
					if( newArrayLength > tube.points.Length && tube.points.Length > 1 ){
						Vector3 step = tube.points[tube.points.Length-1] - tube.points[tube.points.Length-2];
						for( int p=0; p<newArrayLength-tube.points.Length; p++ ) newPoints[tube.points.Length+p] = tube.points[tube.points.Length-1] + (p+1) * step;
					}
				}
				tube.points = newPoints;
				dirty = true;
				GUI.changed = false;
			}
			
			EditorGUI.indentLevel++;
			for( int p=0; p<tube.points.Length; p++ ) tube.points[p] = ArrayVector3Field( "Element " + p, tube.points[p] );
			EditorGUI.indentLevel--;
			
			if( GUI.changed ){
				tube.points = tube.points;
				dirty = true;
				GUI.changed = false;
			}
			
			EditorGUI.indentLevel = 0;
		}
		
		// radiuses //
		if( tube.radiuses == null ) GUI.color = disabledColor;
		radiusesFoldout = EditorGUILayout.Foldout( radiusesFoldout, "Radiuses" );
		GUI.changed = false;
		
		if( radiusesFoldout ){			
			EditorGUI.indentLevel += 2;
			
			int newArrayLength = ArrayCountField( "Count", tube.radiuses == null ? 0 : tube.radiuses.Length );
			if( GUI.changed ){
				float[] newRadiuses = new float[newArrayLength];
				if( tube.radiuses != null && tube.radiuses.Length != 0 ){
					System.Array.Copy( tube.radiuses, newRadiuses, Mathf.Min( tube.radiuses.Length, newArrayLength ) );
					if( tube.radiuses.Length > 0 && newArrayLength > tube.radiuses.Length ){
						for( int r=tube.radiuses.Length; r<newArrayLength; r++ ) newRadiuses[r] = tube.radiuses[tube.radiuses.Length-1];
					}
				} else {
					for( int r=0; r<newArrayLength; r++ ) newRadiuses[r] = 0.1f;
				}
				tube.radiuses = newRadiuses;
				dirty = true;
				GUI.changed = false;
			}
			
			if( tube.radiuses != null && tube.radiuses.Length != 0 ){
				EditorGUI.indentLevel++;
				for( int r=0; r<tube.radiuses.Length; r++ ) tube.radiuses[r] = ArrayFloatField( "Element " + r, tube.radiuses[r] );
				EditorGUI.indentLevel--;
				
				if( GUI.changed ){
					tube.radiuses = tube.radiuses;
					dirty = true;
					GUI.changed = false;
				}
			}
			
			
			EditorGUI.indentLevel = 0;
		}
		if( tube.radiuses == null ) GUI.color = Color.white;
		
		
		// colors //
		colorsFoldout = EditorGUILayout.Foldout( colorsFoldout, "Colors (vertex colors)" );
		GUI.changed = false;
		
		if( colorsFoldout ){			
			EditorGUI.indentLevel += 2;
			
			int newArrayLength = ArrayCountField( "Count", tube.colors == null ? 0 : tube.colors.Length );
			if( GUI.changed ){
				Color32[] newColors = new Color32[newArrayLength];
				if( tube.colors != null && tube.colors.Length != 0 ){
					System.Array.Copy( tube.colors, newColors, Mathf.Min( tube.colors.Length, newArrayLength ) );
					if( tube.colors.Length > 0 && newArrayLength > tube.colors.Length ){
						for( int c=tube.colors.Length; c<newColors.Length; c++ ) newColors[c] = tube.colors[tube.colors.Length-1];
					}
				} else {
					for( int c=0; c<newColors.Length; c++ ) newColors[c] = Color.white;
				}
				tube.colors = newColors;
				dirty = true;
				GUI.changed = false;
			}
			
			if( tube.colors != null && tube.colors.Length != 0 ){
				EditorGUI.indentLevel++;
				for( int c=0; c<tube.colors.Length; c++ ) tube.colors[c] = ArrayColorField( "Element " + c, tube.colors[c] );
				EditorGUI.indentLevel--;
				
				if( GUI.changed ){
					tube.colors = tube.colors;
					dirty = true;
					GUI.changed = false;
				}
			}
			
			EditorGUI.indentLevel = 0;
		}
		
		// tangents //
		bool calculateTangents = EditorGUILayout.Toggle( "Calc Tangents", tube.calculateTangents );
		if( GUI.changed ){
			tube.calculateTangents = calculateTangents;
			dirty = true;
			GUI.changed = false;
		}
		
		// invert mesh //
		bool invertMesh = EditorGUILayout.Toggle( "Invert Mesh", tube.invertMesh );
		if( GUI.changed ){
			tube.invertMesh = invertMesh;
			dirty = true;
			GUI.changed = false;
		}
		
		// normal mode //
		TubeRenderer.NormalMode normalMode = (TubeRenderer.NormalMode) EditorGUILayout.EnumPopup( "Normal Mode", tube.normalMode );
		if( GUI.changed ){
			tube.normalMode = normalMode;
			dirty = true;
			GUI.changed = false;
		}
		
		// caps //
		TubeRenderer.CapMode caps = (TubeRenderer.CapMode) EditorGUILayout.EnumPopup( "Caps", tube.caps );
		if( GUI.changed ){
			tube.caps = caps;
			dirty = true;
			GUI.changed = false;
		}
		
		// edge count //
		int edgeCount = (int) EditorGUILayout.Slider( "Edge Count", tube.edgeCount, 3, 64 );
		if( GUI.changed ){
			tube.edgeCount = edgeCount;
			dirty = true;
			GUI.changed = false;
		}
		
		// radius //
		if( tube.radiuses != null ) GUI.color = disabledColor;
		float radius = EditorGUILayout.Slider( "Radius (global)", tube.radius, 0.1f, 1f );
		if( GUI.changed ){
			tube.radius = radius;
			dirty = true;
			GUI.changed = false;
		}
		if( tube.radiuses != null ) GUI.color = Color.white;
		
		// forward angle offset //
		float forwardAngleOffset = EditorGUILayout.Slider( "Forward angle offset", tube.forwardAngleOffset, -180f, 180 );
		if( GUI.changed ){
			tube.forwardAngleOffset = forwardAngleOffset;
			dirty = true;
			GUI.changed = false;
		}
		
		// uv mapping //
		uvFoldout = EditorGUILayout.Foldout( uvFoldout, "UV Mapping" );
		GUI.changed = false;
		
		if( uvFoldout ){
			EditorGUI.indentLevel += 2;
			
			// uv rect //
			Rect uvRect = EditorGUILayout.RectField( "Rect", tube.uvRect );
			if( GUI.changed ){
				tube.uvRect = uvRect;
				dirty = true;
				GUI.changed = false;
			}
			
			if( tube.caps != TubeRenderer.CapMode.None )
			{
				// uv rect cap //
				Rect uvRectCap = EditorGUILayout.RectField( "Rect Cap", tube.uvRectCap );
				if( GUI.changed ){
					tube.uvRectCap = uvRectCap;
					dirty = true;
					GUI.changed = false;
				}
				
				// uv rect cap mirrored //
				if( tube.caps == TubeRenderer.CapMode.Both || tube.caps == TubeRenderer.CapMode.End ){
					bool uvRectCapEndMirrored = EditorGUILayout.Toggle( "Cap End Mirrored", tube.uvRectCapEndMirrored );
					if( GUI.changed ){
						tube.uvRectCapEndMirrored = uvRectCapEndMirrored;
						dirty = true;
						GUI.changed = false;
					}
				}
			}
			
			EditorGUI.indentLevel -= 2;
		}
		
		// mesh gizmos //
		tube.meshGizmos = EditorGUILayout.Toggle( "Mesh Gizmos", tube.meshGizmos );
		if( GUI.changed ) GUI.changed = false;
		if( tube.meshGizmos ){
			tube.meshGizmoLength = EditorGUILayout.Slider( "Mesh Gizmo Length", tube.meshGizmoLength, 0.01f, 1f );
			if( GUI.changed ) GUI.changed = false;
		}
		
		// update and store the tube //
		if( dirty ){
			EditorUtility.SetDirty(tube);
			if( EditorApplication.isPlaying || EditorApplication.isPaused ){
				tube.ForceUpdate();
			}
		}
	}
	
	
	
	public void OnSceneGUI()
	{
		// labels
		if( tube.points.Length > 1 ){
			Handles.Label( tube.transform.localToWorldMatrix.MultiplyPoint( tube.points[0] ), "p0" );
			Handles.Label( tube.transform.localToWorldMatrix.MultiplyPoint( tube.points[tube.points.Length-1] ), "p" + (tube.points.Length-1) );
		}
		
		// TODO: Handles. this has be carefully thought through before it's integrated
		/*
		Vector3[] newPoints = new Vector3[tube.points.Length];
		for( int p=0; p<tube.points.Length; p++ ){
			newPoints[p] = tube.transform.worldToLocalMatrix.MultiplyPoint( Handles.PositionHandle( tube.transform.localToWorldMatrix.MultiplyPoint( tube.points[p]), Quaternion.identity ) );
		}
		if( GUI.changed ){
			tube.points = newPoints;
			EditorUtility.SetDirty( tube );
		}
		*/
	}

	
	public static int ArrayCountField( string label, int value )
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( "Count", GUILayout.Width(109) );
		EditorGUI.indentLevel -= 2;
		int newCount = EditorGUILayout.IntField( value, GUILayout.Width( 159 ) );
		EditorGUI.indentLevel += 2;
		EditorGUILayout.EndHorizontal();
		return newCount;
	}
	
	
	public static Vector3 ArrayVector3Field( string label, Vector3 value )
	{
	    EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( label, GUILayout.Width(100) );
	        EditorGUI.indentLevel -= 2;
	        value.x = EditorGUILayout.FloatField( value.x, GUILayout.Width(60) );
			EditorGUI.indentLevel--;
	        value.y = EditorGUILayout.FloatField( value.y, GUILayout.Width(50) );
			EditorGUI.indentLevel--;
	        value.z = EditorGUILayout.FloatField( value.z, GUILayout.Width(50) );
			EditorGUI.indentLevel += 4;
	    EditorGUILayout.EndHorizontal();
	    return value;
	}
	
	
	public static float ArrayFloatField( string label, float value )
	{
	    EditorGUILayout.BeginHorizontal();
			value = EditorGUILayout.FloatField( label, value );
			GUILayout.FlexibleSpace();
	    EditorGUILayout.EndHorizontal();
	    return value;
	}
	
	
	public static Color ArrayColorField( string label, Color value )
	{
	    EditorGUILayout.BeginHorizontal();
			value = EditorGUILayout.ColorField( label, value );
			GUILayout.FlexibleSpace();
	    EditorGUILayout.EndHorizontal();
	    return value;
	}
}
