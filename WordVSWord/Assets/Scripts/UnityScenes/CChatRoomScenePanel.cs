using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CChatRoomScenePanel : CDefaultScene {

	#region Fields

	protected Text m_PlayerNameText;

	protected Text m_ChatRoomText;
	protected ScrollRect m_ChatScrollRect;

	protected InputField m_ChatInputField;
	protected Button m_SubmitChatButton;

	protected Button m_QuitButton;

	protected float m_ChatDelay = -1f;

	#endregion

	#region Constructor

	public CChatRoomScenePanel() : base()
	{

	}

	public CChatRoomScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion
	
	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_PlayerNameText = CRootManager.FindObjectWith(GameObject, "PlayerNameText").GetComponent<Text>(); 
		this.m_ChatRoomText = CRootManager.FindObjectWith(GameObject, "ChatRoomText").GetComponent<Text>();
		this.m_ChatRoomText.text = string.Empty;
		this.m_ChatScrollRect = CRootManager.FindObjectWith (GameObject, "ChatScrollRect").GetComponent<ScrollRect>();
		this.m_ChatInputField = CRootManager.FindObjectWith(GameObject, "ChatInputField").GetComponent<InputField>();
		this.m_SubmitChatButton = CRootManager.FindObjectWith (GameObject, "SubmitChatButton").GetComponent<Button>();
		this.m_QuitButton = CRootManager.FindObjectWith (GameObject, "QuitButton").GetComponent<Button>();
		// EVENTS
		CSocketManager.Instance.On ("msgWorldChat", this.OnReceiveWordChat);
		// EVENTS
		this.m_SubmitChatButton.onClick.AddListener (this.OnSubmitWorldChat);
		this.m_QuitButton.onClick.AddListener (this.OnQuitClick);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		this.m_PlayerNameText.text = CGameSetting.USER_NAME;
	}

	public override void OnUpdateObject(float dt)
	{
		base.OnUpdateObject(dt);
		// DELAY CHAT
		if (this.m_ChatDelay > 0)
		{
			this.m_ChatDelay -= dt;
		}
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	#endregion

	#region Private

	private void OnReceiveWordChat(SocketIO.SocketIOEvent ev)
	{
		var user = ev.data.GetField("user").ToString().Replace("\"", string.Empty);
		var msg = ev.data.GetField("message").ToString().Replace("\"", string.Empty);
		var display = string.Format("<b><color=#00B3FF>[{0}]</color></b>: {1}\n", user, msg);
		this.m_ChatRoomText.text += display;
		this.m_ChatScrollRect.verticalNormalizedPosition = 0;
	}

	private void OnSubmitWorldChat()
	{
		var chatText = this.m_ChatInputField.text;
		if (string.IsNullOrEmpty(chatText))
			return;
		if (chatText.Length < 2)
			return;
		if (this.m_ChatDelay > 0)
			return;
		var data = new JSONObject();
		data.AddField("message", chatText);
		CSocketManager.Instance.Emit("sendWorldChat", data);
		// CHAT BOX
		this.m_ChatInputField.text = string.Empty;
		// SCROLL RECT TO BOTTOM
		this.m_ChatScrollRect.verticalNormalizedPosition = 0f;
		// DELAY
		this.m_ChatDelay = CGameSetting.CHAT_DELAY;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnQuitClick()
	{
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}
	
	#endregion

}
