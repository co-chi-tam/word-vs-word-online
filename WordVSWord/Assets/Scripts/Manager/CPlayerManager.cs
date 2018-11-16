using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleSingleton;

public class CPlayerManager : CMonoSingleton<CPlayerManager> {

	protected bool m_IsInternetConnection = false;
	private WWW m_WWW;
	protected WaitForSeconds m_WaitForSeconds = new WaitForSeconds(3f);
	protected float m_DelayConnect = 0f;
	protected float m_MaximumDelayConnect = 30f;

	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad (this.gameObject);
	}

	protected virtual void Start()
	{
		StartCoroutine (this.HandleConnectInternet());
		CSocketManager.Instance.OnConnect += this.HandleAuthorizePlayer;
		// CSocketManager.Instance.OnDisconnect += this.HandleCloseApplication;
	}

	protected virtual void OnApplicationFocus(bool focusStatus)
	{
		
	}

	protected virtual void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus == false 
			&& CSetingUserScenePanel.USER_AUTHORIZE_READY)
		{
			CSocketManager.Instance.Connect(true);
		}
	}

	protected virtual void OnApplicationQuit()
	{
		 
	}

	private IEnumerator HandleConnectInternet()
	{
		this.m_DelayConnect = 0f;
		while(true)
		{
			this.m_WWW = new WWW("https://www.google.com.vn");
			this.m_IsInternetConnection = false;
			this.m_DelayConnect += 3f;
			if (this.m_DelayConnect >= this.m_MaximumDelayConnect)
			{
				var confirm = CRootManager.Instance.ShowPopup ("ConfirmPopup") as CConfirmPopup;
				confirm.Show ("ERROR", "Internet connection problem.", "QUIT", () => {
					this.HandleCloseApplication();
				});
				yield break;
			}
			yield return this.m_WWW;
			if (this.m_WWW.bytesDownloaded > 0)
			{
				this.m_IsInternetConnection = true;
				this.m_DelayConnect = 0f;
			}
			yield return this.m_WaitForSeconds;
		}
	}

	private void HandleAuthorizePlayer()
	{
		// SET PLAYER NAME
		if (CSetingUserScenePanel.USER_AUTHORIZE_READY)
		{
			CGameSetting.UpdatePlayerData(CGameSetting.USER_NAME, CGameSetting.USER_AVATAR, CGameSetting.USER_FRAME);
		}
	}

	private void HandleCloseApplication()
	{
#if !UNITY_EDITOR
		Application.Quit();
#endif
	}

	private bool IsConnectInternet()
	{
		return this.m_IsInternetConnection;
	}

}
