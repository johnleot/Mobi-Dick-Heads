using UnityEngine;
using System.Collections;

public abstract class IObject: MonoBehaviour {
	
	protected GameObject MainUI;
	protected GameObject objectBtnHolder_;
	
	protected string title_;
	protected string contentText_;
	protected int maxNumberofOccupants_;
	//protected int someInt;

	public abstract void insertUI();
	public abstract void showUI();
	public abstract void hideUI();
	public abstract void removeUI();

	public int MaxNumberofOccupants_ {
		get {
			return maxNumberofOccupants_;
		}
		set {
			maxNumberofOccupants_ = value;
		}
	}
}
