using UnityEngine;
using System.Collections;

public abstract class IObject: MonoBehaviour {
	
	protected GameObject MainUI;
	protected GameObject objectBtnHolder_;
	
	protected string title_;
	protected string contentText_;
	protected int someInt;

	public abstract void showUI();
	public abstract void hideUI();
	public abstract void removeUI();
}
