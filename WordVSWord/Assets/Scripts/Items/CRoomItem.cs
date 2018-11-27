using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CRoomItem : MonoBehaviour {

	protected string m_RoomName;
	public string roomName 
	{ 
		get { return this.m_RoomName; }
		set { this.m_RoomName = value; }
	}
	protected int m_RoomMembers;
	public int roomMembers 
	{ 
		get { return this.m_RoomMembers; }
		set { this.m_RoomMembers = value; }
	}
	protected Button m_EnterRoomButton;
	protected Text m_RoomNameText;

	protected virtual void Awake()
	{
		this.Init();
	}

	public virtual void Init()
	{
		this.m_EnterRoomButton = this.transform.Find("Container/EnterRoomButton").GetComponent<Button>();
		this.m_RoomNameText = this.transform.Find("Container/RoomNameText").GetComponent<Text>();
	}

	public virtual void UpdateRoom(string name, int members, string display)
	{
		// ROOM NAME
		this.m_RoomName = name;
		this.m_RoomMembers = members;
		this.m_RoomNameText.text = display;
	}

	public virtual void Setup(string name, int members, string display, System.Action<string> callback)
	{
		// UI
		this.UpdateRoom(name, members, display);
		// BUTTON
		this.m_EnterRoomButton.onClick.RemoveAllListeners();
		this.m_EnterRoomButton.onClick.AddListener(() => {
			if(callback != null)
				callback(this.m_RoomName);
		});
	}

	public virtual void SetEnable(bool value)
	{
		this.m_EnterRoomButton.interactable = value;
	}
	
}
