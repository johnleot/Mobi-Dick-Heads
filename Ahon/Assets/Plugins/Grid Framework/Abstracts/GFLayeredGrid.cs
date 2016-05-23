using UnityEngine;
using System.Collections;

public abstract class GFLayeredGrid : GFGrid {
	
	#region class members
	[SerializeField]
	private float _depth = 1.0f;
	public float depth{
		get{return _depth;}
		set{if(value == _depth)// needed because the editor fires the setter even if this wasn't changed
				return;
			_depth = Mathf.Max(value, 0.1f);
			_gridChanged = true;
		}
	}
	
	// the layers will be parallel the the specified plane
	[SerializeField]
	protected GridPlane _gridPlane = GridPlane.XY;
	public virtual GridPlane gridPlane {
		get {
			return _gridPlane;}
		set {
			if(value == _gridPlane)
				{return;}
			_gridPlane = value;
			_gridChanged = true;
		}
	}
	
	#region helper values (read only)
	// the indices of the axes transformed to quasi-spcae (i.e. the Z-axis works like the Y-axis in XZ-grids)
	protected int[] idx {get {return TransformIndices(gridPlane);}}
	
	//right, up and forward relative to the grid's Transform (i.e. in local space)
	protected Vector3[] locUnits {get { return new Vector3[3] { _transform.right, _transform.up, _transform.forward } ; } }
	#endregion
	#endregion

	#region helper functions
	//transforms from quasi axis to real axis. Quasi axis is the relative X, Y and Z n the current grid plane,
	// all calculations are done in quasi space, so there is only one calculation, and then transformed into real space
	protected virtual int[] TransformIndices(GridPlane plane){
		if(plane == GridPlane.YZ){
			return new int[3] {2, 1, (int)gridPlane};
		} else if(plane == GridPlane.XZ){
			return new int[3] {0, 2, (int)gridPlane};
		} else{
			return new int[3] {0, 1, (int)gridPlane};
		}
	}
	#endregion
}
