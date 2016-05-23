using UnityEngine;
using System.Collections;

public static class GFTransformExtensions{

	//works the same as Transform.Transform point but independent from the scale, i.e. transforms
	//a point from LocalSpace to WorldSpace without takign the scale into account. This is not
	//the same as Grid space
	public static Vector3 GFTransformPointFixed(this Transform theTransform, Vector3 position){
		//return theTransform.localToWorldMatrix.MultiplyPoint3x4(position);
		return theTransform.TransformDirection(position) + theTransform.position;
	}
	
	//
	public static Vector3 GFInverseTransformPointFixed(this Transform theTransform, Vector3 position){
		return theTransform.InverseTransformDirection(position - theTransform.position);
	}
}
