using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDefaultPopup: CDefaultObjectScene {

	#region CONTRUCTORS

	public CDefaultPopup(): base ()
	{
		
	}

	public CDefaultPopup(string objectName, string objectPath): base (objectName, objectPath)
	{
		
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public override void OnInitObject()
	{
		base.OnInitObject();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
	}

	public override void OnFixedUpdateObject(float fdt)
	{
		base.OnFixedUpdateObject(fdt);
	}

	public override void OnUpdateObject(float dt)
	{
		base.OnUpdateObject(dt);
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	public override void OnEscapeObject()
	{
		base.OnEscapeObject();
	}

	#endregion

}
