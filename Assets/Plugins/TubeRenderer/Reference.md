#TubeRenderer Reference


###Properties

**Vector3[] points**  
Center points for the tube. Forward rotations will be calculated from the point at index 0 and upwards. The array is NOT copied; the tube will keep the reference to the array.

**float[] radiuses**  
Radius values for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. If 'radius' has been set then 'radiuses' will be ignored. The array is NOT copied; the tube will keep the reference to the array.

**float radius**  
Radius for the entire tube. If 'radiusses' has been set then 'radius' will be ignored. Default is 0.1.

**Color[] colors**  
Vertex colors for the tube. Each value corresponds to the point at the same array index. Array length must fit the number of points. The array is NOT copied; the tube will keep the reference to the array.

**int edgeCount**  
Edge resolution. Minimum is 3. Default is 12.

**bool calculateTangents**  
Calculation of tangents. Default is false (to boost performance).

**bool invertMesh**  
Mesh inversion (render the tube inside out). In most cases you should do 'Cull Front' in your shader instead. Default is false.

**NormalMode normalMode**  
How normals are rendered. Default is NormalMode.Smooth.

**CapMode caps**  
Closed or open end points. Default is CapMode.Both.

**bool postprocessContinously**  
Postprocess continously (if AddPostprocess has been called). When true, postprocesses will be called every update. When false, they will only be called when tube properties are changed. Default is true.

**Rect uvRect**  
UV mapping rect for wrapped tube body. Default is Rect(0,0,1,1).

**Rect uvRectCap**   
UV mapping rect for tube caps (if addCaps is true). Default is Rect(0,0,1,1).

**bool uvRectCapEndMirrored**  
Mirrored uv mapping for cap at end point (points[points.Length-1]). Default is false.

**float forwardAngleOffset**
Rotation offset around the tubes forward direction.

**Mesh mesh**  
Get the tube mesh. Useful for combining multiple tubes into a static mesh. Do not manipulate directly.

**bool drawMeshGizmos**  
Draw gizmos for mesh normals and tangents. Default is false.

**float drawMeshGizmosLength**  
Length of gizmos for mesh normals and tangents. Default is 0.1.

###Methods

**Quaternion GetRotationAtPoint( int index )**  
Gets the rotation of a point at index in the tube.

**void ForceUpdate()**  
Force update to generate the tube mesh immediately. 

**void MarkDynamic()**  
Shortcut to Mesh.MarkDynamic(). Call this if the tube will be updated often so that Unity can optimise memory use.

**void AddPostprocess( Postprocess postprocess )**  
Add a method to receive and manipulate mesh data before it is applied. Useful for creating distortion effects or complex variations.

**void RemovePostprocess( Postprocess postprocess )**  
Remove a postprocess method that have previously been assigned using the 'AddPostprocess' method.

###Enums

**NormalMode**  
Defines how normals are rendered. Options are Smooth, Hard and HardEdges.

**CapMode**  
Defines what caps should be rendered. Options are None, Begin, End and Both.

###Delegates

**void Postprocess( Vector3[] vertices, Vector3[] normals, Vector4[] tangents )**  
Method for passing mesh data.



