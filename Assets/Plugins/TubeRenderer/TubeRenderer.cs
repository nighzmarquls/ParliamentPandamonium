/*
	TubeRenderer
	v 1.4
	
	
	DOCUMENTATION
	=============
	
	Local
		/Assets/Asset Store/TubeRenderer/Reference.md
	
	Remote
		http://sixthsensor.dk/assetstore/tuberenderer/
	
	
	CHANGES
	=======

	v 1.4 (Aug 27, 2014)
		- Added 'Skin' example showing how to apply a SkinnedMeshRenderer to a tube.
		- Fixed minor uv mapping bug for caps when using HardEdges.
		- Optimised vector calculations (using Set).
		- Optimised color updates and converted external colors to Color32.
		- Added new method 'GetRotationAtPoint'.
	
	v 1.3 (Nov 29, 2013)
		- Added 'normalMode': define how to render normals; Smooth, Hard or HardEdges.
		- Updated a few of the examples.
		- Optimised calculation of mesh data (even more).
		â€“ Renamed 'drawMeshGizmos' to 'meshGizmos'.
		â€“ Renamed 'drawMeshGizmosLength' to 'meshGizmoLength'.
	
	v 1.2 (Nov 1, 2013)
		- Converted all methods for setting and getting tube data to properties.
 		- Added 'caps': open or close the end points of your tube.
		- Added 'uvRect', 'uvRectCap' and 'uvRectCapEndMirrored': create custom uv mappings.
		- Added 'forwardAngleOffset': rotate tube around it's forward direction.
		- Added 'AddPostprocess', 'RemovePostprocess' and 'postprocessContinously': post process the mesh of your tube.
		- Added 'MarkDynamic' shortcut to mesh.MarkDynamic(): optimise for runtime manipulation.
		- Added 'drawMeshGizmos' and 'drawMeshGizmosLength': display gizmos for normals and tangents.
		- Implemented modulo iteration for cases where the length of point, radius and color arrays don't match.
		- Perfected normals and tangents for tubes with variable radiuses.
		- Fixed minor update issue with 'calculateTangents' and 'invertMesh'.
		- Fixed rotation issue with doubled points.
		- Added custom inspector and enabled editor execution.
		- Moved documentation to 'AssetStore/TubeRenderer/Reference.md' and updated it.
		- Created remote documentation at 'http://sixthsensor.dk/code/unity/tuberenderer/'
		- Created new examples and updated the old.
	
	v 1.1 (Jun 25, 2013)
		- Fixed normal magnitude error.
		- Added inline documentation.
		- Changed Color to Color32 internally.
		- Changed the 'EdgeCount' method to a 'edgeCount' property.
		- Moved 'TubeRenderer' folder inside 'Asset Store' folder.
	
	v 1.0 (May 1, 2012)
		- Initial asset store version.
	
	
	TODO
	=======
		â€“ NormalMode.HardSegments
		- Fancy editor modeling tools?
		- Squeezes per point (Vector2)?
		- Replace with costum circle lookup?
		- Grass example?
		- Include Bezier.cs in TubeRenderer?
		
		
	AUTHOR
	======
	Carl Emil Carlsen
	http://sixthsensor.dk
	May 2012 â€“ Nov 2013
	
	This is a Unity Asset Store product.
	https://www.assetstore.unity3d.com/#/content/3281/
*/


using UnityEngine;
using System.Collections;

[ AddComponentMenu( "Effects/TubeRenderer" ) ]
[ RequireComponent( typeof( MeshFilter ) ) ]
[ RequireComponent( typeof( MeshRenderer ) ) ]
[ ExecuteInEditMode ]

public class TubeRenderer : MonoBehaviour
{
	/// <summary>
	/// Center points for the tube. Forward rotations will be calculated from the point at index 0 and upwards. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public Vector3[] points {
		get { return _points; }
		set {
			if( value.Length == 1 ){
				Debug.LogWarning( "Points ignored. Array must have at least two points." );
				return;
			}
			if( value.Length != _points.Length ){
				dirtyTriangles = true;
				dirtyUVs = true;
				dirtyColors = true;
			}
			dirtyRotations = true;
			if( radiuses != null ) dirtySteepnessAngles = true;
			redrawFlag = true;
			_points = value;
			UpdateTargetVertexCount();
		}
	}
	[SerializeField] Vector3[] _points = new Vector3[0];
	
	/// <summary>
	/// Radius values for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. If 'radius' has been set then 'radiuses' will be ignored. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public float[] radiuses {
		get { return _radiuses; }
		set {
			if( value != null && value.Length == 0 ){
				_radiuses = null;
			} else {
				_radiuses = value;
			}
			dirtySteepnessAngles = true;
			redrawFlag = true;
		}
	}
	[SerializeField] float[] _radiuses;
	
	/// <summary>
	/// Radius for the entire tube. If 'radiusses' has been set then 'radius' will be ignored. Default is 0.1.
	/// </summary>
	public float radius {
		get { return _radius; }
		set {
			if( _radiuses != null ){
				_radiuses = null;
				steepnessAngles = null;
			}
			if( value == _radius ) return;
			redrawFlag = true;
			_radius = value;
		}
	}
	[SerializeField] float _radius = 0.1f;
	
	/// <summary>
	/// Vertex colors for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. The array is NOT copied; the tube will keep the reference to the array.
	/// </summary>
	public Color32[] colors {
		get { return _pointColors; }
		set {
			if( value != null && value.Length == 0 ){
				_pointColors = null;
			} else {
				_pointColors = value;
			}
			dirtyColors = true;
		}
	}
	[SerializeField] Color32[] _pointColors;
	
	/// <summary>
	/// Edge resolution. Minimum is 3. Default is 12.
	/// </summary>
	public int edgeCount {
		get { return _edgeCount; }
		set {
			if( value == _edgeCount ) return;
			if( value < 3 ){
				Debug.LogWarning( "TubeRenderer must have at three edges." );
				return;
			}
			dirtyTriangles = true;
			dirtyUVs = true;
			dirtyCircle = true;
			dirtyColors = true;
			redrawFlag = true;
			_edgeCount = value;
			if( _points.Length >= 2 ) UpdateTargetVertexCount();
		}
	}
	[SerializeField] int _edgeCount = 12; // minimum is three
	
	/// <summary>
	/// Calculation of tangents. Default is false (to boost performance).
	/// </summary>
	public bool calculateTangents {
		get { return _calculateTangents; }
		set {
			if( value == _calculateTangents ) return;
			if( !value ){
				tangents = null;
				_mesh.tangents = null;
			}
			redrawFlag = true;
			_calculateTangents = value;
		}
	}
	[SerializeField] bool _calculateTangents;
	
	/// <summary>
	/// Mesh inversion (render the tube inside out). In most cases you should do 'Cull Front' in your shader instead. Default is false.
	/// </summary>
	public bool invertMesh {
		get { return _invertMesh; }
		set {
			if( value == _invertMesh ) return;
			dirtyTriangles = true;
			redrawFlag = true;
			_invertMesh = value;
		}
	}
	[SerializeField] bool _invertMesh;

	/// <summary>
	/// How normals are rendered. Default is NormalMode.Smooth.
	/// </summary>
	public NormalMode normalMode {
		get { return _normalMode; }
		set {
			if( value == _normalMode ) return;
			dirtyTriangles = true;
			dirtyUVs = true;
			dirtyColors = true;
			dirtyCircle = true;
			redrawFlag = true;
			_normalMode = value;
			UpdateTargetVertexCount();
		}
	}
	[SerializeField] NormalMode _normalMode = NormalMode.Smooth;
	public enum NormalMode { Smooth, Hard, HardEdges }

	/// <summary>
	/// Closed end points. Default is true.
	/// </summary>
	public CapMode caps {
		get { return _caps; }
		set {
			if( value == _caps ) return;
			dirtyColors = true;
			dirtyUVs = true;
			dirtyTriangles = true;
			redrawFlag = true;
			_caps = value;
			UpdateTargetVertexCount();
		}
	}
	[SerializeField] CapMode _caps = CapMode.Both;
	public enum CapMode { None, Begin, End, Both }
	
	/// <summary>
	/// Postprocess continously (if AddPostprocess has been called). When true, postprocesses will be called every update. When false, they will only be called when tube properties are changed. Default is true.
	/// </summary>
	public bool postprocessContinously {
		get { return _postprocessContinously; }
		set { _postprocessContinously = value; }
	}
	[SerializeField] bool _postprocessContinously = true;
	
	/// <summary>
	/// UV mapping rect for wrapped tube body. Default is Rect(0,0,1,1).
	/// </summary>
	public Rect uvRect {
		get { return _uvRect; }
		set {
			if( value == _uvRect ) return;
			dirtyUVs = true;
			_uvRect = value;
		}
	}
	[SerializeField] Rect _uvRect = new Rect(0,0,1,1);
	
	/// <summary>
	/// UV mapping rect for tube caps (if caps is true). Default is Rect(0,0,1,1).
	/// </summary>
	public Rect uvRectCap {
		get { return _uvRectCap; }
		set {
			if( value == _uvRectCap ) return;
			dirtyUVs = true;
			_uvRectCap = value;
		}
	}
	[SerializeField] Rect _uvRectCap = new Rect(0,0,1,1);
	
	/// <summary>
	/// Mirrored uv mapping for cap at end point (points[points.Length-1]). Default is false.
	/// </summary>
	public bool uvRectCapEndMirrored {
		get { return _uvRectCapEndMirrored; }
		set {
			if( value == _uvRectCapEndMirrored ) return;
			dirtyUVs = true;
			_uvRectCapEndMirrored = value;
		}
	}
	[SerializeField] bool _uvRectCapEndMirrored;
	
	/// <summary>
	/// Rotation offset around the tubes forward direction.
	/// </summary>
	public float forwardAngleOffset {
		get { return _forwardAngleOffset; }
		set {
			if( value == _forwardAngleOffset ) return;
			dirtyRotations = true;
			redrawFlag = true;
			_forwardAngleOffset = value;
		}
	}
	[SerializeField] float _forwardAngleOffset;
	
	/// <summary>
	/// Get the tube mesh. Useful for combining multiple tubes into a static mesh. Do not manipulate directly.
	/// </summary>
	public Mesh mesh { get{ return _mesh; } }
	[SerializeField] Mesh _mesh;
	
	/// <summary>
	/// Draw gizmos for mesh normals and tangents. Default is false.
	/// </summary>
	public bool meshGizmos;
	
	/// <summary>
	/// Length of gizmos for mesh normals and tangents. Default is 0.1.
	/// </summary>
	public float meshGizmoLength = 0.1f;


	[SerializeField] int targetVertexCount;
	
	Vector3[] vertices;
	Vector3[] normals;
	int[] triangles;
	Vector2[] uvs;
	Vector4[] tangents;
	Color32[] colors32;
	Vector3[] circlePointLookup;
	Vector3[] circleNormalLookup;
	Vector3[] circleTangentLookup;
	Quaternion[] rotations;
	Vector3[] directions;
	float[] steepnessAngles;
	Vector3 pastUp;
	
	MeshFilter filter;
	
	bool dirtyCircle;
	bool dirtyUVs;
	bool dirtyTriangles;
	bool dirtyRotations;
	bool dirtySteepnessAngles;
	bool dirtyColors;
	bool redrawFlag;
	
	const float Tau = Mathf.PI * 2;
	
	
	
	////////////
	// PUBLIC //
	////////////
	
	
	/// <summary>
	/// Force update to generate the tube mesh immediately.
	/// </summary>
	public void ForceUpdate()
	{
		Update();
	}
	
	
	/// <summary>
	/// Shortcut to Mesh.MarkDynamic(). Call this if the tube will be updated often so that Unity can optimise memory use.
	/// </summary>
	public void MarkDynamic()
	{
		_mesh.MarkDynamic();
	}
	
	
	/// <summary>
	/// Add a method to receive and manipulate mesh data before it is applied. Useful for creating distortion effects or complex variations.
	/// </summary>
	public void AddPostprocess( Postprocess postprocess )
	{
		Postprocesses += postprocess;
	}
	
	
	/// <summary>
	/// Remove a postprocess method that have previously been assigned using the 'AddPostprocess' method.
	/// </summary>
	public void RemovePostprocess( Postprocess postprocess )
	{
		Postprocesses -= postprocess;
	}
	
	/// <summary>
	/// Method for passing mesh data.
	/// </summary>
	public delegate void Postprocess( Vector3[] vertices, Vector3[] normals, Vector4[] tangents );
	Postprocess Postprocesses;


	/// <summary>
	/// Gets the rotation at point.
	/// </summary>
	public Quaternion GetRotationAtPoint( int index )
	{
		if( index < 0 || index > rotations.Length-1 ) return Quaternion.identity;
		return rotations[index];
	}

	
	
	/////////////
	// PRIVATE //
	/////////////
	
	
	void Awake()
	{
		// ensure mesh filter //
		filter = gameObject.GetComponent<MeshFilter>();
		if( filter == null ) filter = gameObject.AddComponent<MeshFilter>();
		
		// ensure mesh //
		if( _mesh == null ){
			NewMesh();
		} else if( !Application.isPlaying ){
			// this is kind of a hack to ensure that doublicated tubes have unique meshes
			// we don't check in runtime because FindObjectsOfType() is slow, so dublicates will still fuck up in runtime.
			TubeRenderer[] tubes = GameObject.FindObjectsOfType( typeof( TubeRenderer ) ) as TubeRenderer[];
			for( int t=0; t<tubes.Length; t++ ){
				if( tubes[t] != this && tubes[t].mesh == _mesh ){
					NewMesh();
					break;
				}
			}
		}
		
		// ensure mesh renderer and material //
		if( GetComponent<Renderer>() == null ) gameObject.AddComponent<MeshRenderer>();
		if( GetComponent<Renderer>().sharedMaterial == null ) GetComponent<Renderer>().sharedMaterial = new Material( Shader.Find( "Diffuse" ) );
	}
	
	
	void NewMesh()
	{
		_mesh = new Mesh();
		_mesh.name = "Tube_" + gameObject.GetInstanceID();
		filter.sharedMesh = _mesh;
	}
	
	
	void Update()
	{
		// the tube needs points //
		if( _points.Length == 0 ){
			if( _mesh.vertexCount > 0 ) _mesh.Clear();
			return;
		}
		
		// when postprocessing we need to recalculate mesh data //
		if( Postprocesses != null && _postprocessContinously ) redrawFlag = true;
		
		// ensure that intermediate arrays are defined (editor issue) //
		if( redrawFlag ){
			if( circleNormalLookup == null ) dirtyCircle = true;
			if( uvs == null ) dirtyUVs = true;
			if( rotations == null ) dirtyRotations = true;
			if( triangles == null ) dirtyTriangles = true;
		}
		
		// update only what needs updating //
		if( dirtyCircle ) UpdateCircleLookup();
		if( dirtyRotations ) UpdateRotations();
		if( dirtyTriangles ) UpdateTriangles();
		if( dirtySteepnessAngles ) UpdateSteepnessAngles();
		if( redrawFlag ) ReDraw(); // updates vertices, normals and tangents
		if( dirtyUVs ) UpdateUVs(); // does not require redraw
		if( dirtyColors ) UpdateColors(); // does not require redraw
		
		// reset flags //
		dirtyCircle = false;
		dirtyUVs = false;
		dirtyRotations = false;
		dirtyTriangles = false;
		dirtySteepnessAngles = false;
		dirtyColors = false;
		redrawFlag = false;
	}
	
	
	void OnDrawGizmos()
	{
		if( meshGizmos ){
			Gizmos.matrix = transform.localToWorldMatrix;
			for( int v=0; v<vertices.Length; v++ )
			{
				// normals //
				Gizmos.color = new Color( 0, 0, 1, 0.5f );
				Gizmos.DrawLine( vertices[v], vertices[v] + normals[v] * meshGizmoLength );
				
				// tangents //
				if( tangents != null ){
					if( tangents[v].w == -1 ) Gizmos.color = new Color( 1, 0, 0, 0.5f );
					else if( tangents[v].w == 1 ) Gizmos.color = new Color( 1, 1, 0, 0.5f );
					else Gizmos.color = Color.white;
					Gizmos.DrawLine( vertices[v], vertices[v] + new Vector3( tangents[v].x, tangents[v].y, tangents[v].z ) * meshGizmoLength );
				}
			}
		}
	}
	
	
	void ReDraw()
	{
		// update array length //
		if( vertices == null || vertices.Length != targetVertexCount ){
			vertices = new Vector3[ targetVertexCount ];
			normals = new Vector3[ targetVertexCount ];
			mesh.Clear();
		}
		if( calculateTangents && ( tangents == null || tangents.Length != targetVertexCount ) ){
			tangents = new Vector4[ targetVertexCount ];
		}
		
		int v = 0;
		Matrix4x4 matrix = new Matrix4x4();

		// calculate vertices and update bounds //
		Vector3 minBounds = new Vector3( 10000, 10000, 10000 );
		Vector3 maxBounds = new Vector3( -10000, -10000, -10000 );
		for( int p=0; p<_points.Length; p++ )
		{
			if( _radiuses != null && _radiuses.Length != 0 ){
				int rad = p % radiuses.Length;
				// create transform matrix //
				matrix.SetTRS( _points[ p ], rotations[ p ], Vector3.one * _radiuses[ rad ] );
				// check min and max bounds //
				if( _points[ p ].x - _radiuses[ rad ] < minBounds.x ) minBounds.x = _points[ p ].x - _radiuses[ rad ];
				if( _points[ p ].y - _radiuses[ rad ] < minBounds.y ) minBounds.y = _points[ p ].y - _radiuses[ rad ];
				if( _points[ p ].z - _radiuses[ rad ] < minBounds.z ) minBounds.z = _points[ p ].z - _radiuses[ rad ];
				if( _points[ p ].x + _radiuses[ rad ] > maxBounds.x ) maxBounds.x = _points[ p ].x + _radiuses[ rad ];
				if( _points[ p ].y + _radiuses[ rad ] > maxBounds.y ) maxBounds.y = _points[ p ].y + _radiuses[ rad ];
				if( _points[ p ].z + _radiuses[ rad ] > maxBounds.z ) maxBounds.z = _points[ p ].z + _radiuses[ rad ];
			} else {
				// create transform matrix //
				matrix.SetTRS( _points[ p ], rotations[ p ], Vector3.one * _radius );
				// check min and max bounds //
				if( _points[ p ].x - _radius < minBounds.x ) minBounds.x = _points[ p ].x - _radius;
				if( _points[ p ].y - _radius < minBounds.y ) minBounds.y = _points[ p ].y - _radius;
				if( _points[ p ].z - _radius < minBounds.z ) minBounds.z = _points[ p ].z - _radius;
				if( _points[ p ].x + _radius > maxBounds.x ) maxBounds.x = _points[ p ].x + _radius;
				if( _points[ p ].y + _radius > maxBounds.y ) maxBounds.y = _points[ p ].y + _radius;
				if( _points[ p ].z + _radius > maxBounds.z ) maxBounds.z = _points[ p ].z + _radius;
			}
			
			// calculate vertices //
			for( int e=0; e<_edgeCount; e++ ) vertices[v++] = matrix.MultiplyPoint3x4( circlePointLookup[e] );
			vertices[v] = vertices[v-edgeCount]; // uv wrapping //
			v++;
		}

		// add caps //
		switch( _normalMode ){
			case NormalMode.Smooth: break;
			case NormalMode.Hard: v = (_points.Length-1) * _edgeCount * 4; break;
			case NormalMode.HardEdges: v = _points.Length * _edgeCount * 2; break;
		}
		int invertSign = _invertMesh ? -1 : 1;
		if( _caps == CapMode.Both || _caps == CapMode.Begin ){
			Vector3 normal = rotations[0] * Vector3.back * invertSign;
			Vector4 tangent = rotations[0] * Vector3.right;
			tangent.w = -1;
			for( int e=0; e<_edgeCount+1; e++ ){
				vertices[v] = vertices[e];
				normals[v] = normal;
				if( calculateTangents ) tangents[v] = tangent;
				v++;
			}
			vertices[v] = _points[0]; // center vertex
			normals[v] = normal;
			if( calculateTangents ) tangents[v] = tangent;
		}
		if( _caps == CapMode.Both || _caps == CapMode.End ){
			Vector3 normal = rotations[_points.Length-1] * Vector3.forward * invertSign;
			Vector4 tangent = rotations[_points.Length-1] * Vector3.left;
			tangent.w = -1;
			int vBegin = (_points.Length-1)*(_edgeCount+1);
			if( _caps == CapMode.Both ) v++;
			for( int e=0; e<_edgeCount+1; e++ ){
				vertices[v] = vertices[ vBegin+e ];
				normals[v] = normal;
				if( calculateTangents ) tangents[v] = tangent;
				v++;
			}
			vertices[v] = _points[_points.Length-1]; // center vertex
			normals[v] = normal;
			if( calculateTangents ) tangents[v] = tangent;
		}

		// draw tube in requested normal mode //
		switch( _normalMode ){
			case NormalMode.Smooth: ReDrawSmoothNormals(); break;
			case NormalMode.Hard: ReDrawHardNormals(); break;
			case NormalMode.HardEdges: ReDrawHardNormalEdges(); break;
		}
		
		// post process //
		if( Postprocesses != null ) Postprocesses( vertices, normals, tangents );
		
		// update mesh (note that uvs and colors are set in their update methods) //
		_mesh.vertices = vertices;
		if( dirtyTriangles ) _mesh.triangles = triangles;
		_mesh.normals = normals;
		if( calculateTangents ) _mesh.tangents = tangents;
		
		// update bounds //
		Vector3 boundsSize = new Vector3( maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, maxBounds.z - minBounds.z );
		Vector3 boundsCenter = new Vector3( minBounds.x + boundsSize.x * 0.5f, minBounds.y + boundsSize.y * 0.5f, minBounds.z + boundsSize.z * 0.5f );
		_mesh.bounds = new Bounds( boundsCenter, boundsSize );
	}


	void ReDrawSmoothNormals()
	{
		int invertSign = _invertMesh ? -1 : 1;
		int v = 0;
		for( int p=0; p<_points.Length; p++ ) {
			for( int e=0; e<_edgeCount; e++ ){
				if( radiuses == null ){
					normals[v] = rotations[p] * circleNormalLookup[e] * invertSign;
				} else {
					normals[v] = rotations[p] * Quaternion.AngleAxis( steepnessAngles[p], circleTangentLookup[e] ) * circleNormalLookup[e] * invertSign;
				}
				if( calculateTangents ){
					tangents[v] = Vector3.Cross( rotations[p] * circleTangentLookup[e], normals[v] );
					tangents[v].w = -1;
				}
				v++;
			}
			// uv wrapping
			normals[v] = normals[v-edgeCount];
			if( _calculateTangents ) tangents[v] = tangents[v-edgeCount];
			v++;
		}
	}


	void ReDrawHardNormals()
	{
		// yes, it may seem ugly, but the method below is faster than calculating overlapping vertices //
		int v;

		// store the first two segments temporarily to avoid overwriting what we are about to read //
		Vector3[] verticesTemp = new Vector3[(_edgeCount+1)*2];
		for( int p=0; p<2; p++ ){
			for( int e=0; e<_edgeCount+1; e++ ){
				v = p*(_edgeCount+1)+e;
				verticesTemp[v] = vertices[v];
			}
		}

		// go backwards and copy from already calculated vertices //
		int[] quad = new int[]{ 0, _edgeCount+1, _edgeCount+2, 1 };
		for( int p=_points.Length-2; p>0; p-- ){ // don't copy vertices from first circle
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 4 + e * 4;
				int lv = p * (_edgeCount+1) + e;
				Vector3 normal = Vector3.Cross( vertices[lv+quad[3]] - vertices[lv+quad[0]], vertices[lv+quad[1]] - vertices[lv+quad[0]] ).normalized;
				for( int q=0; q<4; q++ ){
					vertices[v] = vertices[lv+quad[q]];
					normals[v] = normal;
					v++;
				}
			}
		}

		// copy from the temporary first two segments //
		for( int e=_edgeCount-1; e>=0; e-- ){
			v = e * 4;
			Vector3 normal = Vector3.Cross( verticesTemp[e+quad[3]] - verticesTemp[e], verticesTemp[e+quad[2]] - verticesTemp[e] ).normalized;
			for( int q=0; q<4; q++ ){
				normals[v] = normal;
				vertices[v] = verticesTemp[e+quad[q]];
				v++;
			}
		}

		// calculate tangents //
		if( _calculateTangents ){
			if( _radiuses == null ){
				for( int p=0; p<_points.Length-1; p++ ){
					Vector4 tangent = directions[p].normalized;
					tangent.w = -1;
					v = p * _edgeCount * 4;
					for( int e=0; e<_edgeCount; e++ ) for( int q=0; q<4; q++ ) tangents[v++] = tangent;
				}
			} else {
				for( int p=0; p<_points.Length-1; p++ ){
					for( int e=0; e<_edgeCount; e++ ){
						v = p * _edgeCount * 4 + e * 4;
						if( e == 0 ){
							tangents[v] = ( vertices[v+1] - vertices[v] ).normalized;
							tangents[v].w = -1;
						} else {
							tangents[v] = tangents[v-1];
						}
						if( e == _edgeCount-1 ){
							tangents[v+2] = tangents[v-(_edgeCount-1)*4];
						} else {
							tangents[v+2] = ( vertices[v+2] - vertices[v+3] ).normalized;
							tangents[v+2].w = -1;
						}
						tangents[v+1] = tangents[v];
						tangents[v+3] = tangents[v+2];
					}
				}
			}
		}
	}


	void ReDrawHardNormalEdges()
	{
		int v;

		// store the first two segments temporarily to avoid overwriting what we are about to read //
		Vector3[] verticesTemp = new Vector3[(_edgeCount+1)*2];
		for( int p=0; p<2; p++ ){
			for( int e=0; e<_edgeCount+1; e++ ){
				v = p * (_edgeCount+1) + e;
				verticesTemp[v] = vertices[v];
			}
		}

		// go backwards and copy from already calculated vertices //
		int invertSign = _invertMesh ? -1 : 1;
		for( int p=_points.Length-1; p>1; p-- ){ // don't copy vertices from first segment
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 2 + e * 2;
				int lv = p * (_edgeCount+1) + e;
				if( radiuses == null ){
					normals[v] = rotations[p] * circleNormalLookup[e] * invertSign;
				} else {
					normals[v] = rotations[p] * Quaternion.AngleAxis( steepnessAngles[p], circleTangentLookup[e] ) * circleNormalLookup[e] * invertSign;
				}
				normals[v+1] = normals[v];
				vertices[v] = vertices[lv];
				vertices[v+1] = vertices[lv+1];
			}
		}

		// copy from the temporary first two segments //
		for( int p=1; p>=0; p-- ){
			for( int e=_edgeCount-1; e>=0; e-- ){
				v = p * _edgeCount * 2 + e * 2;
				int lv = p * (_edgeCount+1) + e;
				if( radiuses == null ){
					normals[v] = rotations[p] * circleNormalLookup[e] * invertSign;
				} else {
					normals[v] = rotations[p] * Quaternion.AngleAxis( steepnessAngles[p], circleTangentLookup[e] ) * circleNormalLookup[e] * invertSign;
				}
				normals[v+1] = normals[v];
				vertices[v] = verticesTemp[lv];
				vertices[v+1] = verticesTemp[lv+1];
			}
		}

		// calculate tangents //
		if( _calculateTangents ){
			if( _radiuses == null ){
				for( int p=0; p<_points.Length; p++ ){
					Vector4 tangent = directions[p].normalized;
					tangent.w = -1;
					v = p * _edgeCount * 2;
					for( int e=0; e<_edgeCount; e++ ){
						tangents[v++] = tangent;
						tangents[v++] = tangent;
					}
				}
			} else {
				for( int p=0; p<_points.Length; p++ ){
					for( int e=0; e<_edgeCount; e++ ){
						v = p * _edgeCount * 2 + e * 2;
						if( e == 0 ){
							tangents[v] = Vector3.Cross( rotations[p] * circleTangentLookup[e], normals[v] );
							tangents[v].w = -1;
						} else {
							tangents[v] = tangents[v-1];
						}
						if( e == _edgeCount-1 ){
							tangents[v+1] = tangents[v-(_edgeCount-1)*2];
						} else {
							tangents[v+1] = Vector3.Cross( rotations[p] * circleTangentLookup[e], normals[v+1] );
							tangents[v+1].w = -1;
						}
					}
				}
			}
		}
	}


	void UpdateTargetVertexCount()
	{
		switch( _normalMode ){
			case NormalMode.Smooth: targetVertexCount = _points.Length * (_edgeCount+1); break;
			case NormalMode.Hard: targetVertexCount = (_points.Length-1) * _edgeCount * 4; break;
			case NormalMode.HardEdges: targetVertexCount = _points.Length * 2 * _edgeCount; break;
		}
		if( _caps == CapMode.Both ) targetVertexCount += ( (_edgeCount+1) + 1 ) * 2;
		else if( _caps == CapMode.Begin || _caps == CapMode.End ) targetVertexCount += (_edgeCount+1) + 1;
	}
	
	
	void UpdateCircleLookup()
	{
		if( circlePointLookup == null || circlePointLookup.Length != _edgeCount ){
			circlePointLookup = new Vector3[ _edgeCount ];
			circleNormalLookup = new Vector3[ _edgeCount ];
			circleTangentLookup = new Vector3[ _edgeCount ];
		}
		
		float normalizer = 1 / (float) _edgeCount;
		for( int e=0; e<_edgeCount; e++ ) {
			float pointAngle = e * normalizer * Tau;
			circlePointLookup[e] = new Vector3( Mathf.Cos( pointAngle ), Mathf.Sin( pointAngle ), 0 );
			if( _normalMode == NormalMode.HardEdges ){
				float normalAngle = pointAngle + normalizer * Mathf.PI;
				circleNormalLookup[e] = new Vector3( Mathf.Cos( normalAngle ), Mathf.Sin( normalAngle ), 0 );
			} else {
				circleNormalLookup[e] = circlePointLookup[e];
			}
			circleTangentLookup[e] = Vector3.Cross( circleNormalLookup[e], Vector3.forward );
		}
	}
	
	
	void UpdateSteepnessAngles()
	{
		if( steepnessAngles == null || steepnessAngles.Length != _points.Length ) steepnessAngles = new float[ _points.Length ];
		
		float[] radiusDiffs = new float[ _points.Length-1 ];
		for( int p=0; p<_points.Length-1; p++ ){
			radiusDiffs[p] = _radiuses[ (p+1)%_radiuses.Length ] - _radiuses[ p%_radiuses.Length ];
		}
		
		for( int p=0; p<_points.Length-1; p++ ){
			float avgRadiusDiff;
			if( p == 0 ) avgRadiusDiff = radiusDiffs[0];
			else avgRadiusDiff = ( radiusDiffs[p] + radiusDiffs[p-1] ) * 0.5f;
			if( avgRadiusDiff == 0 ) steepnessAngles[p] = 0;
			else steepnessAngles[p] = -Mathf.Atan2( avgRadiusDiff, directions[p].magnitude ) * Mathf.Rad2Deg;
		}
		steepnessAngles[_points.Length-1] = -Mathf.Atan2( radiusDiffs[radiusDiffs.Length-1], directions[directions.Length-1].magnitude ) * Mathf.Rad2Deg;
	}
	
	
	void UpdateRotations()
	{
		Vector3 forward, up;
		
		// update array lengths //
		if( rotations == null || _points.Length != rotations.Length ){
			rotations = new Quaternion[ _points.Length ];
			directions = new Vector3[ _points.Length ];
		}
		
		// calculate directions //
		for( int p=0; p<points.Length-1; p++ ) directions[p].Set( _points[p+1].x - _points[p].x, _points[p+1].y - _points[p].y, _points[p+1].z - _points[p].z );
		
		// fix directions for doubled points //
		for( int p=0; p<points.Length-1; p++ ){
			if( directions[p] == Vector3.zero ){
				if( p+1 < directions.Length-2 && directions[p+1] != Vector3.zero ){
					directions[p].Set( directions[p+1].x, directions[p+1].y, directions[p+1].z );
				} else if( p-1 > 0 && directions[p-1] != Vector3.zero ){
					directions[p].Set( directions[p-1].x, directions[p-1].y, directions[p-1].z );
				}
			}
		}
		
		// doublicate last direction //
		directions[ _points.Length-1 ] = directions[ _points.Length-2 ];
		
		// if the up direction has not been set in last frame then use default //
		if( pastUp == Vector3.zero ){
			up = directions[0].x == 0 && directions[0].z == 0 ? Vector3.right : Vector3.up;
		} else {
			up = pastUp;
		}

		forward = Vector3.zero;
		for( int p=0; p<points.Length; p++ )
		{
			// calculate averaged forward direction //
			if( p != 0 && p != _points.Length-1 ){
				forward.Set( directions[p].x + directions[p-1].x, directions[p].y + directions[p-1].y, directions[p].z + directions[p-1].z );
			} else {
				// this is the start or end point, check for tube loop //
				if( _points[0] == _points[ _points.Length-1 ] ) forward.Set( directions[_points.Length-1].x + directions[0].x, directions[_points.Length-1].y + directions[0].y, directions[_points.Length-1].z + directions[0].z );
				else forward.Set( directions[p].x, directions[p].y, directions[p].z );
			}
			
			// if the forward vector is zero (probably because the last
			// point was at the same position at this point) we don't
			// mind calculating the rotation.
			if( forward == Vector3.zero ){
				rotations[p] = Quaternion.identity;
				continue;
			}
				
			forward.Normalize();
			
			// To find the optimal up-rotation of the circle plane we do the following:
			// The cross product of lastUp and forward gives us a vector that is rotated 90
			// degrees right on the forward axis from the new up vector. Taking the cross 
			// product of forward and right gives us the up vector. We save the new up drectly
			// in the lastUp variable.
			// Vector3 up = Vector3.Cross( new Vector3( 0, 0, 1 ), new Vector3( 1, 0, 0 ) );
			// http://en.wikipedia.org/wiki/Right-hand_rule
			
			Vector3 right = Vector3.Cross( up, forward );
			up = Vector3.Cross( forward, right );
			
			if( p == 0 ){
				// store the up direction for the first point //
				pastUp = up;
				// offset
				if( _forwardAngleOffset != 0 ) up = Quaternion.AngleAxis( _forwardAngleOffset, forward ) * up;
			}
			
			// create a Quaternion rotation using LookRotation //
			rotations[p].SetLookRotation( forward, up );
		}
	}
	
	
	void UpdateTriangles()
	{
		// updaate array length //
		int triangleCount = (points.Length-1) * _edgeCount * 3 * 2;
		if( _caps == CapMode.Both || _caps == CapMode.Begin ) triangleCount += _edgeCount * 3;
		if( _caps == CapMode.Both || _caps == CapMode.End ) triangleCount += _edgeCount * 3;
		if( triangles == null || triangles.Length != triangleCount ) triangles = new int[triangleCount];

		// stitch the tube //
		int v=0; int t=0;
		int[] quad;
		switch( _normalMode ){

		case NormalMode.Smooth:
			if(!_invertMesh) quad = new int[]{ 0, 1, _edgeCount+2, 0, _edgeCount+2, _edgeCount+1 };
			else quad = new int[]{ 0, _edgeCount+2, 1, 0, _edgeCount+1, _edgeCount+2 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) triangles[ t++ ] = v + quad[ q ];
					v++;
				}
				v++; // skip hidden vertex
			}
			v += _edgeCount+1; // skip last point
			break;

		case NormalMode.Hard:
			if(!_invertMesh) quad = new int[]{ 0, 3, 1, 3, 2, 1 };
			else quad = new int[]{ 0, 1, 3, 3, 1, 2 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) triangles[ t++ ] = v + quad[ q ];
					v += 4;
				}
			}
			break;

		case NormalMode.HardEdges:
			if(!_invertMesh) quad = new int[]{ 0, 1, _edgeCount*2, _edgeCount*2, 1, _edgeCount*2+1 };
			else quad = new int[]{ 0, _edgeCount*2, 1, 1, _edgeCount*2, _edgeCount*2+1 };
			for( int p=0; p<points.Length-1; p++ ){
				for( int e=0; e<_edgeCount; e++ ){
					for( int q = 0; q < quad.Length; q++ ) triangles[ t++ ] = v + quad[ q ];
					v += 2;
				}
			}
			v += _edgeCount*2;
			break;

		}

		// stitch the begin cap //
		if( _caps  == CapMode.Both || _caps  == CapMode.Begin ){
			int vCenter = v + _edgeCount+1;
			if(!_invertMesh){ // ugly but fast
				for( int e=0; e<_edgeCount; e++ ){
					triangles[ t++ ] = v;
					triangles[ t++ ] = vCenter;
					triangles[ t++ ] = v+1;
					v++;
				}
			} else {
				for( int e=0; e<_edgeCount; e++ ){
					triangles[ t++ ] = v;
					triangles[ t++ ] = v+1;
					triangles[ t++ ] = vCenter;
					v++;
				}
			}
		}

		// stitch the end cap //
		if( _caps  == CapMode.Both || _caps  == CapMode.End ){
			if( _caps  == CapMode.Both ){
				v++; // skip hidden vertex
				v++; // skip center vertex
			}
			int vCenter = v + _edgeCount+1;
			if(!_invertMesh){ // ugly but fast
				for( int e=0; e<_edgeCount; e++ ){
					triangles[ t++ ] = v;
					triangles[ t++ ] = v+1;
					triangles[ t++ ] = vCenter;
					v++;
				}
			} else {
				for( int e=0; e<_edgeCount; e++ ){
					triangles[ t++ ] = v;
					triangles[ t++ ] = vCenter;
					triangles[ t++ ] = v+1;
					v++;
				}
			}
		}
	}


	void UpdateUVs()
	{
		float u, v;
		if( uvs == null || uvs.Length != targetVertexCount ) uvs = new Vector2[ targetVertexCount ];
		int uv = 0;
		float uNormalizer = 1 / ( _points.Length -1f );
		float vNormalizer = 1 / (float) _edgeCount;
		
		switch( _normalMode ){
			
		case NormalMode.Smooth:
			for( int p=0; p<points.Length; p++ ){
				u =  _uvRect.xMin + _uvRect.width * (p*uNormalizer);
				for( int e=0; e<_edgeCount+1; e++ ){
					v = _uvRect.yMin + _uvRect.height * (e*vNormalizer);
					uvs[ uv++ ] = new Vector2( u, v );
				}
			}
			break;
			
		case NormalMode.Hard:
			for( int p=0; p<points.Length-1; p++ ){
				u =  _uvRect.xMin + _uvRect.width * (p*uNormalizer);
				float nextU = _uvRect.xMin + _uvRect.width * ((p+1)*uNormalizer);
				for( int e=0; e<_edgeCount; e++ ){
					v = _uvRect.yMin + _uvRect.height * (e*vNormalizer);
					float nextV = _uvRect.yMin + _uvRect.height * ((e+1)*vNormalizer);
					uvs[ uv++ ] = new Vector2( u, v );
					uvs[ uv++ ] = new Vector2( nextU, v );
					uvs[ uv++ ] = new Vector2( nextU, nextV );
					uvs[ uv++ ] = new Vector2( u, nextV );
				}
			}
			break;
		case NormalMode.HardEdges:
			for( int p=0; p<points.Length; p++ ){
				u =  _uvRect.xMin + _uvRect.width * (p*uNormalizer);
				for( int e=0; e<_edgeCount; e++ ){
					v = _uvRect.yMin + _uvRect.height * (e*vNormalizer);
					float nextV = _uvRect.yMin + _uvRect.height * ((e+1)*vNormalizer);
					uvs[ uv++ ] = new Vector2( u, v );
					uvs[ uv++ ] = new Vector2( u, nextV );
				}
			}
			break;
		}

		if( _caps == CapMode.Both || _caps  == CapMode.Begin ){
			for( int e=0; e<_edgeCount; e++ ){
				u = _uvRectCap.yMin + _uvRectCap.height * ( circlePointLookup[e].y*0.5f+0.5f );
				v = _uvRectCap.xMin + _uvRectCap.width * ( 1-(circlePointLookup[e].x*0.5f+0.5f) );
				uvs[ uv++ ] = new Vector2( u, v );
			}
			uvs[uv] = uvs[uv-_edgeCount]; // uv wrap
			uv++;
			u = _uvRectCap.yMin + _uvRectCap.height * 0.5f;
			v = _uvRectCap.xMin + _uvRectCap.width * 0.5f;
			uvs[ uv++ ] = new Vector2( u, v ); // center
		}
		
		if( _caps == CapMode.Both || _caps  == CapMode.End ){
			for( int e=0; e<_edgeCount; e++ ){
				if( _uvRectCapEndMirrored ) u = _uvRectCap.yMin + _uvRectCap.height * ( circlePointLookup[e].y*0.5f+0.5f );
				else u = _uvRectCap.yMin + _uvRectCap.height * ( 1-(circlePointLookup[e].y*0.5f+0.5f) );
				v = _uvRectCap.xMin + _uvRectCap.width * ( 1-(circlePointLookup[e].x*0.5f+0.5f) );
				uvs[ uv++ ] = new Vector2( u, v );
			}
			uvs[uv] = uvs[uv-_edgeCount]; // uv wrap
			uv++;
			u = _uvRectCap.yMin + _uvRectCap.height * 0.5f;
			v = _uvRectCap.xMin + _uvRectCap.width * 0.5f;
			uvs[ uv++ ] = new Vector2( u, v ); // center
		}
		
		_mesh.uv = uvs;
	}

	
	void UpdateColors()
	{
		if( _pointColors != null && _pointColors.Length != 0 ){
			int v = 0;

			if( colors32 == null || colors32.Length != targetVertexCount ) colors32 = new Color32[ targetVertexCount ];

			switch( _normalMode ){

			case NormalMode.Smooth:
				for( int p=0; p<_points.Length; p++ ) {
					int c = p % _pointColors.Length;
					for( int s=0; s<_edgeCount+1; s++ ) colors32[ v++ ] = _pointColors[ c ];
				}
				break;

			case NormalMode.Hard:
				for( int p=0; p<_points.Length-1; p++ ) {
					int c0 = p % _pointColors.Length;
					int c1 = (p+1) % _pointColors.Length;
					for( int s=0; s<_edgeCount; s++ ){
						colors32[ v++ ] = _pointColors[ c0 ];
						colors32[ v++ ] = _pointColors[ c1 ];
						colors32[ v++ ] = _pointColors[ c1 ];
						colors32[ v++ ] = _pointColors[ c0 ];
					}
				}
				break;

			case NormalMode.HardEdges:
				for( int p=0; p<_points.Length; p++ ) {
					int c = p % _pointColors.Length;
					for( int s=0; s<_edgeCount; s++ ){
						colors32[ v++ ] = _pointColors[c];
						colors32[ v++ ] = _pointColors[c];
					}
				}
				break;
			}

			if( _caps == CapMode.Both || _caps == CapMode.Begin ){
				for( int s=0; s<_edgeCount+2; s++ ) colors32[ v++ ] = _pointColors[ 0 ]; // start cap
			}
			if( _caps == CapMode.Both || _caps == CapMode.End ){
				for( int s=0; s<_edgeCount+2; s++ ) colors32[ v++ ] = _pointColors[ (_points.Length-1) % colors.Length ]; // end cap
			}

			mesh.colors32 = colors32;
		}
	}
	
	
	
	
	////////////////
	// DEPRECATED //
	////////////////
	
	
	/// <summary>
	/// DEPRECATED. Setting point count is now handled indirectly.
	/// </param>
	[System.ObsoleteAttribute( "Setting point count is now handled indirectly.")]
	public void SetPointCount( int pointCount )
	{
		if( pointCount < 2 ){
			Debug.LogWarning( "TubeRenderer must have at two three _points." );
			return;
		}
		
		dirtyTriangles = true;
		dirtyUVs = true;
		dirtyColors = true;
		dirtyRotations = true;
		if( circleNormalLookup == null ) dirtyCircle = true;
		redrawFlag = true;
		
		targetVertexCount = pointCount * (_edgeCount+1);
		
		this.points = new Vector3[ pointCount ];
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'edgeCount' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'edgeCount' property instead.")]
	public void SetEdgeCount( int number )
	{
		edgeCount = number;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'points' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'points' property instead.")]
	public void SetPoints( Vector3[] points )
	{
		this.points = points;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'radius' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'radius' property instead.")]
	public void SetRadius( float radius )
	{
		this.radius = radius;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'radiuses' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'radiuses' property instead.")]
	public void SetRadiuses( float[] radiuses )
	{
		this.radiuses = radiuses;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'colors' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'colors' property instead.")]
	public void SetColors( Color32[] colors )
	{
		this.colors = colors;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'points' property instead and copy the array yourself if necessary.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'points' property instead and copy the array yourself if necessary.")]
	public Vector3[] Points(){
		Vector3[] copy = new Vector3[ _points.Length ];
		_points.CopyTo( copy, 0 );
		return copy;
	}
	
	
	/// <summary>
	/// DEPRECATED. Please use 'radiuses' property instead and copy the array yourself if necessary.
	/// </summary>
	[System.ObsoleteAttribute( "Deprecated. Please use 'radiuses' property instead and copy the array yourself if necessary.")]
	public float[] Radiuses(){
		float[] copy = new float[ _radiuses.Length ];
		_radiuses.CopyTo( copy, 0 );
		return copy;
	}

	/// <summary>
	//// DEPRECATED. Please use 'meshGizmos' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'meshGizmos' property instead.")]
	public bool drawMeshGizmos {
		get { return meshGizmos; }
		set { meshGizmos = value; }
	}
	
	/// <summary>
	/// DEPRECATED. Please use 'meshGizmoLength' property instead.
	/// </summary>
	[System.ObsoleteAttribute( "Please use 'meshGizmoLength' property instead.")]
	public float drawMeshGizmosLength {
		get { return meshGizmoLength; }
		set { meshGizmoLength = value; }
	}
}