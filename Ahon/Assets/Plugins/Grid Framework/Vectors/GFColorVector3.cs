using UnityEngine;
using System.Collections;

/// <summary>
/// Three colours grouped together like a vector
/// </summary>
///<para>
/// This class groups three colours together, similar to how Vector3 groups three float numbers together.
/// Just like Vector3 you can read and assign values using x, y, or an indexer.
/// </para>
[System.Serializable]
public class GFColorVector3{
	/// <summary>X component of the colour vector</summary>
	public Color x;
	/// <summary>Y component of the colour vector</summary>
	public Color y;
	/// <summary>Z component of the colour vector</summary>
	public Color z;

	/// <summary>
	/// Access the X, Y or Z components using [0], [1], [2] respectively
	/// </summary>
	/// <param name="i">The index</param>
	/// Access the x, y, z components using [0], [1], [2] respectively. Example:
	/// \code
	/// GFColorVector3 c = new GFColorVector3;();
	/// c[1] = Color.green; // the same as c.y = green
	/// \endcode
	public Color this[int i]{
		get{if(i == 0){
				return x;
			} else if(i == 1){
				return y;
			} else if(i == 2){
				return z;
			} else{
				return Color.white;
			}
		}
		set{switch(i){
				case 0: x = value;break;
				case 1: y = value;break;
				case 2: z = value;break;
			}}
	}

	/// <summary>Creates a new bool vector with given X, Y and Z components</summary>
	public GFColorVector3(Color xColor, Color yColor, Color zColor){ //taking individual colours
		x = xColor; y = yColor;	 z = zColor;
	}
	/// <summary>Creates a new standard RGB <see cref="GFColorVector3"/> where all three colours have their alpha set to 0.5</summary>
	public GFColorVector3(){ //default
		x = new Color(1.0f, 0.0f, 0.0f, 0.5f);
		y = new Color(0.0f, 1.0f, 0.0f, 0.5f);
		z = new Color(0.0f, 0.0f, 1.0f, 0.5f);
	}
	/// <summary>Creates a new <see cref="GFColorVector3"/> where all components are set to the same colour.</summary>
	public GFColorVector3(Color color){ //one colour for everything
		x = color; y = color; z = color;
	}

	/// <summary>Shorthand writing for <see cref="GFColorVector3()"></summary>
	public static GFColorVector3 RGB {get{return new GFColorVector3();}} // standard RGB Colour Vector
	/// <summary>Shorthand writing for <c>GFColorVector3(Color(0,1,1,0.5), Color(1,0,1,0.5), Color(1,1,0,0.5))</c></summary>
	public static GFColorVector3 CMY {get{return new GFColorVector3(new Color(0, 1, 1, 0.5f), new Color(1, 0, 1, 0.5f), new Color(1, 1, 0, 0.5f));}}
	/// <summary>Shorthand writing for <c>GFColorVector3(Color(0,0,0,0.5), Color(0.5,0.5,0.5,0.5), Color(1,1,1,0.5))</c></summary>
	public static GFColorVector3 BGW {get{return new GFColorVector3(new Color(0, 0, 0, 0.5f), new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(1, 1, 1, 0.5f));}}
}