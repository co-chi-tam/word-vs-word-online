using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CLoadingScenePanel : CDefaultScene {

	protected WaitForSeconds m_ShortTime = new WaitForSeconds(1f);

	public CLoadingScenePanel() : base()
	{

	}

	public CLoadingScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	public override void OnInitObject()
	{
		base.OnInitObject();
		// INIT SOCKET
		CSocketManager.Instance.Init();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// EVENTS
		CSocketManager.Instance.OnConnect -= this.OnConnectSocket;
		CSocketManager.Instance.OnConnect += this.OnConnectSocket;
		// CONNECT
		CSocketManager.Instance.Connect(true);
	}

	public override void OnDestroyObject()
	{
		// EVENTS
		CSocketManager.Instance.OnConnect -= this.OnConnectSocket;
	}

	protected IEnumerator HandleMoveNextScene()
	{
		yield return this.m_ShortTime;
		CRootManager.Instance.ShowScene("SetingUserPanel");
	}

	private void OnConnectSocket()
	{
		CRootManager.Instance.StopCoroutine(this.HandleMoveNextScene());
		CRootManager.Instance.StartCoroutine(this.HandleMoveNextScene());
	}

}
