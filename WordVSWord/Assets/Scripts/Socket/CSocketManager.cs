using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;
using SimpleSingleton;

[RequireComponent(typeof(SocketIOComponent))]
public class CSocketManager : CMonoSingleton<CSocketManager> {

	#region Fields
	// ws://word-vs-word-online.herokuapp.com/:80/socket.io/?EIO=4&transport=websocket
	[SerializeField]	protected SocketIOComponent m_Socket;
	public SocketIOComponent socket { 
		get { return this.m_Socket; } 
		set { this.m_Socket = value; }
	}

	// Delay 3 second
	protected WaitForFixedUpdate m_WaiForFixedUpdate = new WaitForFixedUpdate();
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);

	// EVENTS
	public Action OnConnect;
	public Action OnDisconnect;
	public Action OnError;
	public Action OnBeepBoop;

	protected List<string> m_RemainError = new List<string>();

	#endregion

	#region Implementation MonoBehaviour

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void OnApplicationQuit()
	{
		this.Disconnect();
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		this.m_Socket = this.GetComponent<SocketIOComponent>();
		// this.m_Socket.autoConnect = false;
		this.m_Socket.url = CGameSetting.URL;
		// CALLBACK
		socket.On("connect", ReceiveConnectMsg);
		socket.On("open", ReceiveOpenMsg);
		socket.On("boop", ReceiveBoop);
		socket.On("error", ReceiveErrorMsg);
		socket.On("msgError", ReceiveErrorMsg);
		socket.On("close", ReceiveCloseMsg);
		socket.On("disconnect", ReceiveCloseMsg);
		// LOOP PING
		StartCoroutine("BeepBoop");
	}

	public virtual void Connect(bool autoConnect = true) {
		if (this.m_Socket != null)
		{
			this.m_Socket.autoConnect = autoConnect;
			this.m_Socket.Connect();
			if (this.m_Socket.IsConnected)
			{
				if (this.OnConnect != null)
				{
					this.OnConnect();
				}
			}
		}
	}

	public virtual void Disconnect() {
		if (this.m_Socket != null 
			&& this.m_Socket.IsConnected) {
			this.m_Socket.Close();
		}
	}

	public virtual void On(string ev, Action<SocketIOEvent> callback)
	{
		if (this.m_Socket == null)
			return;
		this.m_Socket.On (ev, callback);
	}

	public virtual void Off(string ev, Action<SocketIOEvent> callback)
	{
		if (this.m_Socket == null)
			return;
		this.m_Socket.Off (ev, callback);
	}

	private IEnumerator BeepBoop()
	{
		while (true) {
			// wait 3 seconds and continue
			yield return this.m_DelaySeconds;
			if (this.m_Socket != null 
				&& this.m_Socket.autoConnect)
			{
				this.m_Socket.Connect();
				this.Emit("beep");
				// EVENT
				if (this.OnBeepBoop != null)
				{
					this.OnBeepBoop();
				}
			}
		}
	}

	#endregion

	#region Send

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev) {
		if (this.m_Socket != null && this.m_Socket.IsConnected) {
			this.m_Socket.Emit(ev);
		}
	}

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev, JSONObject data) {
		if (this.m_Socket != null && this.m_Socket.IsConnected) {
			this.m_Socket.Emit(ev, data);
		}
	}

	public virtual void EmitAfter(float timer, string ev, Action complete) {
		this.EmitAfter(timer, ev, null, complete);
	}

	public virtual void EmitAfter(float timer, string ev, JSONObject data, Action complete) {
		StopCoroutine(HandleEmitAfter(timer, ev, data, complete));
		StartCoroutine(HandleEmitAfter(timer, ev, data, complete));
	}

	protected IEnumerator HandleEmitAfter(float timer, string ev, JSONObject data, Action complete)
	{
		// TIME
		var countTimer = 0f;
		while(countTimer < timer)
		{
			yield return this.m_WaiForFixedUpdate;
			countTimer += Time.fixedDeltaTime;
		}
		// EMIT
		if (data == null)
			this.Emit(ev);
		else
			this.Emit(ev, data);
		// EVENT
		if (complete != null)
		{
			complete ();
		}
	}

	/// <summary>
	/// Send room message chat.
	/// </summary>
	public void SendMessageRoomChat(string msg = "Hey, i'm Norman.") {
		#if SOCKET_IO_DEBUG
		Debug.Log ("SendMessageRoomChat");
		#endif
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("message", msg);
		this.Emit("sendRoomChat", roomData);
	}

	/// <summary>
	/// Send World message chat.
	/// </summary>
	public void SendMessageWorldChat(string msg = "Hey, i'm Norman.") {
		#if SOCKET_IO_DEBUG
		Debug.Log ("SendMessageWorldChat");
		#endif
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("message", msg);
		this.Emit("sendWorldChat", roomData);
	}

	/// <summary>
	/// Emit leave room.
	/// </summary>
	public void LeaveRoom() {
		#if SOCKET_IO_DEBUG
		Debug.Log ("leaveRoom");
		#endif
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("leaveRoom");
	}

	#endregion

	#region Receive
	
	/// <summary>
	/// Receive open connect message.
	/// </summary>
	public void ReceiveConnectMsg(SocketIOEvent e)
	{
		#if SOCKET_IO_DEBUG
		Debug.Log("[SocketIO] Connect received: " + e.name + " " + e.data);
		#endif
		if (this.OnConnect != null)
		{
			this.OnConnect();
		}
	}

	/// <summary>
	/// Receive open connect message.
	/// </summary>
	public void ReceiveOpenMsg(SocketIOEvent e)
	{
		#if SOCKET_IO_DEBUG
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		#endif
	}

	/// <summary>
	/// Receive beep and boop message.
	/// Keep connect between client and server.
	/// </summary>
	public void ReceiveBoop(SocketIOEvent e) {
		#if SOCKET_IO_DEBUG
		Debug.Log("[SocketIO] Boop received: " + e.name + " " + e.data);
		#endif
	}
	
	/// <summary>
	/// Receive error.
	/// </summary>
	public void ReceiveErrorMsg(SocketIOEvent e)
	{
		if (e == null || e.data == null)
			return;
		#if SOCKET_IO_DEBUG
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
		#endif
		// REMAIN ERROR
		if (e.data.HasField("msg"))
		{
			this.m_RemainError.Add(e.data.GetField("msg").ToString());
		}
	}
	
	/// <summary>
	/// Receive close connect message.
	/// </summary>
	public void ReceiveCloseMsg(SocketIOEvent e)
	{	
		#if SOCKET_IO_DEBUG
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
		#endif
		if (this.OnDisconnect != null)
		{
			this.OnDisconnect();
		}
	}

	/// <summary>
	/// Receive Chat message.
	/// Emit from SendMessageRoomChat.
	/// </summary>
	public void ReceiveRoomChat (SocketIOEvent e) {
		#if SOCKET_IO_DEBUG
		Debug.Log ("[SOCKET IO] Room chat receive " + e.name + e.data);
		#endif
	}

	/// <summary>
	/// Receive World Chat message.
	/// Emit from SendMessageWorldChat.
	/// </summary>
	public void ReceiveWorldChat (SocketIOEvent e) {
		#if SOCKET_IO_DEBUG
		Debug.Log ("[SOCKET IO] World chat receive " + e.name + e.data);
		#endif
	}

	#endregion

	#region CALL BACK

	public void ShowLastErrorPopup()
	{
		if (this.m_RemainError.Count < 1)
			return;
		var lastMessage = this.m_RemainError[this.m_RemainError.Count - 1];
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show("ERROR", lastMessage, "OK", () => {
			confirm.OnBackPress();
		});
	}

	#endregion

	#region PUBLIC

	public bool IsConnected()
	{
		if (this.m_Socket == null)
			return false;
		return this.m_Socket.IsConnected;
	}

	#endregion

}
