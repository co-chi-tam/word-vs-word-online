using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CRoomDisplayScenePanel : CDefaultScene {

	#region Fields

	protected GameObject m_RoomContent;
	protected CRoomItem m_RoomItemPrefab;
	protected List<CRoomItem> m_Rooms;

	protected Button m_JoinRandomButton;
	protected Button m_RefreshButton;
	protected Button m_ExitButton;
	protected Button m_OpenChatButton;
	protected Button m_SoundOnOffButton;
	protected GameObject m_SoundOnImage;
	protected GameObject m_SoundOffImage;


	protected Text m_GoldDisplayText;

	protected bool m_RoomLoaded = false;

	#endregion
	
	#region Constructor

	public CRoomDisplayScenePanel() : base()
	{

	}

	public CRoomDisplayScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion
	
	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// ROOM
		this.m_Rooms = new List<CRoomItem>();
		this.m_RoomContent = CRootManager.FindObjectWith(GameObject, "RoomContent");
		this.m_RoomItemPrefab = Resources.Load<CRoomItem>("Items/RoomItem");
		this.m_RoomLoaded = false;
		// BUTTONS
		this.m_JoinRandomButton = CRootManager.FindObjectWith(GameObject, "JoinRandomButton").GetComponent<Button>();
		this.m_RefreshButton = CRootManager.FindObjectWith(GameObject, "RefreshButton").GetComponent<Button>();
		this.m_ExitButton = CRootManager.FindObjectWith(GameObject, "ExitButton").GetComponent<Button>();
		this.m_OpenChatButton = CRootManager.FindObjectWith(GameObject, "OpenChatButton").GetComponent<Button>();
		this.m_SoundOnOffButton = CRootManager.FindObjectWith(GameObject, "SoundOnOffButton").GetComponent<Button>();
		this.m_SoundOffImage = CRootManager.FindObjectWith(GameObject, "SoundOffImage");
		this.m_SoundOnImage = CRootManager.FindObjectWith(GameObject, "SoundOnImage");
		this.m_SoundOffImage.SetActive (CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOnImage.SetActive (!CGameSetting.SETTING_SOUND_MUTE);
		// GOLD
		this.m_GoldDisplayText = CRootManager.FindObjectWith (GameObject, "GoldDisplayText").GetComponent<Text>();
		// OFF EVENTS
		// CSocketManager.Instance.Off("updateRoomStatus", this.ReceiveRoomList);
		// CSocketManager.Instance.Off("joinRoomFailed", this.OnJoinRoomFailed);
		// ON EVENTS
		CSocketManager.Instance.On("updateRoomStatus", this.ReceiveRoomList);
		CSocketManager.Instance.On("updateRoomSize", this.ReceiveRoomSize);
		CSocketManager.Instance.On("joinRoomCompleted", this.OnJoinRoomCompleted);
		CSocketManager.Instance.On("joinRoomFailed", this.OnJoinRoomFailed);
		// BUTTONS
		this.m_JoinRandomButton.onClick.AddListener(this.OnJoinRandomClick);
		this.m_RefreshButton.onClick.AddListener(this.OnRefreshClick);
		this.m_ExitButton.onClick.AddListener(this.OnExitClick);
		this.m_OpenChatButton.onClick.AddListener (this.OnOpenChatClick);
		this.m_SoundOnOffButton.onClick.AddListener (this.OnSoundOnOffClick);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// ROOMS
		this.RequestRoomList();
		// GOLD DISPLAY
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
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

	protected virtual void JoinRoom(string name)
	{
		var sendData = new JSONObject();
		sendData.AddField("roomName", name);
		CSocketManager.Instance.Emit("joinOrCreateRoom", sendData);
		CGameSetting.LAST_ROOM_NAME = name;
		this.SetEnableSelectRooms (false);
		CSoundManager.Instance.Play ("sfx_click");
	}

	protected virtual void RequestRoomList()
	{
		this.m_RoomLoaded = false;
		// EMIT
		CSocketManager.Instance.Emit("getRoomsStatus");
	}

	private void ReceiveRoomList (SocketIO.SocketIOEvent ev) 
	{
		if (GameObject.activeInHierarchy == false)
			return;
		this.m_RoomLoaded = true;
		var receiveRooms = ev.data.GetField("rooms").list;
		for (int i = 0; i < receiveRooms.Count; i++)
		{
			if (i >= this.m_Rooms.Count)
			{
				var roomInstantiate = GameObject.Instantiate(this.m_RoomItemPrefab);
				roomInstantiate.transform.SetParent(this.m_RoomContent.transform);
				roomInstantiate.transform.localPosition = Vector3.zero;
				roomInstantiate.transform.localRotation = Quaternion.identity;
				roomInstantiate.transform.localScale = Vector3.one;
				this.m_Rooms.Add (roomInstantiate);
			}
			var room = receiveRooms[i];
			var roomName = room.GetField("roomName").ToString().Replace("\"", "");
			var players = room.GetField("players").ToString().Replace("\"", "");
			var maxPlayers = room.GetField("maxPlayers").ToString().Replace("\"", "");
			var roomDisplay = string.Format("{0}: {1}/{2}", roomName, players, maxPlayers);
			this.m_Rooms[i].Setup(roomName, roomDisplay, this.JoinRoom);
			this.m_Rooms[i].SetEnable (true);
		}
	}

	private void ReceiveRoomSize (SocketIO.SocketIOEvent ev) 
	{
		var roomName = ev.data.GetField("roomName").ToString().Replace("\"", "");
		var players = ev.data.GetField("players").ToString().Replace("\"", "");
		var maxPlayers = ev.data.GetField("maxPlayers").ToString().Replace("\"", "");
		var roomDisplay = string.Format("{0}: {1}/{2}", roomName, players, maxPlayers);
		for (int i = 0; i < this.m_Rooms.Count; i++)
		{
			if (this.m_Rooms[i].roomName == roomName)
			{
				this.m_Rooms[i].Setup(roomName, roomDisplay, this.JoinRoom);
				this.m_Rooms[i].SetEnable (true);
				break;
			}
		}
	}

	private void OnJoinRandomClick()
	{
		this.SetEnableSelectRooms (false);
		// JOIN RANDOM
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show("JOIN ROOM", "Do you want joint random a room on list ??", "OK", () => {
			confirm.OnEscapeObject();
			var randomRoom = this.m_Rooms[Random.Range(0, this.m_Rooms.Count)];
			this.JoinRoom(randomRoom.roomName);
			this.SetEnableSelectRooms (false);
		}, "CANCEL", () => {
			confirm.OnEscapeObject();
			this.SetEnableSelectRooms (true);
		});
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnJoinRoomCompleted(SocketIO.SocketIOEvent ev)
	{
		CRootManager.Instance.ShowScene("MainGamePanel");
		this.SetEnableSelectRooms (false);
	}

	private void OnJoinRoomFailed(SocketIO.SocketIOEvent ev)
	{
		
	}

	private void OnRefreshClick()
	{
		// REFRESH
		this.RequestRoomList();
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.ShowAndCloseAfter("REFESH ROOM", "Please wait...", this.IsRoomLoaded);
		this.SetEnableSelectRooms (false);
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnExitClick()
	{
		// EXIT
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnOpenChatClick()
	{
		CRootManager.Instance.ShowScene("ChatRoomPanel");
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnSoundOnOffClick()
	{
		CGameSetting.SETTING_SOUND_MUTE = !CGameSetting.SETTING_SOUND_MUTE;
		this.m_SoundOffImage.SetActive (CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOnImage.SetActive (!CGameSetting.SETTING_SOUND_MUTE);
		CSoundManager.Instance.MuteAll (CGameSetting.SETTING_SOUND_MUTE);
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void SetEnableSelectRooms(bool value)
	{
		for (int i = 0; i < this.m_Rooms.Count; i++)
		{
			this.m_Rooms[i].SetEnable (value);
		}
	}

	#endregion

	#region Public

	public virtual bool IsRoomLoaded()
	{
		return this.m_RoomLoaded;
	}

	#endregion

}
