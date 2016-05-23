using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// Abstract base class for all Grid Framework grids.
/// </summary>
/// <para>
/// This is the standard class all grids are based on.
/// Aside from providing a common set of variables and a template for what methods to use, this class has no practical meaning for end users.
/// Use this as reference for what can be done without having to specify which type of grid you are using.
/// </para>

[System.Serializable]
public abstract class GFGrid : MonoBehaviour {
	#region nested classes and enums
	// the index refers to the missing axis (X=0, Y=1, Z=2)
	public enum GridPlane {YZ, XZ, XY};
	// instead of always aligning the centre we can use this to align other regions (WIP)
	private enum AlignReference {Center, RightUpBack, RightUpFront, RightDownBack, RightDownFront, LeftUpBack, LeftUpFront, LeftDownBack, LeftDownFront};
	#region optimization
	protected class TransformCache{
		protected Transform _gridTransform;
		// these parts can be shortened using Transform.hasChanged, but that is Unity 4+ only
		public Vector3 oldPosition;
		public Quaternion oldRotation;
		
		// true if values have changed, false if they re the same
		public bool needRecalculation{get{return !(oldPosition == _gridTransform.position && oldRotation == _gridTransform.rotation);}}
		
		public void Recache(){
			oldPosition = _gridTransform.position;
			oldRotation = _gridTransform.rotation;
		}
		
		public TransformCache(GFGrid grid){
			_gridTransform = grid._transform;
			Recache();
		}
	}
	#endregion
	#endregion
	
	#region class members
	#region size and range
	#region Protected
	[SerializeField]
	private bool _relativeSize = false;
	
	[SerializeField]
	protected Vector3 _size = new Vector3(5.0f, 5.0f, 5.0f);
	
	[SerializeField]
	protected Vector3 _renderFrom = Vector3.zero; //first one from, second one to 
	
	[SerializeField]
	protected Vector3 _renderTo = 3*Vector3.one; //first one from, second one to 
	#endregion
	#region Accessors
	/// <summary>Whether the drawing/rendering will scale with spacing</summary>
	/// <value><c>true</c> if relative size; otherwise, <c>false</c>.</value>
	public bool relativeSize {
		get{return _relativeSize;}
		set{
			if(value == _relativeSize)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			_relativeSize = value;
		}
	}
	/// <summary>The size of the visual representation of the grid</summary>
	/// <value>The size.</value>
	public virtual Vector3 size{
		get{return _size;}
		set{if(value == _size)// needed because the editor fires the setter even if this wasn't changed
			return;
			_gridChanged = true;
			_size = Vector3.Max(value, Vector3.zero);}
	}
	/// <summary>Custom lower limit for the rendering</summary>
	/// <value>The render from.</value>
	public virtual Vector3 renderFrom{
		get{return _renderFrom;}
		set{if(value == _renderFrom)// needed because the editor fires the setter even if this wasn't changed
			return;
			_gridChanged = true;
			_renderFrom = Vector3.Min(value, renderTo);}
	}
	/// <summary>Custom upper limit for the rendering</summary>
	/// <value>The render to.</value>
	public virtual Vector3 renderTo{
		get{return _renderTo;}
		set{if(value == _renderTo)// needed because the editor fires the setter even if this wasn't changed
			return;
			_gridChanged = true;
			_renderTo = Vector3.Max(value, _renderFrom);}
	}
	#endregion
	#endregion
	
	#region colours
	/// <summary>Colour of the axes when drawing and rendering</summary>
	public GFColorVector3 axisColors = new GFColorVector3();
	/// <summary>colour of vertices when drawing and rendering</summary>
	public Color vertexColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
	/// <summary>Whether to use the same colours for rendering as for drawing</summary>
	public bool useSeparateRenderColor = false;
	/// <summary>Separate colour of the axes when rendering (optional)</summary>
	public GFColorVector3 renderAxisColors = new GFColorVector3(Color.gray);
	#endregion
	
	#region Draw & Render Flags
	/// <summary>Whether to hide the grid completely</summary>
	public bool hideGrid = false;
	/// <summary>Whether to hide the grid in play mode</summary>
	public bool hideOnPlay = false;
	/// <summary>Whether to hide just individual axes</summary>
	public GFBoolVector3 hideAxis = new GFBoolVector3();
	/// <summary>Whether to draw a little sphere at the origin of the grid</summary>
	public bool drawOrigin = false;
	/// <summary>Whether to render the grid at runtime</summary>
	public bool renderGrid = true;

	[SerializeField]
	protected bool _useCustomRenderRange = false;
	/// <summary>Use you own values for the range of the rendering</summary>
	/// <value><c>true</c> if use custom render range; otherwise, <c>false</c>.</value>
	public bool useCustomRenderRange{get{return _useCustomRenderRange;}set{if(value == _useCustomRenderRange){return;} _useCustomRenderRange = value; _gridChanged = true;}}

	[SerializeField]
	protected int _renderLineWidth = 1;
	/// <summary>The width of the lines used when rendering the grid</summary>
	/// <value>The width of the render line.</value>
	public int renderLineWidth{
		get{return _renderLineWidth;}
		set{_renderLineWidth = Mathf.Max(value, 1);}
	}

	/// <summary>The material for rendering, if none is given it uses a default</summary>
	public Material renderMaterial = null;
	protected Material defaultRenderMaterial {get {return new Material("Shader \"Lines/Colored Blended\" {" +
		"SubShader { Pass { " +
		"	Blend SrcAlpha OneMinusSrcAlpha " +
		"	ZWrite Off Cull Off Fog { Mode Off } " +
		"	BindChannels {" +
		"	Bind \"vertex\", vertex Bind \"color\", color }" +
		"} } }" );}}
	#endregion
	
	#region cache members
	/// <summary>Three-dimensional matrix for storing a list of grid vertices</summary>
	public Vector3[,,] ownVertexMatrix;
	// we store the draw points here for re-use
	protected Vector3[][][] _drawPoints;
	#region optimization
	// caching the transform for performance
	protected Transform cachedTransform; //this is the real cache
	// this is used for access, if there is nothing cached it performs the cache first, then return the component
	protected Transform _transform{
		get{
			if(!cachedTransform)
				cachedTransform = transform;
			return cachedTransform;
		}
	}
	
	// this will be set to true whenever one of the accessors calls its set
	private TransformCache _transformCache;
	protected TransformCache transformCache {
		get{
			if(_transformCache == null)
				_transformCache = new TransformCache(this);
			return _transformCache;
		}
	}
	protected bool _gridChanged = false;
	protected bool hasChanged {
		get{
			if(_gridChanged || transformCache.needRecalculation){
				_gridChanged = false;
				transformCache.Recache();
				return true;
			} else{
				return _gridChanged;
			}
		}
	}
	#endregion
	#endregion
	
	#region helper values (read only)
	// the normal X-, Y- and Z- vectors in world-space
	protected Vector3[] units {get {return new Vector3[3]{Vector3.right, Vector3.up, Vector3.forward};}}
	#endregion
	#endregion
	
	#region Grid <-> World coordinate transformation
	/// <summary>Converts world coordinates to grid coordinates</summary>
	/// <returns>World coordinates of the grid point</returns>
	/// <param name="worldPoint">Point in world space.</param>
	public abstract Vector3 WorldToGrid(Vector3 worldPoint);
	/// <summary>Converts grid coordinates to world coordinates</summary>
	/// <returns>Grid coordinates of the world point.</returns>
	/// <param name="gridPoint">Point in grid space.</param>
	public abstract Vector3 GridToWorld(Vector3 gridPoint);
	#endregion
	
	#region nearest in world space
	/// <summary>Returns the world position of the nearest vertex</summary>
	/// <returns>World position of the nearest vertex.</returns>
	/// <param name="worldPoint">Point in world space</param>
	/// <param name="doDebug">If set to <c>true</c> draw a sphere at the destination.</param>
	public abstract Vector3 NearestVertexW(Vector3 worldPoint, bool doDebug = false);
	/// <summary>Returns the world position of the nearest face</summary>
	/// <returns>World position of the nearest vertex.</returns>
	/// <param name="worldPoint">Point in world space</param>
	/// </param name ="plane">Plane on which the face lies</param>
	/// <param name="doDebug">If set to <c>true</c> draw a sphere at the destination.</param>
	public abstract Vector3 NearestFaceW(Vector3 worldPoint, GridPlane plane, bool doDebug = false);
	/// <summary>Returns the world position of the nearest box</summary>
	/// <returns>World position of the nearest vertex.</returns>
	/// <param name="worldPoint">Point in world space</param>
	/// <param name="doDebug">If set to <c>true</c> draw a sphere at the destination.</param>
	public abstract Vector3 NearestBoxW(Vector3 worldPoint, bool doDebug = false);
	#endregion
	
	#region nearest in grid space
	/// <summary>Returns the grid position of the nearest vertex</summary>
	/// <returns>Grid position of the nearest vertex.</returns>
	/// <param name="worldPoint">Point in world space</param>
	public abstract Vector3 NearestVertexG(Vector3 worldPoint);
	/// <summary>Returns the grid position of the nearest vertex</summary>
	/// <returns>Grid position of the nearest face.</returns>
	/// <param name="worldPoint">Point in world space</param>
	/// /// </param name ="plane">Plane on which the face lies</param>
	public abstract Vector3 NearestFaceG(Vector3 worldPoint, GridPlane plane);
	/// <summary>Returns the grid position of the nearest box</summary>
	/// <returns>Grid position of the nearest box.</returns>
	/// <param name="worldPoint">Point in world space</param>
	public abstract Vector3 NearestBoxG(Vector3 worldPoint);
	#endregion
	
	#region vertex matrix
	/// <summary>returns a Vector3[,,] containing the world position of grid vertices within a certain range of the origin</summary>
	/// <returns>Three-dimensional array of vertices as Vector3</returns>
	/// <param name="matrixSize">Size of the matrix</param>
	public Vector3[,,] BuildVertexMatrix(Vector3 matrixSize){
		return BuildVertexMatrix(matrixSize.x, matrixSize.y, matrixSize.z);
	}
	/// <summary>returns a Vector3[,,] containing the world position of grid vertices within a certain range of the origin</summary>
	/// <returns>Three-dimensional array of vertices as [Vector3]</returns>
	/// <param name="height">Height of the matrix</param>
	/// <param name="width">Width of the matrix</param>
	/// <param name="depth">Depth of the matrix</param>
	public abstract Vector3[,,] BuildVertexMatrix(float height, float width, float depth);
	public abstract Vector3 ReadVertexMatrix(int i, int j, int k, Vector3[,,] vertexMatrix, bool warning = false);
	#endregion
	
	#region Align Methods
	#region overload
	public void AlignTransform(Transform theTransform){
		AlignTransform(theTransform, true, new GFBoolVector3(false));
	}
	
	public void AlignTransform(Transform theTransform, GFBoolVector3 lockAxis){
		AlignTransform(theTransform, true, lockAxis);
	}
	
	public void AlignTransform(Transform theTransform, bool rotate){
		AlignTransform(theTransform, rotate, new GFBoolVector3(false));
	}
	
	public Vector3 AlignVector3(Vector3 pos){
		return AlignVector3(pos, Vector3.one, new GFBoolVector3(false));
	}
	
	public Vector3 AlignVector3(Vector3 pos, GFBoolVector3 lockAxis){
		return AlignVector3(pos, Vector3.one, lockAxis);
	}
	
	public Vector3 AlignVector3(Vector3 pos, Vector3 scale){
		return AlignVector3(pos, scale, new GFBoolVector3(false));
	}
	#endregion
	/// <summary>Fits a Transform inside the grid, but does not scale it</summary>
	/// <param name="theTransform">The Transform to align</param>
	/// <param name="rotate">Whether to rotate to the grid</param>
	/// <param name="ignoreAxis">Which axes should be ignored</param>
	public void AlignTransform(Transform theTransform, bool rotate, GFBoolVector3 ignoreAxis){
		Quaternion oldRotation = theTransform.rotation;
		theTransform.rotation = transform.rotation;

		theTransform.position = AlignVector3(theTransform.position, theTransform.lossyScale, ignoreAxis);
		if(!rotate)
			theTransform.rotation = oldRotation;
	}
	/// <summary>Similar to AlignTransform, except only for Vectors</summary>
	/// <returns>The aligned vector</returns>
	/// <param name="pos">Position in world space</param>
	/// <param name="scale">Scale used to place the vector</param>
	/// <param name="ignoreAxis">Which axes should be ignored</param>
	public abstract Vector3 AlignVector3(Vector3 pos, Vector3 scale, GFBoolVector3 ignoreAxis);
	#endregion
	
	#region Scale Methods
	#region overload
	public void ScaleTransform(Transform theTransform){
		ScaleTransform(theTransform, new GFBoolVector3(false));
	}
	public Vector3 ScaleVector3(Vector3 scl){
		return ScaleVector3(scl, new GFBoolVector3(false));
	}
	#endregion
	/// <summary>Scales a Transform to fit the grid but does not move it</summary>
	/// <param name="theTransform">The Transform to scale</param>
	/// <param name="lockAxis">The axes to ignore</param>
	public void ScaleTransform(Transform theTransform, GFBoolVector3 ignoreAxis){
		theTransform.localScale = ScaleVector3(theTransform.localScale, ignoreAxis);
	}
	/// <summary>Similar to SclaeTransform, except only for Vectors</summary>
	/// <param name="scl">The vector to scale</param>
	/// <param name="lockAxis">The axes to ignore</param>
	public abstract Vector3 ScaleVector3(Vector3 scl, GFBoolVector3 ignoreAxis);
	#endregion
	
	#region Render Methods
	#region overload
	public virtual void RenderGrid(int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGrid(-size, size, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	public void RenderGrid(Vector3 from, Vector3 to, int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGrid(from, to, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	#endregion
	/// <summary>Renders the grid at runtime</summary>
	/// <param name="from">Lower limit</param>
	/// <param name="to">Upper limit</param>
	/// <param name="colors">Colors for rendering</param>
	/// <param name="width">Width of the line</param>
	/// <param name="cam">Camera for rendering</param>
	/// <param name="camTransform">Transform of the camera</param>
	public virtual void RenderGrid(Vector3 from, Vector3 to, GFColorVector3 colors, int width = 0, Camera cam = null, Transform camTransform = null){
		if(!renderGrid)
			return;

		if(!renderMaterial)
			renderMaterial = defaultRenderMaterial;
		
		CalculateDrawPoints(from, to);
		
		RenderGridLines(colors, width, cam, camTransform);
	}
	
	protected void RenderGridLines(GFColorVector3 colors, int width = 0, Camera cam = null, Transform camTransform = null){
		renderMaterial.SetPass(0);
		
		if(width <= 1 || !cam || !camTransform){// use simple lines for width 1 or if no camera was passed
			GL.Begin(GL.LINES);
			for(int i = 0; i < 3; i++){
				if(hideAxis[i])
					continue;
				GL.Color(colors[i]);
				foreach(Vector3[] line in _drawPoints[i]){
					if(line == null) continue;
						GL.Vertex(line[0]); GL.Vertex(line[1]);
				}
			}
			GL.End();
		} else{// quads for "lines" with width
			GL.Begin(GL.QUADS);
			float mult = Mathf.Max(0, 0.5f * width); //the multiplier, half the desired width
			
			for(int i = 0; i < 3; i++){
				GL.Color(colors[i]);
				if(hideAxis[i])
					continue;
				
				//sample a direction vector, one per direction is enough (using the first line of each line set (<- !!! ONLY TRUE FOR RECT GRIDS !!!)
				Vector3 dir = new Vector3();
				if(_drawPoints[i].Length > 0) //can't get a line if the set is empty
					dir = Vector3.Cross(_drawPoints[i][0][0] - _drawPoints[i][0][1], camTransform.forward).normalized;
				//multiply dir with the world length of one pixel in distance
				if(cam.orthographic){
					dir *= (cam.orthographicSize * 2) / cam.pixelHeight;
				} else{// (the 50 below is just there to smooth things out)
					dir *= (cam.ScreenToWorldPoint(new Vector3(0, 0, 50)) - cam.ScreenToWorldPoint(new Vector3(20, 0, 50))).magnitude/20;
				}
				
				foreach(Vector3[] line in _drawPoints[i]){
					if(line == null) continue;
					// if the grid is not rectangular we need to change dir every time
					if (this.GetType() != typeof(GFRectGrid)) {
						dir = Vector3.Cross(line[0] - line[1], camTransform.forward).normalized;
						if(cam.orthographic){
							dir *= (cam.orthographicSize * 2) / cam.pixelHeight;
						} else{// (the 50 below is just there to smooth things out)
							dir *= (cam.ScreenToWorldPoint(new Vector3(0, 0, 50)) - cam.ScreenToWorldPoint(new Vector3(20, 0, 50))).magnitude/20;
						}
					}
					GL.Vertex(line[0]-mult*dir); GL.Vertex(line[0]+mult*dir); GL.Vertex(line[1]+mult*dir); GL.Vertex(line[1]-mult*dir);
				}
			}
			GL.End();
		}
	}
	#endregion
	
	#region Draw Methods
	public virtual void DrawGrid(){
		DrawGrid(-size, size);
	}
	public virtual void DrawGrid(Vector3 from, Vector3 to){
		//don't draw if not supposed to
		if(hideGrid)
			return;
		
		CalculateDrawPoints(from, to);
		
		for(int i = 0; i < 3; i++){
			if(hideAxis[i])
				continue;
			Gizmos.color = axisColors[i];
			foreach(Vector3[] line in _drawPoints[i]){
				if (line == null) continue;
				Gizmos.DrawLine(line[0], line[1]);
			}
		}
		
		//draw a sphere at the centre
		if(drawOrigin){
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(_transform.position, 0.3f);
		}
	}
	
	public void DrawVertices(Vector3[,,] vertexMatrix, bool drawOnPlay = false){
		//do not draw vertices when playing, this is a performance hog
		if(Application.isPlaying && !drawOnPlay)
		return;
		
		Gizmos.color = vertexColor;
		
		for(int i=0;  i<= vertexMatrix.GetUpperBound(0); i++){
			for(int j=0;  j<= vertexMatrix.GetUpperBound(1); j++){
				for(int k=0;  k<= vertexMatrix.GetUpperBound(2); k++){
					DrawSphere(vertexMatrix[i,j,k]);
				}
			}
		}
	}
	
	protected void DrawSphere (Vector3 pos, float rad = 0.3f){
		Gizmos.DrawSphere(pos, rad);
	}
	#endregion
	
	#region Calculate Draw Points
	#region overload
	protected virtual Vector3[][][] CalculateDrawPoints(){
		return CalculateDrawPoints(-size, size);
	}
	#endregion
	protected abstract Vector3[][][] CalculateDrawPoints(Vector3 from, Vector3 to);
	#endregion
	
	#region Vectrosity Methods
	#region overload
	public Vector3[] GetVectrosityPoints(){
		return GetVectrosityPoints(-size, size);
	}
	
	public Vector3[][] GetVectrosityPointsSeparate(){
		return GetVectrosityPointsSeparate(-size, size);
	}
	#endregion
	public Vector3[] GetVectrosityPoints(Vector3 from, Vector3 to){
		Vector3[][] seperatePoints = GetVectrosityPointsSeparate(from, to); 
		Vector3[] returnedPoints = new Vector3[seperatePoints[0].Length + seperatePoints[1].Length + seperatePoints[2].Length];
		seperatePoints[0].CopyTo(returnedPoints, 0);
		seperatePoints[1].CopyTo(returnedPoints, seperatePoints[0].Length);
		seperatePoints[2].CopyTo(returnedPoints, seperatePoints[0].Length + seperatePoints[1].Length);
		return returnedPoints;
	}
	
	public Vector3[][] GetVectrosityPointsSeparate(Vector3 from, Vector3 to){
		CalculateDrawPoints(from, to);
		int[] lengths = new int[3];
		for(int i = 0; i < 3; i++){
			lengths[i] = _drawPoints[i].Count(line => line != null)*2; // <- ! watch out, using LINQ !
		}
		
		Vector3[][] returnedPoints = new Vector3[3][];
		for(int i = 0; i < 3; i++){
			returnedPoints[i] = new Vector3[lengths[i]];
//			Debug.Log(lengths[i]);
		}
		int iterator = 0;
		for(int i = 0; i < 3; i++){
			iterator = 0;
			foreach(Vector3[] line in _drawPoints[i]){
				if(line == null)
					continue;
				returnedPoints[i][iterator] = line[0];
				iterator++;
				returnedPoints[i][iterator] = line[1];
				iterator++;
			}
		}
		
		return returnedPoints;
	}
	#endregion
	
	#region Runtime Methods
	void Awake(){
		if(hideOnPlay)
			hideGrid = true;
		
		if(renderMaterial == null)
			renderMaterial = defaultRenderMaterial;
		
		GFGridRenderManager.AddGrid(this);
	}
	
	void OnDestroy(){
	    GFGridRenderManager.RemoveGrid(GetComponent<GFGrid>());
	}
	#endregion
	
	#region helper methods
	// swaps two variables, useful for swapping quasi-X and quasi-Y to keep the same formula for pointy sides and flat sides
	protected static void Swap<T>(ref T a, ref T b, bool condition = true){
		if(condition){
			T temp = b; b = a; a = temp;
		}
	}
	
	// returns the a number rounded to the nearest multiple of anothr number (rounds up)
	protected static float RoundCeil (float number, float multiple){
		return Mathf.Ceil(number / multiple) * multiple;// could use Ceil or Floor to always round up or down
	}
	// returns the a number rounded to the nearest multiple of anothr number (rounds up or down)
	protected static float RoundMultiple (float number, float multiple){
		return Mathf.Round(number / multiple) * multiple;// could use Ceil or Floor to always round up or down
	}
	// returns the a number rounded to the nearest multiple of anothr number (rounds down)
	protected static float RoundFloor (float number, float multiple){
		return Mathf.Floor(number / multiple) * multiple;// could use Ceil or Floor to always round up or down
	}
	#endregion
	
	#region deprecated methods (used for backwards compatibility with older releases)
	// world space
	[System.Obsolete("Deprecated, please use NearestVertexW instead",false)]
	public Vector3 FindNearestVertex(Vector3 fromPoint, bool doDebug = false) {return NearestVertexW(fromPoint, doDebug);}
	[System.Obsolete("Deprecated, please use NearestFaceW instead")]
	public Vector3 FindNearestFace(Vector3 fromPoint, GridPlane thePlane, bool doDebug = false) {return NearestFaceW(fromPoint, thePlane, doDebug);}
	[System.Obsolete("Deprecated, please use NearestBoxW instead",false)]
	public Vector3 FindNearestBox(Vector3 fromPoint, bool doDebug = false) {return NearestBoxW(fromPoint, doDebug);}
	// grid space
	[System.Obsolete("Deprecated, please use NearestVertexG instead",false)]
	public Vector3 GetVertexCoordinates(Vector3 world) {return NearestVertexG(world);}
	[System.Obsolete("Deprecated, please use NearestFaceG instead",false)]
	public Vector3 GetFaceCoordinates(Vector3 world, GridPlane thePlane) {return NearestFaceG(world, thePlane);}
	[System.Obsolete("Deprecated, please use NearestBoxG instead",false)]
	public Vector3 GetBoxCoordinates(Vector3 world) {return NearestBoxG(world);}
	#endregion
}

/// <summary>
/// Radians or degrees
/// </summary>
/// <para>
/// A simple enum for specifying whether an angle is given in radians for degrees.
/// This enum is so far only used in methods of GFPolarGrid, but I decided to make it global in case other grids in the future will use it was well.
/// </para>
public enum GFAngleMode {radians, degrees};