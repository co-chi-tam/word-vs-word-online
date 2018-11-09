using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyJSON;
using SimpleSingleton;

public class CRootManager: CMonoSingleton<CRootManager> {

	#region FIELDS

	protected string m_ConfigPath = "UnityScenes/config";
	protected CConfigs m_Configs = null;
	protected Dictionary<string, CDefaultScene> m_SceneMaps = new Dictionary<string, CDefaultScene>();
	protected Dictionary<string, CDefaultPopup> m_PopupMaps = new Dictionary<string, CDefaultPopup>();
	protected Stack<string> m_RoadMap = new Stack<string>();
	[SerializeField]	protected CDefaultScene m_CurrentScene = null;
	[SerializeField]	protected CDefaultPopup m_CurrentPopup = null;

	protected GameObject m_MainCanvas;
	protected GameObject m_TopCanvas;

	#endregion

	#region IMPLEMENTATION MONOBEHAVIOUR

	protected virtual void Start()
	{
		// CONFIGS
		this.LoadConfigs();
	}

	protected virtual void FixedUpdate()
	{
		this.FixedUpdateCurrentScene();
		this.FixedUpdateCurrentPopup();
	}

	protected virtual void Update()
	{
		this.UpdateCurrentScene();
		this.UpdateCurrentPopup();
	}

	#endregion

	#region MAIN METHODS

	protected virtual void LoadConfigs()
	{
		// TRANSFORM
		this.m_MainCanvas = GameObject.FindGameObjectWithTag ("Root");
		this.m_TopCanvas = GameObject.FindGameObjectWithTag ("TopCanvas");
		// CONFIGS
		var configFile = Resources.Load<TextAsset>(this.m_ConfigPath);
		this.m_Configs = JSON.Load(configFile.text).Make<CConfigs>();
		// SCENE
		var scenes = this.m_Configs.scenes;
		for (int i = 0; i < scenes.Length; i++)
		{
			var sceneName = scenes[i].sceneObjectName;
			if (this.m_SceneMaps.ContainsKey(sceneName) == false)
			{
				this.m_SceneMaps.Add (sceneName, null);
			}
			// SETUP
			this.m_SceneMaps[sceneName] = scenes[i];
			this.m_SceneMaps[sceneName].GameObject = this.LoadSceneObject(scenes[i].sceneObjectModel);
			// INIT
			this.m_SceneMaps[sceneName].OnInitObject();
			this.m_SceneMaps[sceneName].SetActive(false);
		}
		// ACTIVE && AWAKE
		for (int i = 0; i < scenes.Length; i++)
		{
			var sceneName = scenes[i].sceneObjectName;
			if (this.m_SceneMaps[sceneName].isStartFirst)
			{
				// START FIRST SCENE
				this.ShowScene(sceneName);
				break;
			}
		}
		// POPUPS
		var popups = this.m_Configs.popups;
		for (int i = 0; i < popups.Length; i++)
		{
			var popupName = popups[i].sceneObjectName;
			if (this.m_PopupMaps.ContainsKey(popupName) == false)
			{
				this.m_PopupMaps.Add (popupName, null);
			}
			// SETUP
			this.m_PopupMaps[popupName] = popups[i];
			this.m_PopupMaps[popupName].GameObject = this.LoadSceneObject(popups[i].sceneObjectModel);
			// INIT
			this.m_PopupMaps[popupName].OnInitObject();
			// ACTIVE
			this.m_PopupMaps[popupName].SetActive(false);
		}
	}

	#endregion

	#region ROAD MAP

	protected virtual void AddRoadMap(string name)
	{
		this.m_RoadMap.Push(name);
	}

	public virtual void Back()
	{
		if (this.m_RoadMap.Count < 2)
			return;
		// CLOSE OBJECT
		var firstObject = this.m_RoadMap.Pop();
		if (this.m_SceneMaps.ContainsKey(firstObject))
		{
			this.CloseScene(firstObject);
		}
		else if (this.m_PopupMaps.ContainsKey(firstObject))
		{
			this.ClosePopup(firstObject);
		}
		// SHOW OBJECT
		var nextObject = this.m_RoadMap.Pop();
		if (this.m_SceneMaps.ContainsKey(nextObject))
		{
			this.ShowScene(nextObject);
		}
		else if (this.m_PopupMaps.ContainsKey(nextObject))
		{
			this.ShowPopup(nextObject);
		}
	}

	public virtual void BackTo(string name)
	{
		while(this.m_RoadMap.Count >= 2)
		{
			if (this.m_RoadMap.Peek() != name)
				this.Back();
			else
				break;
		}
	}

	#endregion

	#region SCENE

	public virtual CDefaultScene ShowScene(string sceneName, bool savePath = true)
	{
		if (string.IsNullOrEmpty(sceneName))
			return null;
		if (this.m_SceneMaps.ContainsKey(sceneName) == false)
			return null;
		// ROAD MAP
		if (savePath)
			this.AddRoadMap (sceneName);
		// CURRENT SCENE
		if (this.m_CurrentScene != null 
			&& this.m_CurrentScene.sceneObjectName == sceneName)
		{
			this.m_CurrentScene.SetActive(true);
			this.m_CurrentScene.GameObject.transform.SetParent(this.m_TopCanvas.transform);
			return this.m_CurrentScene;
		}
		this.m_CurrentScene = null;
		// ACTIVE && AWAKE
		foreach(var item in this.m_SceneMaps)
		{
			if (this.m_CurrentScene == null
				&& item.Key == sceneName)
			{
				item.Value.SetActive(true);
				item.Value.OnStartObject();
				item.Value.GameObject.transform.SetParent(this.m_TopCanvas.transform);
				// item.Value.GameObject.transform.SetAsLastSibling();
				item.Value.SetObjectSceneAnimator("IsShow", true);
				this.m_CurrentScene = item.Value;
			}
			else
			{
				this.CloseScene(item.Key);
			}
		}
		// RETURN
		return this.m_CurrentScene;
	}

	public virtual CDefaultScene CloseScene(string sceneName)
	{
		if (this.m_SceneMaps.ContainsKey(sceneName))
		{
			if (this.m_SceneMaps[sceneName].GameObject.activeInHierarchy)
			{
				this.m_SceneMaps[sceneName].OnDestroyObject();
				this.m_SceneMaps[sceneName].GameObject.transform.SetParent(this.m_MainCanvas.transform);
				this.m_SceneMaps[sceneName].SetObjectSceneAnimatorAsync(CGameSetting.ANIM_OBJECT, "IsShow", false, (go) => {
					go.SetActive(false);
				});
				return this.m_SceneMaps[sceneName];
			}
		}
		return null;
	}

	protected virtual void FixedUpdateCurrentScene()
	{
		if (this.m_CurrentScene == null)
			return;
		// FIXED UPDATE
		this.m_CurrentScene.OnFixedUpdateObject(Time.fixedDeltaTime);
	}

	protected virtual void UpdateCurrentScene()
	{
		if (this.m_CurrentScene == null)
			return;
		// UPDATE
		this.m_CurrentScene.OnUpdateObject(Time.deltaTime);
		// PRESSED ESC
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
		{
			this.m_CurrentScene.OnEscapeObject();
		}
	}

	#endregion

	#region POPUPS

	public virtual CDefaultPopup ShowPopup(string popupName, bool savePath = true)
	{
		if (string.IsNullOrEmpty(popupName))
			return null;
		if (this.m_PopupMaps.ContainsKey(popupName) == false)
			return null;
		// ROAD MAP
		if (savePath)
			this.AddRoadMap (popupName);
		// CURRENT SCENE
		if (this.m_CurrentPopup != null 
			&& this.m_CurrentPopup.sceneObjectName == popupName)
		{
			this.m_CurrentPopup.SetActive(true);
			this.m_CurrentPopup.SetObjectSceneAnimator("IsShow", true);
			this.m_CurrentPopup.GameObject.transform.SetParent(this.m_TopCanvas.transform);
			this.m_CurrentPopup.GameObject.transform.SetAsLastSibling();
			return this.m_CurrentPopup;
		}
		this.m_CurrentPopup = null;
		// ACTIVE && AWAKE
		if (this.m_PopupMaps.ContainsKey(popupName))
		{
			this.m_PopupMaps[popupName].SetActive(true);
			this.m_PopupMaps[popupName].OnStartObject();
			this.m_PopupMaps[popupName].SetObjectSceneAnimator("IsShow", true);
			this.m_PopupMaps[popupName].GameObject.transform.SetParent(this.m_TopCanvas.transform);
			this.m_PopupMaps[popupName].GameObject.transform.SetAsLastSibling();
			this.m_CurrentPopup = this.m_PopupMaps[popupName];
		}
		// RETURN
		return this.m_CurrentPopup;
	}

	public virtual CDefaultPopup ClosePopup(string popupName)
	{
		if (this.m_PopupMaps.ContainsKey(popupName))
		{
			this.m_PopupMaps[popupName].GameObject.transform.SetParent(this.m_TopCanvas.transform);
			this.m_PopupMaps[popupName].GameObject.transform.SetAsLastSibling();
			this.m_PopupMaps[popupName].OnDestroyObject();
			this.m_PopupMaps[popupName].SetObjectSceneAnimatorAsync(CGameSetting.ANIM_OBJECT, "IsShow", false, (go) => {
				this.m_PopupMaps[popupName].GameObject.transform.SetParent(this.m_MainCanvas.transform);
				go.SetActive(false);
			});
			return this.m_PopupMaps[popupName];
		}
		return null;
	}

	protected virtual void FixedUpdateCurrentPopup()
	{
		if (this.m_CurrentPopup == null)
			return;
		// FIXED UPDATE
		this.m_CurrentPopup.OnFixedUpdateObject(Time.fixedDeltaTime);
	}

	protected virtual void UpdateCurrentPopup()
	{
		if (this.m_CurrentPopup == null)
			return;
		// UPDATE
		this.m_CurrentPopup.OnUpdateObject(Time.deltaTime);
	}

	#endregion

	#region  ULTILITES

	protected virtual GameObject LoadSceneObject(string name)
	{
		GameObject result = null;
		if (string.IsNullOrEmpty(name))
			return result;
		// MAIN CANVAS
		if (result == null)
		{
			result = CRootManager.FindObjectSurfaceWith(this.m_MainCanvas.transform, name);
		}
		if (result == null)
		{
			var inResource = this.FindObjectInResourceWith("UnityScenes/{0}", name);
			if (inResource != null)
			{
				result = Instantiate(inResource);
				result.transform.SetParent (this.m_MainCanvas.transform);
			}
		}
		// TOP CANVAS
		if (result == null)
		{
			result = CRootManager.FindObjectSurfaceWith(this.m_TopCanvas.transform, name);
		}
		if (result == null)
		{
			var inResource = this.FindObjectInResourceWith("UnityPopups/{0}", name);
			if (inResource != null)
			{
				result = Instantiate(inResource);
				result.transform.SetParent (this.m_TopCanvas.transform);
			}
			Debug.Log("BBB " + name);
		}
		result.transform.localPosition = Vector3.zero;
		result.transform.localRotation = Quaternion.identity;
		result.transform.localScale = Vector3.one;
		result.name = name;
		return result;
	}

	protected GameObject FindObjectInResourceWith(string pattern, string name)
	{
		if (string.IsNullOrEmpty(name))
			return null;
		return Resources.Load<GameObject>(string.Format(pattern, name));
	}

	public static GameObject FindObjectSurfaceWith(Transform parent, string name)
	{
		if (parent == null || string.IsNullOrEmpty(name))
			return null;
		var childCount = parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			var child = parent.GetChild(i);
			if (child.name == name)
				return child.gameObject;
		}
		return null;
	}

	public static GameObject FindObjectWith(GameObject parent, string name)
	{
		return CRootManager.FindObjectWith(parent.transform, name);
	}

	private static Queue<Transform> FIND_QUEUE = new Queue<Transform>();
	public static GameObject FindObjectWith(Transform parent, string name)
	{
		if (parent == null || string.IsNullOrEmpty(name))
			return null;
		FIND_QUEUE.Clear();
		FIND_QUEUE.TrimExcess();	
		FIND_QUEUE.Enqueue(parent);
		while(FIND_QUEUE.Count > 0)
		{
			var queueNext = FIND_QUEUE.Dequeue();
			var childCount = queueNext.childCount;
			for (int i = 0; i < childCount; i++)
			{
				var child = queueNext.GetChild(i);
				if (child.name == name)
				{
					return child.gameObject;
				}
				else
				{
					FIND_QUEUE.Enqueue(child);
				}
			}
		}
		return null;
	}

	#endregion

}
