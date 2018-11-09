using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CRoomDisplayScenePanel : CDefaultScene {

	protected GameObject m_RoomContent;
	protected CRoomItem m_RoomItemPrefab;
	protected CRoomItem[] m_Rooms;

	protected Button m_JoinRandomButton;
	protected Button m_RefreshButton;
	protected Button m_ExitButton;

	protected Text m_GoldDisplayText;

	protected bool m_RoomLoaded = false;

	public CRoomDisplayScenePanel() : base()
	{

	}

	public CRoomDisplayScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	public override void OnInitObject()
	{
		base.OnInitObject();
		// ROOM
		this.m_Rooms = new CRoomItem[10];
		this.m_RoomContent = CRootManager.FindObjectWith(GameObject, "RoomContent");
		this.m_RoomItemPrefab = Resources.Load<CRoomItem>("Items/RoomItem");
		this.m_RoomLoaded = false;
		// BUTTONS
		this.m_JoinRandomButton = CRootManager.FindObjectWith(GameObject, "JoinRandomButton").GetComponent<Button>();
		this.m_RefreshButton = CRootManager.FindObjectWith(GameObject, "RefreshButton").GetComponent<Button>();
		this.m_ExitButton = CRootManager.FindObjectWith(GameObject, "ExitButton").GetComponent<Button>();
		// GOLD
		this.m_GoldDisplayText = CRootManager.FindObjectWith (GameObject, "GoldDisplayText").GetComponent<Text>();
		// OFF EVENTS
		// CSocketManager.Instance.Off("updateRoomStatus", this.ReceiveRoomList);
		// CSocketManager.Instance.Off("joinRoomFailed", this.OnJoinRoomFailed);
		// ON EVENTS
		CSocketManager.Instance.On("updateRoomStatus", this.ReceiveRoomList);
		CSocketManager.Instance.On("joinRoomCompleted", this.OnJoinRoomCompleted);
		CSocketManager.Instance.On("joinRoomFailed", this.OnJoinRoomFailed);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// ROOMS
		this.RequestRoomList();
		// BUTTONS
		this.m_JoinRandomButton.onClick.AddListener(this.OnJoinRandomClick);
		this.m_RefreshButton.onClick.AddListener(this.OnRefreshClick);
		this.m_ExitButton.onClick.AddListener(this.OnExitClick);
		// GOLD DISPLAY
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		// BUTTONS
		this.m_JoinRandomButton.onClick.RemoveAllListeners();
		this.m_RefreshButton.onClick.RemoveAllListeners();
		this.m_ExitButton.onClick.RemoveAllListeners();
	}

	protected virtual void JoinRoom(string name)
	{
		Debug.Log ("Join room" + name);
		var sendData = new JSONObject();
		sendData.AddField("roomName", name);
		CSocketManager.Instance.Emit("joinOrCreateRoom", sendData);
		CGameSetting.LAST_ROOM_NAME = name;
	}

	protected virtual void RequestRoomList()
	{
		this.m_RoomLoaded = false;
		// EMIT
		CSocketManager.Instance.Emit("getRoomsStatus");
		Debug.Log ("RequestRoomList");
	}

	private void ReceiveRoomList (SocketIO.SocketIOEvent ev) 
	{
		Debug.Log ("ReceiveRoomList " + ev.ToString());
		this.m_RoomLoaded = true;
		var receiveRooms = ev.data.GetField("rooms").list;
		if (this.m_Rooms.Length != receiveRooms.Count)
		{
			this.m_Rooms = new CRoomItem[receiveRooms.Count];
		}
		for (int i = 0; i < receiveRooms.Count; i++)
		{
			if (this.m_Rooms[i] == null)
			{
				this.m_Rooms[i] = GameObject.Instantiate(this.m_RoomItemPrefab);
				this.m_Rooms[i].transform.SetParent(this.m_RoomContent.transform);
				this.m_Rooms[i].transform.localPosition = Vector3.zero;
				this.m_Rooms[i].transform.localRotation = Quaternion.identity;
				this.m_Rooms[i].transform.localScale = Vector3.one;
			}
			var tmpRoom = receiveRooms[i];
			var tmpRoomName = tmpRoom.GetField("roomName").ToString().Replace("\"", "");
			var tmpRoomDisplay = tmpRoom.GetField("roomDisplay").ToString().Replace("\"", "");
			var tmpRoomPlayers = tmpRoom.GetField("players").ToString().Replace("\"", "");
			this.m_Rooms[i].Setup(tmpRoomName, tmpRoomDisplay, this.JoinRoom);
		}
	}

	private void OnJoinRandomClick()
	{
		// JOIN RANDOM
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show("JOIN ROOM", "Do you want joint random a room on list ??", "OK", () => {
			confirm.OnEscapeObject();
			var randomRoom = this.m_Rooms[Random.Range(0, this.m_Rooms.Length)];
			this.JoinRoom(randomRoom.roomName);
		}, "CANCEL", () => {
			confirm.OnEscapeObject();
		});
	}

	private void OnJoinRoomCompleted(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnJoinRoomCompleted " + ev.ToString());
		CRootManager.Instance.ShowScene("MainGamePanel");
	}

	private void OnJoinRoomFailed(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnJoinRoomFail " + ev.ToString());
	}

	private void OnRefreshClick()
	{
		// REFRESH
		this.RequestRoomList();
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.ShowAndCloseAfter("REFESH ROOM", "Please wait...", this.IsRoomLoaded);
	}

	private void OnExitClick()
	{
		// EXIT
		CRootManager.Instance.Back();
	}

	public virtual bool IsRoomLoaded()
	{
		return this.m_RoomLoaded;
	}

}
