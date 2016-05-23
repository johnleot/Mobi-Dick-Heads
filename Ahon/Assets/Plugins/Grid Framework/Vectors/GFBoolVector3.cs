using UnityEngine;
using System.Collections;

/// <summary>
/// Three booleans grouped together like a vector.
/// </summary>
/// <para>
/// This class groups three booleans together, similar to how Vector3 groups three float numbers together.
/// Just like Vector3 you can read and assign values using x, y, or an indexer.
/// </para>
[System.Serializable]
public class GFBoolVector3 {
	/// <summary>X component of the bool vector</summary>
	public bool x;
	/// <summary>Y component of the bool vector</summary>
	public bool y;
	/// <summary>Z component of the bool vector</summary>
	public bool z;

	/// <summary>
	/// Access the X, Y or Z components using [0], [1], [2] respectively
	/// </summary>
	/// <param name="i">The index</param>
	/// Access the x, y, z components using [0], [1], [2] respectively. Example:
	/// \code
	/// GFBoolVector3 b = new GFBoolVector3();
	/// b[1] = true; // the same as b.y = true
	/// \endcode
	public bool this[int i]{
		get{if(i == 0){
				return x;
			} else if(i == 1){
				return y;
			} else if(i == 2){
				return z;
			} else{
				return false;
			}
		}
		set{switch(i){
				case 0: x = value;break;
				case 1: y = value;break;
				case 2: z = value;break;
			}}
	}

	/// <summary>Creates a new bool vector with given X, Y and Z components</summary>
	public GFBoolVector3(bool xBool, bool yBool, bool zBool){
		x = xBool; y = yBool; z = zBool;
	}
	/// <summary>Creates an all-<c>false</c> <see cref="GFBoolVector3"/></summary>
	public GFBoolVector3(){
		x = false; y = false; z = false;
	}
	/// <summary>Creates a new <see cref="GFBoolVector3"/> set to <c>condition</c></summary>
	public GFBoolVector3(bool condition){
		x = condition; y = condition; z = condition;
	}	

	/// <summary>Creates a new all-<c>false</c> <see cref="GFBoolVector3"/></summary>
	public static GFBoolVector3 False {get{return new GFBoolVector3(false);}}
	/// <summary>Creates a new all-<c>true</c> <see cref="GFBoolVector3"/></summary>
	public static GFBoolVector3 True {get{return new GFBoolVector3(true);}}
}