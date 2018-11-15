using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDefaultObjectScene : IDefaultObjectScene {

	#region FIELDS

	public string sceneObjectName;
	public string sceneObjectModel;

	protected GameObject m_GameObject;
	protected Animator m_ObjectSceneAnimator;

	public GameObject GameObject 
	{ 
		get { return this.m_GameObject; }
		set 
		{ 
			this.m_GameObject = value; 
			if (value != null)
			{
				this.m_ObjectSceneAnimator = value.GetComponent<Animator>();
			}
		}
	}

	#endregion

	#region CONTRUCTORS

	public CDefaultObjectScene()
	{
		this.sceneObjectName = string.Empty;
		this.sceneObjectModel = string.Empty;
	}

	public CDefaultObjectScene(string objectName, string objectPath)
	{
		this.sceneObjectName = objectName;
		this.sceneObjectModel = objectPath;
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public virtual void OnInitObject()
	{

	}

	public virtual void OnStartObject()
	{
		
	}

	public virtual void OnFixedUpdateObject(float fdt)
	{

	}

	public virtual void OnUpdateObject(float dt)
	{

	}

	public virtual void OnDestroyObject()
	{
		
	}

	public virtual void OnBackPress()
	{
		CRootManager.Instance.Back();
	}

	#endregion

	#region GETTER && SETTER

	public virtual void SetActive(bool value)
	{
		if (this.GameObject == null)
			return;
		this.GameObject.SetActive(value);
	}

	public virtual void SetObjectSceneAnimator(string name, object value)
	{
		if (this.m_ObjectSceneAnimator == null)
			return;
		if (value is bool)
		{
			this.m_ObjectSceneAnimator.SetBool (name, (bool) value);
		}
		else if (value is int)
		{
			this.m_ObjectSceneAnimator.SetInteger (name, (int) value);
		}
		else if (value is float)
		{
			this.m_ObjectSceneAnimator.SetFloat (name, (int) value);
		}
		else if (value == null)
		{
			this.m_ObjectSceneAnimator.SetTrigger(name);
		}
	}

	public virtual void SetObjectSceneAnimatorAsync(float time, string name, object value, System.Action<IDefaultObjectScene> complete)
	{
		if (this.GameObject.activeInHierarchy)
		{
			this.SetObjectSceneAnimator(name, value);
			CRootManager.Instance.StopCoroutine(this.HandleObjectAnimatorAsync(time, complete));
			CRootManager.Instance.StartCoroutine(this.HandleObjectAnimatorAsync(time, complete));
		}
	}

	private WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();
	protected IEnumerator HandleObjectAnimatorAsync(float time, System.Action<IDefaultObjectScene> complete)
	{
		var countTimer = 0f;
		while(countTimer < time)
		{
			yield return this.m_WaitForFixedUpdate;
			countTimer += Time.fixedDeltaTime;
		}
		if (complete != null)
		{
			complete (this);
		}
	}
	
	#endregion

}
