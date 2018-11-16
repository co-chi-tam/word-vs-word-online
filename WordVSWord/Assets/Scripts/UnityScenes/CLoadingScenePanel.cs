using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CLoadingScenePanel : CDefaultScene {

	#region Fields

	protected WaitForSeconds m_ShortTime = new WaitForSeconds(1f);

	#endregion

	#region Constructor

	public CLoadingScenePanel() : base()
	{

	}

	public CLoadingScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// INIT SOCKET
		CSocketManager.Instance.Init();
		CPlayerManager.GetInstance();
		// ADMOB
		CAdmobManager.Init();
		CAdmobManager.InitBanner();
		CAdmobManager.InitRewardedVideo();
		CAdmobManager.LoadBanner();
		CAdmobManager.LoadRewardedVideo();
		if (CGameSetting.HasTimeToAd() == false)
			CGameSetting.ResetTimerToAd();
		// EVENTS
		CSocketManager.Instance.On ("welcome", this.OnReceiveWelcome);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// CONNECT
		CSocketManager.Instance.Connect(true);
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	public override void OnBackPress()
	{
		// base.OnEscapeObject();
	}
	
	#endregion

	#region Private

	private void OnReceiveWelcome(SocketIO.SocketIOEvent ev)
	{
		this.OnConnectSocket();
	}

	private void OnConnectSocket()
	{
		if (this.GameObject.activeInHierarchy == false)	
			return;
		CRootManager.Instance.StopCoroutine(this.HandleMoveNextScene());
		CRootManager.Instance.StartCoroutine(this.HandleMoveNextScene());
	}

	private IEnumerator HandleMoveNextScene()
	{
		yield return this.m_ShortTime;
		CRootManager.Instance.ShowScene("SetingUserPanel", true);
	}
	
	#endregion

}
