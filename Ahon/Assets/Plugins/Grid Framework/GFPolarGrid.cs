using UnityEngine;
using System.Collections;

// format of polar (cylindrical) coordinates (radius, angle, height), where:
//	- radius as distance from centre of the grid
//	- angle in radians in [0, 2π)
//	- height as distance from centre of the grid
//	- (plus index transformation due to gridPlane)

// format of grid coordinates (radius, sector, height), where:
//	- radius as multiple of "radius"
//	- sector as multiple of "angle"
//	- height as multiple of "depth"
//	- (plus index transformation due to gridPlane)
public class GFPolarGrid : GFLayeredGrid {

	#region class members
	#region overriding inherited accessors
	public override Vector3 size{
		get{return _size;}
		set{if(value == _size)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			//_size = Vector3.Max(Vector3.Min(value, value[idx[0]] * units[idx[0]] + 2 * Mathf.PI * units[idx[1]] + value[idx[2]] * units[idx[2]]), Vector3.zero);
			_size[idx[0]] = Mathf.Max(value[idx[0]], 0);
			_size[idx[1]] = Mathf.Max(Mathf.Min(value[idx[1]], 2 * Mathf.PI), 0);
			_size[idx[2]] = Mathf.Max(value[idx[2]], 0);
		}
	}
	
	public override Vector3 renderFrom{
		get{return _renderFrom;}
		set{if(value == _renderFrom)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			//_renderFrom = Vector3.Min(value, renderTo);
			_renderFrom[idx[0]] = Mathf.Max(Mathf.Min(value[idx[0]], renderTo[idx[0]]), 0); // prevent negative quasi-X and keep lower than renderTo
			_renderFrom[idx[1]] = Float2Rad(value[idx[1]]);
			_renderFrom[idx[2]] = Mathf.Min(renderTo[idx[2]], value[idx[2]]); // keep lower than renderTo
		}
	}
	
	public override Vector3 renderTo{
		get{return _renderTo;}
		set{if(value == _renderTo)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			//_renderTo = Vector3.Max(value, _renderFrom);
			_renderTo[idx[0]] = Mathf.Max(renderFrom[idx[0]], value[idx[0]]); // prevent negative quasi-X
			_renderTo[idx[1]] = Float2Rad(value[idx[1]]);
			_renderTo[idx[2]] = Mathf.Max(renderFrom[idx[2]], value[idx[2]]); // keep lower than renderTo
		}
	}
	#endregion
	
	#region members
	[SerializeField]
	private float _radius = 1;
	public float radius {
		get{return _radius;}
		set{if(value == _radius)// needed because the editor fires the setter even if this wasn't changed
				return;
			_radius = Mathf.Max(0.01f, value);
			_gridChanged = true;
		}
	}
	
	[SerializeField]
	private int _sectors = 8;
	public int sectors{
		get{return _sectors;}
		set{if(value == _sectors)// needed because the editor fires the setter even if this wasn't changed
				return;
			_sectors = Mathf.Max(1, value);
			_gridChanged = true;
		}
	}
			
	// the amount of segments within a segment, more looks smoother 
	[SerializeField]
	private int _smoothness = 5;
	public int smoothness{
		get{return _smoothness;}
		set{if(value == _smoothness)// needed because the editor fires the setter even if this wasn't changed
				return;
			_smoothness = Mathf.Max(1, value);
			_gridChanged = true;
		}
	}
	#endregion
	
	#region helper values (read only)
	public float angle { get { return (2 * Mathf.PI) / sectors;} }
	public float angleDeg { get { return 360.0f / sectors;} }
	#endregion
	#endregion
		
	#region Grid <-> World coordinate transformation
	public override Vector3 WorldToGrid(Vector3 worldPoint){
		return PolarToGrid(WorldToPolar(worldPoint));
	}
	
	public override Vector3 GridToWorld(Vector3 gridPoint){
		return PolarToWorld(GridToPolar(gridPoint));
	}
	#endregion
	
	#region Polar <-> World coordinate transformation
	public Vector3 WorldToPolar ( Vector3 worldPoint) {
		// first transform the point into local coordinates
		Vector3 localPoint = _transform.GFInverseTransformPointFixed(worldPoint);
		// now turn the point from Cartesian coordinates into polar coordinates
		return Mathf.Sqrt(Mathf.Pow(localPoint[idx[0]], 2) + Mathf.Pow(localPoint[idx[1]], 2)) * units[idx[0]]
			+ Atan3(localPoint[idx[1]], localPoint[idx[0]]) * units[idx[1]]
			+ localPoint[idx[2]] * units[idx[2]];
	}
	
	public Vector3 PolarToWorld ( Vector3 polarPoint) {
		return _transform.GFTransformPointFixed(
			polarPoint[idx[0]] * Mathf.Cos(Float2Rad(polarPoint[idx[1]])) * units[idx[0]]
			+ polarPoint[idx[0]] * Mathf.Sin(Float2Rad(polarPoint[idx[1]])) * units[idx[1]]
			+ polarPoint[idx[2]] * units[idx[2]]);
	}
	#endregion
	
	#region Grid <-> Polar coordinate transformation
	public Vector3 GridToPolar ( Vector3 gridPoint) {
		gridPoint [idx[1]] = Float2Sector (gridPoint[idx[1]]);
		Vector3 polar = Vector3.Scale(gridPoint, radius * units[idx[0]] + Float2Rad(angle) * units[idx[1]] + depth * units[idx[2]]);
		polar[idx[1]] = Float2Rad(polar[idx[1]]);
		return polar;
	}
	
	public Vector3 PolarToGrid ( Vector3 polarPoint) {
		return polarPoint.GFReverseScale(radius * units[idx[0]] + Float2Rad(angle) * units[idx[1]] + depth * units[idx[2]]);
	}
	#endregion

	#region Conversions
	#region Public
	// converts an angle (radians or degree) to the corresponding sector coordinate
	public float Angle2Sector (float angle, GFAngleMode mode = GFAngleMode.radians) {
		angle = Float2Rad (angle * (mode == GFAngleMode.degrees ? Mathf.Deg2Rad : 1.0f));
		return angle / this.angle * (mode == GFAngleMode.degrees ? Mathf.Rad2Deg : 1.0f);
	}
	// converts a sector to the corresponding angle (radians or degree) coordinate
	public float Sector2Angle (float sector, GFAngleMode mode = GFAngleMode.radians) {
		sector = Float2Sector (sector);
		return sector * angle * (mode == GFAngleMode.degrees ? Mathf.Rad2Deg : 1.0f);
	}

	// converts an angle around the origina to a rotation
	public Quaternion Angle2Rotation (float angle, GFAngleMode mode = GFAngleMode.radians) {
		return Quaternion.AngleAxis(angle * (mode == GFAngleMode.radians ? Mathf.Rad2Deg : 1.0f), locUnits[idx[2]] * (gridPlane == GridPlane.XY ? 1.0f : -1.0f));
	}
	// converts a sector around the origina to a rotation
	public Quaternion Sector2Rotation (float sector) {
		return Angle2Rotation(Sector2Angle(sector,GFAngleMode.radians), GFAngleMode.radians);
	}
	// rotates around the grid based on world position
	public Quaternion World2Rotation (Vector3 world) {
		return Angle2Rotation(World2Angle(world));
	}

	// extract the angle around the grid from a point in world space
	public float World2Angle (Vector3 world, GFAngleMode mode = GFAngleMode.radians) {
		return WorldToPolar(world)[idx[1]] * (mode == GFAngleMode.radians ? 1.0f : Mathf.Rad2Deg);
	}
	// extract the angle around the grid from a point in world space
	public float World2Sector (Vector3 world, GFAngleMode mode = GFAngleMode.radians) {
		return WorldToGrid(world)[idx[1]];
	}
	// extract the distance for the origin
	public float World2Radius (Vector3 world) {
		return WorldToPolar (world) [idx[0]];
	}
	#endregion
	#region Private
	// interprets a float as radians; loops if value exceeds 2π and runs in reverse for negative values
	private static float Float2Rad (float number){
		return number >= 0 ? number % (2* Mathf.PI) : 2 * Mathf.PI + (number % Mathf.PI);
	}
	// interprets a float as degree; loops if value exceeds 360 and runs in reverse for negative values
	private static float Float2Deg (float number){
		return number >= 0 ? number % 360 : 360 + (number % 360);
	}
	// interprets a float as sector; loops if value exceeds [sectors] and runs in reverse for negative values
	private float Float2Sector (float number) {
		return number >= 0 ? number % sectors : sectors + (number % sectors);
	}
	#endregion
	#endregion
	
	#region nearest in world space
	public override Vector3 NearestVertexW(Vector3 fromPoint, bool doDebug = false){
		Vector3 dest = PolarToWorld(NearestVertexP(fromPoint));
		if(doDebug)
			DrawSphere(dest);
		return dest;
	}
	
	public override Vector3 NearestFaceW (Vector3 fromPoint, GridPlane thePlane, bool doDebug = false){
		Vector3 dest = PolarToWorld(NearestFaceP(fromPoint, thePlane));
		if(doDebug)
			DrawSphere(dest);
		return dest;
	}
	public Vector3 NearestFaceW (Vector3 fromPoint, bool doDebug = false){
		return NearestFaceW(fromPoint, gridPlane, doDebug);
	}

	public override Vector3 NearestBoxW (Vector3 fromPoint, bool doDebug = false){
		Vector3 dest = PolarToWorld(NearestBoxP(fromPoint));
		if(doDebug)
			DrawSphere(dest);
		return dest;
	}
	#endregion
	
	#region nearest in grid space
	public override Vector3 NearestVertexG(Vector3 world){
		Vector3 local = WorldToGrid(world);
		return RoundGridPoint(local);
	}

	public override Vector3 NearestFaceG(Vector3 world, GridPlane thePlane){
		return PolarToGrid (NearestFaceP(world, thePlane));
	}
	public Vector3 NearestFaceG (Vector3 world) {
		return NearestFaceG (world, gridPlane);
	}

	public override Vector3 NearestBoxG(Vector3 world){

		return PolarToGrid (NearestBoxP(world));
	}
	
	private Vector3 RoundGridPoint (Vector3 point) {
		return Mathf.Round(point[idx[0]]) * units[idx[0]] + Mathf.Round(point[idx[1]]) * units[idx[1]] + Mathf.Round(point[idx[2]]) * units[idx[2]];
	}
	#endregion
	
	#region nearest in polar space
	// TO DO: edge case when the point it right in the centre of the grid
	public Vector3 NearestVertexP(Vector3 world){
		Vector3 polar = WorldToPolar(world);
		return RoundPolarPoint(polar);
	}

	public Vector3 NearestFaceP(Vector3 world, GridPlane thePlane){
		Vector3 polar = WorldToPolar(world);
		polar -= 0.5f * radius * units[idx[0]] + 0.5f * angle * units[idx[1]]; // virtually shift the point half an angle and half a radius down, this will simulate the shifted coordinates
		polar[idx[1]] = Mathf.Max(0, polar[idx[1]]); // prevent the angle from becoming negative
		polar = RoundPolarPoint (polar); // round the point
		polar += 0.5f * radius * units[idx[0]] + 0.5f * angle * units[idx[1]];
		//Debug.Log (polar);
		return polar;
	}

	public Vector3 NearestBoxP(Vector3 world){
		Vector3 polar = WorldToPolar(world);
		// virtually shift the point half an angle, half a radius and half the depth down, this will simulate the shifted coordinates
		polar -= 0.5f * radius * units[idx[0]] + 0.5f * angle * units[idx[1]] + 0.5f * depth * units[idx[2]];
		polar[idx[1]] = Mathf.Max(0, polar[idx[1]]);  // prevent the angle from becoming negative
		polar = RoundPolarPoint (polar);
		polar += 0.5f * radius * units[idx[0]] + 0.5f * angle * units[idx[1]] + 0.5f * depth * units[idx[2]];
		return polar;
	}
	
	private Vector3 RoundPolarPoint (Vector3 point) {
		return RoundMultiple(point[idx[0]], radius) * units[idx[0]] + RoundMultiple(point[idx[1]], angle) * units[idx[1]] + RoundMultiple(point[idx[2]], depth) * units[idx[2]];
	}
	#endregion
	
	#region vertex matrix
	public override Vector3[,,] BuildVertexMatrix(float height, float width, float depth){
		//prevent negative values
		width = Mathf.Abs(width);// the amount of multiples of radius
		height = Mathf.Abs(height);// the amount of sectors
		depth = Mathf.Abs(depth);// the amount of layers (times 2)
		
		Vector3[,,] vertexMatrix = new Vector3[Mathf.RoundToInt(width),Mathf.Min(Mathf.RoundToInt(height), sectors),2 * Mathf.RoundToInt(depth) + 1];
		
		// fill the matrix
		for (int k = 0; k < vertexMatrix.GetLength(2); k++) {
			for (int i = 1; i <= vertexMatrix.GetLength(0); i++) {
				for (int j = 0; j < vertexMatrix.GetLength(1); j++) {
					vertexMatrix[i-1,j,k] = GridToWorld(new Vector3(i, j, k - Mathf.RoundToInt(width) ));
				}
			}
		}
		return vertexMatrix;
	}
	public override Vector3 ReadVertexMatrix(int i, int j, int k, Vector3[,,] vertexMatrix, bool warning = false){
		return Vector3.zero;
	}
	#endregion

	#region Align Methods
	#region overload
	public void AlignRotateTransform (Transform theTransform) {
		AlignRotateTransform(theTransform, true,  new GFBoolVector3(false));
	}
	#endregion
	public void AlignRotateTransform (Transform theTransform, bool rotate, GFBoolVector3 lockAxis) {
		AlignTransform(theTransform, rotate, lockAxis);
		theTransform.rotation = World2Rotation(theTransform.position);
	}
	public override Vector3 AlignVector3 (Vector3 pos, Vector3 scale, GFBoolVector3 lockAxis) {
		float fracAngle = World2Angle(pos) / angle - Mathf.Floor (World2Angle(pos) / angle);
		float fracRad = World2Radius(pos) / radius - Mathf.Floor (World2Radius(pos) / radius);

		Vector3 vertex = NearestVertexP(pos);
		Vector3 box = NearestBoxP(pos);
		Vector3 final = Vector3.zero;

		//final += (scale [idx[0]] % 2.0f >= 0.5f || scale [idx[0]] < 1.0f ? box [idx[0]] : vertex [idx[0]]) * units[idx[0]]; % <-- another idea based on scale
		final += (0.25f < fracRad && fracRad < 0.75f ? box [idx[0]] : vertex [idx[0]]) * units[idx[0]];
		final += (0.25f < fracAngle && fracAngle < 0.75f ? box [idx[1]] : vertex [idx[1]]) * units [idx[1]];
		final += (scale [idx[2]] % 2.0f >= 0.5f || scale [idx[0]] < 1.0f ? box [idx[2]] : vertex [idx[2]]) * units[idx[2]];

		for (int i = 0; i <= 2; i++) {final[i] = lockAxis[i] ? pos[i] : final[i];}
		return PolarToWorld(final);
	}
	#endregion
	
	#region Scale Methods
	public override Vector3 ScaleVector3(Vector3 scl, GFBoolVector3 lockAxis){
		Vector3 result = Vector3.Max(RoundMultiple(scl[idx[0]], radius) * locUnits[idx[0]] + scl[idx[1]] * locUnits[idx[1]] + RoundMultiple(scl[idx[2]], depth) * locUnits[idx[2]],
			radius * locUnits[idx[0]] + scl[idx[1]] * locUnits[idx[1]] + depth * locUnits[idx[2]]);
		for (int i = 0; i <= 2; i++) {result[i] = lockAxis[i] ? scl[i] : result[i];}
		return result;
	}
	#endregion
		
	#region Gizmos
	public void OnDrawGizmos(){
		if(useCustomRenderRange){
			DrawGrid(renderFrom, renderTo);
		} else{
			DrawGrid();
		}
	}
	#endregion
	
	#region Draw Methods
	public override void DrawGrid () {
		DrawGrid (Vector3.zero - size[idx[2]] * units[idx[2]], size);
	}
	#endregion
	
	#region Render Methods
	public override void RenderGrid(int width = 0, Camera cam = null, Transform camTransform = null){
		RenderGrid(Vector3.zero - size[idx[2]] * units[idx[2]], size, useSeparateRenderColor ? renderAxisColors : axisColors, width, cam, camTransform);
	}
	#endregion
	
	#region Calculate Draw Points
	#region overload
	protected override Vector3[][][] CalculateDrawPoints(){
		Debug.Log("ping");
		return CalculateDrawPoints(Vector3.zero - size[idx[2]] * units[idx[2]], size);
	}
	#endregion
	protected override Vector3[][][] CalculateDrawPoints(Vector3 from, Vector3 to){
		// reuse the points if the grid hasn't changed, we already have some points and we use the same range
		if(!hasChanged && _drawPoints != null && from == renderFrom && to == renderTo){
			return _drawPoints;
		}
		
		if (relativeSize) {
			from[idx[0]] *= radius; to[idx[0]] *= radius;
			from[idx[2]] *= depth; to[idx[2]] *= depth;
		}
		
		// fit the float values of the second component into radians
		from[idx[1]] = Float2Rad(from[idx[1]]);
		to[idx[1]] = Float2Rad(to[idx[1]]);
		
		// our old points are of no use, so let's create a new set
		_drawPoints = new Vector3[3][][];
				
		// fist we need to figure out how many of each line we require, start with the amount of layers
		//float lowerZ = relativeSize ? depth * from[idx[2]] : from[idx[2]];
		//float upperZ = relativeSize ? depth * to[idx[2]] : to[idx[2]];
		float lowerZ = from[idx[2]];
		float upperZ = to[idx[2]];
		int layers = Mathf.FloorToInt(upperZ / depth) - Mathf.CeilToInt(lowerZ / depth) + 1;

		// the amount of circles per layer
		float startR = RoundCeil(from[idx[0]], radius);
		float endR = RoundFloor(to[idx[0]], radius);
		int circles = Mathf.RoundToInt((endR - startR) / radius) + 1;
		
		// the amount of sectors
		float startA = RoundMultiple(from[idx[1]], angle);
		float endA = RoundMultiple(to[idx[1]], angle);
		if(from[idx[1]] >= to[idx[1]]) // if the to angle did a full loop
			endA += 2*Mathf.PI; // add a whole cycle to it
		int sctrs = Mathf.RoundToInt((endA - startA) / angle);
		
		// from where to start (the centre of the lowest layer) (will be shifted after each layer)
		Vector3 startPos = _transform.position + depth * Mathf.CeilToInt(lowerZ / depth) * locUnits[idx[2]];
		
		_drawPoints[0] = new Vector3[layers * circles * sctrs * smoothness][]; // the circles
		_drawPoints[1] = new Vector3[layers * sectors][]; // the radial lines
		_drawPoints[2] = new Vector3[1][] {new Vector3[2] {_transform.position + lowerZ * locUnits[idx[2]], _transform.position + upperZ * locUnits[idx[2]]}}; // the Z-line
		
		// various counters to keep track what index to use for which line
		int circleSegmentCounter = 0; //line segment of a circle (does not reset)
		int radialCounter = 0; // a radial line (does not reset)
		
		// NOTE:  * (gridPlane == GridPlane.XY ? 1 : -1) is used to make sure the drawing is always counter-clockwise

		for (int i = 0; i < layers; i++){ // loop through the layers, each layer is one iteration
			for (int j = 0; j < circles; j++) { // first draw the circles, one loop for each of the circles of the current layer
				for (int k = 0; k < sctrs * smoothness; k++) { // each circle is made of segments, sectors * smoothness
					// formula: start at starPos, then rotate around the quasi-Z-axis (add starting angle to the rotation and shift along the radial line (plus starting radius)
					_drawPoints[0][circleSegmentCounter] = new Vector3[2] { // [origin] + ([rotation by degrees] * [direction] * [distance from origin])
						startPos + (Quaternion.AngleAxis(k * 360.0f / (sectors * smoothness) + startA * Mathf.Rad2Deg, locUnits[idx[2]] * (gridPlane == GridPlane.XY ? 1 : -1)) * locUnits[idx[0]] * (j *radius +startR)),
						startPos + (Quaternion.AngleAxis((k+1) * 360.0f / (sectors * smoothness) + startA * Mathf.Rad2Deg, locUnits[idx[2]] * (gridPlane == GridPlane.XY ? 1 : -1)) * locUnits[idx[0]] * (j * radius + startR))
					};
					circleSegmentCounter++;
				}
			}
			// now draw the radial lines, one loop fills the entire layer
			for (int j = 0; j <= Mathf.Min(sctrs, sectors - 1); j++) {
				// formula: start at startPos, then rotate around the quasi-Z-axis and add the start/ending length along the radial line
				_drawPoints[1][radialCounter] = new Vector3[2] {
					startPos + (Quaternion.AngleAxis(j * 360.0f / sectors + startA * Mathf.Rad2Deg, locUnits[idx[2]] * (gridPlane == GridPlane.XY ? 1 : -1)) * locUnits[idx[0]] * from[idx[0]]),
					startPos + (Quaternion.AngleAxis(j * 360.0f / sectors + startA * Mathf.Rad2Deg, locUnits[idx[2]] * (gridPlane == GridPlane.XY ? 1 : -1)) * locUnits[idx[0]] * to[idx[0]])
				};
				radialCounter++;
			}
			// increment starting position of the current layer
			startPos += depth * locUnits[idx[2]];
		}
		
		return _drawPoints;
	}
	#endregion
	
	#region helper functions		
	// an extended version of Atan 2; defaults to 0 if x=0 and maps to [0, 2π)
	private static float Atan3 (float y, float x) {
		return Mathf.Atan2(y, x) + (y >= 0 ? 0 : 2 * Mathf.PI);
	}
	
	// the maximum radius of the drawing
	private float MaxRadius (Vector3 from, Vector3 to){
		return Mathf.Min (Mathf.Abs (from[idx[0]]), Mathf.Abs (from[idx[0]]));
	}
	#endregion
}