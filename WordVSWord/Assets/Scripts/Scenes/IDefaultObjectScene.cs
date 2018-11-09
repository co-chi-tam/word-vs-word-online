using System;
using UnityEngine;

public interface IDefaultObjectScene {

	void OnInitObject();
	void OnStartObject();
	void OnFixedUpdateObject(float fdt);
	void OnUpdateObject(float dt);
	void OnDestroyObject();
	void OnEscapeObject();

	void SetActive(bool value);
	void SetObjectSceneAnimator(string name, object value);
	void SetObjectSceneAnimatorAsync(float time, string name, object value, System.Action<IDefaultObjectScene> complete);

}
