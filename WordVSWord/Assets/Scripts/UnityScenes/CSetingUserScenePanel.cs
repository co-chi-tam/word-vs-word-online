using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CSetingUserScenePanel : CDefaultScene {

	#region Fields

	protected InputField m_PlayerNameInputField;
	protected Button m_SubmitButton;
	protected Button m_QuitButton;
	protected GameObject m_AvatarGrid;
	protected Toggle[] m_AvatarButtons;

	protected Image m_CurrentAvatarImage;

	public static bool USER_AUTHORIZE_READY = false;

	#endregion
	
	#region Constructor

	public CSetingUserScenePanel() : base()
	{

	}

	public CSetingUserScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_PlayerNameInputField = CRootManager.FindObjectWith(GameObject, "PlayerNameInputField").GetComponent<InputField>();
		this.m_SubmitButton = CRootManager.FindObjectWith(GameObject, "SubmitButton").GetComponent<Button>();
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
		// AVATAR
		this.m_CurrentAvatarImage = CRootManager.FindObjectWith(GameObject, "CurrentAvatarImage").GetComponent<Image>();
		this.m_AvatarGrid = CRootManager.FindObjectWith(GameObject, "AvatarGrid");
		this.m_AvatarButtons = this.m_AvatarGrid.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			this.m_AvatarButtons[i].onValueChanged.RemoveAllListeners();
			this.m_AvatarButtons[i].onValueChanged.AddListener (this.OnAvatarSelectionClick);
		}
		// SETTING
		USER_AUTHORIZE_READY = false;
		// EVENTS
		CSocketManager.Instance.On("playerNameSet", this.ReceivePlayerName);
		CSocketManager.Instance.On("msgError", this.ReceiveMessageError);
		// EVENTS
		this.m_SubmitButton.onClick.AddListener(this.OnSubmitClick);
		this.m_QuitButton.onClick.AddListener(this.OnQuitClick);
		// ADMOB
		CAdmobManager.Init();
		CAdmobManager.InitBanner();
		CAdmobManager.InitRewardedVideo();
		if (CGameSetting.HasTimeToAd() == false)
			CGameSetting.ResetTimerToAd();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// PLAYER NAME
		this.m_PlayerNameInputField.text = CGameSetting.USER_NAME;
		// UI
		this.m_SubmitButton.interactable = true;
		this.m_PlayerNameInputField.interactable = true;
		this.EnableGridAvatars(true);
		// PLAYER AVATAR
		if (CGameSetting.USER_AVATAR < this.m_AvatarButtons.Length)
		{
			this.m_AvatarButtons[CGameSetting.USER_AVATAR].isOn = true;
		}
		else
		{
			for (int i = 0; i < this.m_AvatarButtons.Length; i++)
			{
				this.m_AvatarButtons[i].isOn = false;
			}
		}
		this.m_CurrentAvatarImage.sprite = CGameSetting.GetAvatarSprite(CGameSetting.USER_AVATAR);
		// SETTING
		USER_AUTHORIZE_READY = false;
		// ADMOB
		CAdmobManager.LoadBanner();
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

	private void OnAvatarSelectionClick(bool value)
	{
		CSoundManager.Instance.Play ("sfx_click");
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			if (this.m_AvatarButtons[i].isOn)
			{
				this.m_CurrentAvatarImage.sprite = CGameSetting.GetAvatarSprite(i);
				CGameSetting.USER_AVATAR = i;
				break;
			}
		}
	}

	private void OnSubmitClick()
	{
		if (CSocketManager.Instance.IsConnected() == false)
			return;
		CSoundManager.Instance.Play ("sfx_click");
		var playerName = this.m_PlayerNameInputField.text;
		// USER NAME
		if (string.IsNullOrEmpty(playerName))
			return;
		// SAVE
		CGameSetting.USER_NAME = playerName;
		// UI
		this.m_SubmitButton.interactable = false;
		this.m_PlayerNameInputField.interactable = false;
		this.EnableGridAvatars(false);
		// AVATAR
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			if (this.m_AvatarButtons[i].isOn)
			{
				CGameSetting.USER_AVATAR = i;
				break;
			}
		}
		// SET PLAYER NAME
		SetPlayerInfo(CGameSetting.USER_NAME, CGameSetting.USER_AVATAR, CGameSetting.USER_FRAME);
	}

	private void ReceivePlayerName (SocketIO.SocketIOEvent ev) 
	{
		// UI
		this.m_SubmitButton.interactable = true;
		this.m_PlayerNameInputField.interactable = true;
		USER_AUTHORIZE_READY = true;
		this.EnableGridAvatars(true);
		// SWITCH SCENE
		CRootManager.Instance.ShowScene("RoomDisplayPanel");
	}

	private void ReceiveMessageError(SocketIO.SocketIOEvent ev)
	{
		if (this.GameObject.activeInHierarchy == false)	
			return;
		// UI
		this.m_SubmitButton.interactable = true;
		this.m_PlayerNameInputField.interactable = true;
		USER_AUTHORIZE_READY = false;
		this.EnableGridAvatars(true);
		CSocketManager.Instance.Invoke("ShowLastErrorPopup", 0f);
	}

	private void EnableGridAvatars(bool value)
	{
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			this.m_AvatarButtons[i].interactable = value;
		}
	}

	private void OnQuitClick()
	{
		Application.Quit();
	}

	#endregion

	#region Public

	public static void SetPlayerInfo(string playerName, int avatarIndex, int frame) 
	{
		// DATA
		var userData = new JSONObject();
		userData.AddField("playerAvatar", avatarIndex);
		userData.AddField("playerName", playerName);
		userData.AddField("playerFrame", frame);
		// EMIT
		CSocketManager.Instance.Emit("setPlayerName", userData);
	}

	#endregion

}
