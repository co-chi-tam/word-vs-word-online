using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CSetingUserScenePanel : CDefaultScene {

	#region Fields

	protected InputField m_PlayerNameInputField;
	protected Button m_SubmitButton;
	protected GameObject m_AvatarGrid;
	protected Toggle[] m_AvatarButtons;

	protected bool m_SettingComplete = false;

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
		this.m_AvatarGrid = CRootManager.FindObjectWith(GameObject, "AvatarGrid");
		this.m_AvatarButtons = this.m_AvatarGrid.GetComponentsInChildren<Toggle>();
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			// AVATAR
			this.m_AvatarButtons[i].onValueChanged.RemoveAllListeners();
			this.m_AvatarButtons[i].onValueChanged.AddListener ((value) => {
				CSoundManager.Instance.Play ("sfx_click");
			});
		}
		// SETTING
		this.m_SettingComplete = false;
		// EVENTS
		CSocketManager.Instance.On("playerNameSet", this.ReceivePlayerName);
		CSocketManager.Instance.On("msgError", this.ReceiveMessageError);
		// EVENTS
		this.m_SubmitButton.onClick.AddListener(this.OnSubmitClick);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// PLAYER NAME
		this.m_PlayerNameInputField.text = CGameSetting.USER_NAME;
		this.m_SubmitButton.interactable = true;
		// PLAYER AVATAR
		this.m_AvatarButtons[CGameSetting.USER_AVATAR].isOn = true;
		// SETTING
		this.m_SettingComplete = false;
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	public override void OnEscapeObject()
	{
		// base.OnEscapeObject();
	}

	#endregion

	#region Private

	private void OnSubmitClick()
	{
		CSoundManager.Instance.Play ("sfx_click");
		// USER NAME
		if (string.IsNullOrEmpty(this.m_PlayerNameInputField.text))
			return;
		// SAVE
		CGameSetting.USER_NAME = this.m_PlayerNameInputField.text;
		// UI
		this.m_SubmitButton.interactable = false;
		// AVATAR
		var avatarIndex = 0;
		for (int i = 0; i < this.m_AvatarButtons.Length; i++)
		{
			if (this.m_AvatarButtons[i].isOn)
			{
				avatarIndex = i;
				break;
			}
		}
		CGameSetting.USER_AVATAR = avatarIndex;
		// SET PLAYER NAME
		this.SetPlayerInfo(this.m_PlayerNameInputField.text, avatarIndex);
	}

	private void SetPlayerInfo(string playerName, int avatarIndex) 
	{
		// DATA
		var userData = new JSONObject();
		userData.AddField("playerAvatar", avatarIndex);
		userData.AddField("playerName", playerName);
		// EMIT
		CSocketManager.Instance.Emit("setPlayerName", userData);
	}

	private void ReceivePlayerName (SocketIO.SocketIOEvent ev) 
	{
		// UI
		this.m_SubmitButton.interactable = true;
		// SWITCH SCENE
		CRootManager.Instance.ShowScene("RoomDisplayPanel");
	}

	private void ReceiveMessageError(SocketIO.SocketIOEvent ev)
	{
		if (this.GameObject.activeInHierarchy == false)	
			return;
		// UI
		this.m_SubmitButton.interactable = true;
		CSocketManager.Instance.Invoke("ShowLastErrorPopup", 0f);
	}

	#endregion

	#region Public

	public virtual bool IsSettingComplete()
	{
		return this.m_SettingComplete;
	}

	#endregion

}
