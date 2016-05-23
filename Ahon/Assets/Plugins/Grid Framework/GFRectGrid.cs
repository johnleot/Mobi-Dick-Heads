using UnityEngine;
using System.Collections;

/// <summary>
/// A standard three-dimensional rectangular grid.
/// </summary>


public class GFRectGrid: GFGrid{
	
	#region Class Members
	[SerializeField]
	private Vector3 _spacing = Vector3.one;
	public Vector3 spacing{
		get{return _spacing;}
		set{if(value == _spacing)// needed because the editor fires the setter even if this wasn't changed
				return;
			_gridChanged = true;
			_spacing = Vector3.Max(value, 0.1f*Vector3.one);
		}
	}

	#region Helper Values (read-only)
	public Vector3 right { get { return spacing.x * _transform.right; } }
	public Vector3 up { get { return spacing.y * _transform.up; } }
	public Vector3 forward { get { return spacing.z * _transform.forward; } }
	#endregion
	#endregion
	
//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
	#region grid to world	
	public override Vector3 WorldToGrid(Vector3 worldPoint){
		return gwMatrix.inverse.MultiplyPoint3x4(worldPoint);
		//return _transform.GFInverseTransformPointFixed(worldPoint).GFReverseScale(spacing);
	}
	
	//takes the coordinates of something inside the grid and returns its world coordinates
	public override Vector3 GridToWorld(Vector3 gridPoint){
		return gwMatrix.MultiplyPoint(gridPoint);
		//return _transform.GFTransformPointFixed(Vector3.Scale(gridPoint, spacing));
	}
	#endregion
	
//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
	#region nearest in world space
	public override Vector3 NearestVertexW(Vector3 fromPoint, bool doDebug = false){
		//convert fromPoint to grid coordinates first
		Vector3 toPoint = WorldToGrid(fromPoint);
		
		// each coordinate has to be set to a multiple of spacing
		for(int i = 0; i<=2; i++){
			toPoint[i] = Mathf.Round(toPoint[i]);
		}
		
		//back to World coordinates
		toPoint = GridToWorld(toPoint);
		
		if(doDebug){
			Gizmos.DrawSphere(GridToWorld(NearestVertexG(fromPoint)), 0.3f);
			//Gizmos.DrawSphere(toPoint, 0.3f);
		}
		//return toPoint;
		return GridToWorld(NearestVertexG(fromPoint));
	}
	
	public override Vector3 NearestFaceW(Vector3 fromPoint, GridPlane thePlane, bool doDebug = false){
		//get a temporary point (world space)
		//Vector3 toPoint = NearestBoxW(fromPoint);
		
		//snap to the plane
		//toPoint[(int)thePlane] = NearestVertexW(fromPoint)[(int)thePlane];
		
		//debugging
		if(doDebug){
			Vector3 debugCube = spacing;
			debugCube[(int)thePlane] = 0.0f;
			
			//store the old matrix and create a new one based on the grid's roation and the point's position
			Matrix4x4 oldRotationMatrix = Gizmos.matrix;
			//Matrix4x4 newRotationMatrix = Matrix4x4.TRS(toPoint, transform.rotation, Vector3.one);
			Matrix4x4 newRotationMatrix = Matrix4x4.TRS(GridToWorld(NearestFaceG(fromPoint, thePlane) + 0.5f * Vector3.one - 0.5f * units[(int)thePlane]), transform.rotation, Vector3.one);
			
			Gizmos.matrix = newRotationMatrix;
			Gizmos.DrawCube(Vector3.zero, debugCube);//Position zero because the matrix already contains the point
			Gizmos.matrix = oldRotationMatrix;
		}
		
		//return toPoint;
		return GridToWorld(NearestFaceG(fromPoint, thePlane) + 0.5f * Vector3.one - 0.5f * units[(int)thePlane]);
	}
	
	public override Vector3 NearestBoxW(Vector3 fromPoint, bool doDebug = false){
		//convert fromPoint to grid coordinates first
		//Vector3 toPoint = WorldToGrid(fromPoint);
		
		//loops through the XYZ components
		//for(int i = 0; i<=2; i++){
		//	toPoint[i] = Mathf.Round(toPoint[i]-0.5f)+0.5f;
		//}
		
		//toPoint = GridToWorld(toPoint);
		
		if(doDebug){
			//store the old matrix and create a new one based on the grid's roation and the point's position
			Matrix4x4 oldRotationMatrix = Gizmos.matrix;
			//Matrix4x4 newRotationMatrix = Matrix4x4.TRS(toPoint, transform.rotation, Vector3.one);
			Matrix4x4 newRotationMatrix = Matrix4x4.TRS(GridToWorld(NearestBoxG(fromPoint) + 0.5f * Vector3.one), transform.rotation, Vector3.one);
			
			
			Gizmos.matrix = newRotationMatrix;
			Gizmos.DrawCube(Vector3.zero, spacing);
			Gizmos.matrix = oldRotationMatrix;
		}
		//convert back to world coordinates
		//return toPoint;
		return GridToWorld(NearestBoxG(fromPoint) + 0.5f * Vector3.one);
	}
	
	#endregion
	
//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-

	#region nearest in grid space
	
	//returns XYZ grid coordinates of a vertex close to a given point
	public override Vector3 NearestVertexG(Vector3 fromPoint){
		return RoundPoint(WorldToGrid(fromPoint));
		//return WorldToGrid(NearestVertexW(fromPoint));
	}
	
	/*		A NOTE ABOUT BOXES AND FACES
	 * 
	 * For boxes and faces I need to return the coordinates of the space between two grid
	 * "bars", but there is no central one I could use a zero. That's why I have chosen
	 * the one right from the zero bar (i. e. positive from the centre) as zero.
	 * 
	 * 	EXAMPLE: for the X-axis
	 * 		
	 * 		i.i.i.i:I.i.i.i
	 * 		i.i.i.i:I.i.i.i
	 * 		i.i.i.i:I.i.i.i
	 * 
	 * 	the dots are vertices, the lower case i are the spaces between them and the colon (:)
	 * 	is the central vertex. The space between the central vertex and vertex 1 is the zero
	 * 	space.
	*/  
	
	//returns XYZ grid coordinates of a box close to a given point
	public override Vector3 NearestBoxG(Vector3 fromPoint){
		return RoundPoint(WorldToGrid(fromPoint) - 0.5f * Vector3.one);
		//return WorldToGrid(NearestBoxW(fromPoint)) - 0.5f * Vector3.one;
	}
	
	//returns XYZ grid coordinates of a face close to a given point
	public override Vector3 NearestFaceG(Vector3 fromPoint, GridPlane thePlane){
		//get the grid coordinates of the face
		//Vector3 face = NearestBoxG(NearestFaceW(fromPoint, thePlane));
		// two of the face coordinates are in a box, the other is on the vertex closest to the face
		//face[(int)thePlane] = NearestVertexW(fromPoint)[(int)thePlane];
		
		//return face;
		return RoundPoint(WorldToGrid(fromPoint) - 0.5f * Vector3.one + 0.5f * units[(int)thePlane]);
	}
	
	#endregion
	
//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
	#region VertexMatrixMethods
	
	//NOTE ABOUT MATRIX ORDER: The matrix starts in the topright most backwards corner, the first component
	//	represents the width, from right to left, the second one the height from top to bottom, the third
	//	the depth from back to front
	
	public override Vector3[,,] BuildVertexMatrix(float width, float height, float depth){
		//prevent negative values
		width = Mathf.Abs(width);
		height = Mathf.Abs(height);
		depth = Mathf.Abs(depth);
		Vector3 matrixSize = new Vector3(width, height, depth);
			
		Vector3 iterationVector = Vector3.zero;
		for(int n=0; n <=2; n++){
			iterationVector[n] = Mathf.Floor(matrixSize[n] / 1.0f);
		}

		Vector3[,,] vertexMatrix = new Vector3[2*(int)Mathf.Floor(width)+1, 2*(int)Mathf.Floor(height)+1, 2*(int)Mathf.Floor(depth)+1];

		for(int i = 0; i <= 2*(int)Mathf.Floor(width); i++){
			for(int j = 0; j <= 2*(int)Mathf.Floor(height); j++){
				for(int k = 0; k <= 2*(int)Mathf.Floor(depth); k++){
					vertexMatrix[i,j,k] = GridToWorld(iterationVector - new Vector3(i,j, k));
				}
			}
		}
//		Debug.Log("Matrix size: " + vertexMatrix.GetUpperBound(0) +"/"+ vertexMatrix.GetUpperBound(1) +"/"+ vertexMatrix.GetUpperBound(2));
		
		return vertexMatrix;
	}
	
	// return an entry from the vertex matrix in a cartesian fashion (central vertex is (0,0,0), first component is the x-axis, second one the y-axis,
	// third one the z-axis
	public override Vector3 ReadVertexMatrix(int x, int y, int z, Vector3[,,] vertexMatrix, bool warning = false){
		if(Mathf.Abs(x)>vertexMatrix.GetUpperBound(0)/2 || Mathf.Abs(y) >vertexMatrix.GetUpperBound(1)/2 || Mathf.Abs(z) >vertexMatrix.GetUpperBound(2)/2){
			if(warning)
				Debug.LogWarning("coordinates too large for this matrix, will default to " + Vector3.zero);
			return vertexMatrix[(vertexMatrix.GetUpperBound(0)/2), (vertexMatrix.GetUpperBound(1)/2), (vertexMatrix.GetUpperBound(2)/2)];
		}
		return vertexMatrix[(vertexMatrix.GetUpperBound(0)/2) - x, (vertexMatrix.GetUpperBound(1)/2) - y, (vertexMatrix.GetUpperBound(2)/2) - z];
	}
	
	#endregion

//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-

	#region AlignScaleMethods
	
	public override Vector3 AlignVector3(Vector3 position, Vector3 scale, GFBoolVector3 lockAxis){
		Vector3 currentPosition = WorldToGrid(position);
		Vector3 newPositionB = WorldToGrid (NearestBoxW(position));
		Vector3 newPositionV = WorldToGrid (NearestVertexW (position));
		Vector3 newPosition = new Vector3();
		
		for (int i = 0; i <=2; i++){
			// vertex or box, depends on whether scale is a multiple of spacing
			newPosition [i] = (scale [i] / spacing [i]) % 2f <= 0.5f ? newPositionV [i] : newPositionB [i];
		}
		
		// don't apply aligning if the axis has been locked+
		for (int i = 0; i < 3; i++) {
			if(lockAxis[i])
				newPosition[i] = currentPosition[i];
		}

		return GridToWorld(newPosition);
	}
	
	//scale the Transform to the grid
	public override Vector3 ScaleVector3(Vector3 scale, GFBoolVector3 lockAxis){
		Vector3 relScale = scale.GFModulo3(spacing);
		Vector3 newScale = Vector3.zero;
		
		for (int i = 0; i <= 2; i++){
			newScale[i] = scale[i];
			
			if(relScale[i] >= 0.5f * spacing[i]){
//				Debug.Log ("Grow by " + (spacing.x - relScale.x));
				newScale[i] = newScale[i] - relScale[i] + spacing[i];
			} else{
//				Debug.Log ("Shrink by " + relativeScale.x);
				newScale[i] = newScale[i] - relScale[i];
				//if we went too far default to the spacing
				if(newScale[i] < spacing[i])
					newScale[i] = spacing[i];
			}		
		}
		
		for(int i = 0; i < 3; i++){
			if(lockAxis[i])
				newScale[i] = scale[i];
		}
		
		return  newScale;
	}
	
	#endregion

//_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
	
	#region Draw Gizoms
	
	public void OnDrawGizmos(){
		if(useCustomRenderRange){
			DrawGrid(renderFrom, renderTo);
		} else{
			DrawGrid();
		}
	}
	
	#endregion
	
	#region Calculate draw points
	//This function returns a three-dimensional jagged array. The most inner layer contains
	// a pair of points for one line, the second layer contains the sets of all lines in the
	// same direction and the third layer contains all the sets.
	//The first set of lines are the horizontal X-lines, their amount depends on Y and Z. The
	// second set are the vertical lines (X and Z), the third set the forward lines (X and Y).
	protected override Vector3[][][] CalculateDrawPoints(Vector3 from, Vector3 to){
		// reuse the points if the grid hasn't changed, we already have some points and we use the same range
		if(!hasChanged && _drawPoints != null && from == renderFrom && to == renderTo){
			return _drawPoints;
		}
		
		// our old points are of no ue, so let's create a new set
		_drawPoints = new Vector3[3][][];
		
		Vector3 relFrom = relativeSize ? Vector3.Scale(from, spacing) : from;
		Vector3 relTo = relativeSize ? Vector3.Scale(to, spacing) : to;
		
		float[] length = new float[3];
		for(int i = 0; i < 3; i++){
			length[i] = relTo[i] - relFrom[i];
		}

		
		//the amount of lines for each direction
		int[] amount = new int[3];
		for(int i = 0; i < 3; i++){
			amount[i] = Mathf.FloorToInt(relTo[i] / spacing[i]) - Mathf.CeilToInt(relFrom[i] / spacing[i]) + 1;
		}
				
		//the starting point of the first pair (an iteration vector will be added to this)
		Vector3[] startPoint = new Vector3[3]{
			//everything in the right top front
			_transform.GFTransformPointFixed(new Vector3(relTo.x, spacing.y * Mathf.Floor(relTo.y / spacing.y), spacing.z * Mathf.Floor(relTo.z / spacing.z))),
			_transform.GFTransformPointFixed(new Vector3(spacing.x * Mathf.Floor(relTo.x / spacing.x), relTo.y, spacing.z * Mathf.Floor(relTo.z / spacing.z))),
			_transform.GFTransformPointFixed(new Vector3(spacing.x * Mathf.Floor(relTo.x / spacing.x), spacing.y * Mathf.Floor(relTo.y / spacing.y), relTo.z))
		};
		
		//this will be added to each first point in a pair
		Vector3[] endDirection = new Vector3[3]{
			_transform.TransformDirection(new Vector3(-Mathf.Abs(relTo.x - relFrom.x), 0.0f, 0.0f)),
			_transform.TransformDirection(new Vector3(0.0f, -Mathf.Abs(relTo.y - relFrom.y), 0.0f)),
			_transform.TransformDirection(new Vector3(0.0f, 0.0f, -Mathf.Abs(relTo.z - relFrom.z)))
		};
		
		//a multiple of this will be added to the starting point for iteration
		Vector3[] iterationVector = new Vector3[3]{
			_transform.TransformDirection(new Vector3(-spacing.x, 0.0f, 0.0f)),
			_transform.TransformDirection(new Vector3(0.0f, -spacing.y, 0.0f)),
			_transform.TransformDirection(new Vector3(0.0f, 0.0f, -spacing.z))
		};
		
		// assemble the array
		for(int i = 0; i < 3; i++){			
			//when collecting the line sets we need to know the amount of lines, it depends on
			// the other two coordinates that don't affect the line's size. Get them using modulo
			int idx1 = ((i+1)%3);
			int idx2 = ((i+2)%3);
			int iterator = 0;//j+k won't do it if one is larger than zero, use this independent iterator
			
			Vector3[][] lineSet = new Vector3[amount[idx1]*amount[idx2]][];
			
			if(relTo[i] - relFrom[i] <= 0.01f){// no need for a huge line set no one will see
				lineSet = new Vector3[0][];
			} else{
				for(int j = 0; j < amount[idx1]; ++j){
					for(int k = 0; k < amount[idx2]; ++k){
						Vector3[] line = new Vector3[2];
						line[0] = startPoint[i] + j*iterationVector[idx1] + k*iterationVector[idx2];
						line[1] = line[0] + endDirection[i];
						lineSet[iterator] = line;
						iterator++;
					}
				}
			}
			_drawPoints[i] = lineSet;
		}
		// if the points were calculated from the outside chances are they don't match the range anymore and the rendering could show the wrong range
		if(from != renderFrom || to != renderTo)
			_gridChanged = true;
		// in that case set _gridChanged to force a second calculation with the proper range
		
		return _drawPoints;
	}
	#endregion
	
	#region Matrices
	private Matrix4x4 gwMatrix {
		get{
			Matrix4x4 _gwMatrix = new Matrix4x4();
			_gwMatrix.SetColumn(0, _transform.right * spacing.x); // the scaled axes form the first three colums as Vector4 (the w component is 0)
			_gwMatrix.SetColumn(1, _transform.up * spacing.y);
			_gwMatrix.SetColumn(2, _transform.forward * spacing.z);
			_gwMatrix.SetColumn(3, _transform.position); // the fourth column is the position of theorigin, used for translation)
			_gwMatrix[15] = 1; // the final matrix entry is always set to 1 by default

			return _gwMatrix;
		}
	}
	#endregion
	
	#region helper methods
	private Vector3 RoundPoint (Vector3 point) {
		return RoundPoint(point, Vector3.one);
	}
	private Vector3 RoundPoint (Vector3 point, Vector3 multi) {
		for(int i = 0; i < 3; i++){
			point[i] = RoundMultiple(point[i], multi[i]);
		}
		return point;
	}
	#endregion
}