using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting {

	#region CONFIGS

	public static string URL = "ws://word-vs-word-online.herokuapp.com:80/socket.io/?EIO=4&transport=websocket";

	public static float ANIM_OBJECT = 0.3f;

	public static Sprite GetAvatarSprite(int index)
	{
		var avatarName = string.Format ("Avatars/avatar-icon-{0}", index);
		return Resources.Load<Sprite>(avatarName);
	}

	#endregion

	#region SAVE && LOAD

	private static string SAVE_USER_AVATAR = "SAVE_USER_AVATAR";

	public static int USER_AVATAR 
	{
		get { return PlayerPrefs.GetInt(SAVE_USER_AVATAR, 0); }
		set 
		{
			PlayerPrefs.SetInt(SAVE_USER_AVATAR, value);
			PlayerPrefs.Save();
		}
	}

	private static string SAVE_USER_NAME = "SAVE_USER_NAME";
	public static string USER_NAME 
	{
		get { return PlayerPrefs.GetString(SAVE_USER_NAME, string.Empty); }
		set 
		{
			PlayerPrefs.SetString(SAVE_USER_NAME, value);
			PlayerPrefs.Save();
		}
	}
	
	public static string LAST_ROOM_NAME = "";

	private static string SAVE_USER_GOLD = "SAVE_USER_GOLD";
	public static int USER_GOLD 
	{
		get { return PlayerPrefs.GetInt(SAVE_USER_GOLD, 100); }
		set 
		{
			PlayerPrefs.SetInt(SAVE_USER_GOLD, value);
			PlayerPrefs.Save();
		}
	}

	#endregion

	public static string[] BACKGROUND_PATHS = new string[] {
		"Images/board_ground",
		//"Images/board_green",
		"Images/board_grey",
		//"Images/board_red"	
	};
	
}
