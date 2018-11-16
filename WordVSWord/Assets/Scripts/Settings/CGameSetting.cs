using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting {

	#region CONFIGS

	public static string URL = "ws://word-vs-word-online.herokuapp.com:80/socket.io/?EIO=4&transport=websocket";

	public static float ANIM_OBJECT = 0.3f;

	private static Dictionary<string, Sprite> CACHE_SPRITES = new Dictionary<string, Sprite>();
	public static Sprite GetSprite(string path)
	{
		if (CACHE_SPRITES.ContainsKey(path))
			return CACHE_SPRITES[path];
		var resoucesSprite = Resources.Load<Sprite>(path);
		CACHE_SPRITES.Add (path, resoucesSprite);
		return resoucesSprite;
	}

	public static Sprite GetAvatarSprite(int index)
	{
		var avatarName = string.Format ("Avatars/avatar-icon-{0}", index);
		if (CACHE_SPRITES.ContainsKey(avatarName))
			return CACHE_SPRITES[avatarName];
		var resoucesSprite = Resources.Load<Sprite>(avatarName);
		CACHE_SPRITES.Add (avatarName, resoucesSprite);
		return resoucesSprite;
	}

	public static float CHAT_DELAY = 3f;

	public static string GetDisplayWord(string word)
	{
		if (string.IsNullOrEmpty(word))
			return string.Empty;
		if (word.Length < 3)
			return word;
		return string.Format ("<color=#0074FF>{0}</color>{1}<color=#FF004F>{2}</color>", 
			word.Substring(0, 1).ToUpper(), 
			word.Substring(1, word.Length - 2).ToLower(), 
			word.Substring(word.Length - 1, 1).ToLower());
	}

	public static int GOLD_VIDEO_REWARD = 3;

	#endregion

	#region TIMER TO ADS

    public static string SAVE_TIMER_TO_AD = "TIMER_TO_AD";
	public static long DELAY_TO_AD = 45;

	public static bool HasTimeToAd()
	{
		return PlayerPrefs.HasKey(SAVE_TIMER_TO_AD);
	}

    public static bool IsTimerToAd(long delay)
    {
        var timeStr = PlayerPrefs.GetString(SAVE_TIMER_TO_AD, DateTime.Now.Ticks.ToString());
        var ticks = DateTime.Now.Ticks - long.Parse(timeStr);
        var elapsedSpan = new TimeSpan(ticks);
        return elapsedSpan.TotalSeconds >= delay;
    }

    public static void ResetTimerToAd()
    {
        PlayerPrefs.SetString(SAVE_TIMER_TO_AD, DateTime.Now.Ticks.ToString());
        PlayerPrefs.Save();
    }

    #endregion
	
	#region UPDATE DATA

	public static void UpdatePlayerData (string playerName, int avatarIndex, int frame)
	{
		// DATA
		var userData = new JSONObject();
		userData.AddField("playerAvatar", avatarIndex);
		userData.AddField("playerName", playerName);
		userData.AddField("playerFrame", frame);
		// EMIT
		CSocketManager.Instance.Emit("updatePlayerData", userData);
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

	private static string SAVE_USER_FRAME = "SAVE_USER_FRAME";
	public static int USER_FRAME 
	{
		get { return PlayerPrefs.GetInt(SAVE_USER_FRAME, 0); }
		set 
		{
			PlayerPrefs.SetInt(SAVE_USER_FRAME, value);
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

	private static string SAVE_SHOW_TUTORIAL = "SAVE_SHOW_TUTORIAL";
	public static bool SHOW_TUTORIAL 
	{
		get { return PlayerPrefs.GetInt(SAVE_SHOW_TUTORIAL, 1) == 1; }
		set 
		{
			PlayerPrefs.SetInt(SAVE_SHOW_TUTORIAL, value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	#endregion

	#region BACKGROUNDS

	public static string[] BACKGROUND_PATHS = new string[] {
		"Images/board_ground",
		"Images/board_grey",
		"Images/board_green",
		"Images/board_red",
		"Images/board_ground_1",
		"Images/board_ground_2",
		"Images/board_ground_3"	
	};

	#endregion

	#region SETTING

	public static string SOUND_MUTE = "SOUND_MUTE";

	public static bool SETTING_SOUND_MUTE
	{
		get
		{
			return PlayerPrefs.GetInt(SOUND_MUTE, 1) == 1;
		}
		set
		{
			PlayerPrefs.SetInt(SOUND_MUTE, value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	#endregion
	
}
