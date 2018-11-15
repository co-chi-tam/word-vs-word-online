using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CResultItem : MonoBehaviour {

	protected GameObject m_StarImage;
	protected Image m_AvatarImage;
	protected Text m_ScoreText;
	protected Text m_PlayerNameText;

	protected virtual void Awake()
	{
		this.Init();
	}

	public virtual void Init()
	{
		this.m_StarImage = this.transform.Find("StarImage").gameObject;
		this.m_AvatarImage = this.transform.Find("AvatarImageFrame/AvatarImage").GetComponent<Image>();
		this.m_ScoreText = this.transform.Find("ScoreText").GetComponent<Text>();
		this.m_PlayerNameText = this.transform.Find("PlayerNameText").GetComponent<Text>();
	}

	public virtual void Setup(int rank, int avatarIndex, string playerName, int score)
	{
		this.m_StarImage.SetActive(rank == 0);
		this.m_AvatarImage.sprite = CGameSetting.GetAvatarSprite(avatarIndex);
		this.m_PlayerNameText.text = playerName;
		this.m_ScoreText.text = string.Format ("+{0}", score);
	}
	
}
