using UnityEngine;
using System.Collections;
using Assets.Scripts.Behaviour;


public class ObjectFactory : MonoBehaviour {

	public IObject createInstance(ObjectHandler.objectType objectType)
	{
		switch (objectType) {
		case ObjectHandler.objectType.Pabahay :
			return new Pabahay();
				//break;
		default:
			return null;
			//break;
		}
	}
}
