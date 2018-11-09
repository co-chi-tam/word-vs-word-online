using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CWordItem : MonoBehaviour {

	[Header("Word")]
	[SerializeField]	protected string m_WordString;
	public string wordString 
	{ 
		get { return this.m_WordString; }
		set 
		{ 
			this.m_WordString = value;
			if (this.m_WordText != null)
				this.m_WordText.text = value;
		}
	}
	
	[Header("UI")]
	[SerializeField]	protected Image m_BackgroundImage;
	[SerializeField]	protected Image m_AvatarImage;
	[SerializeField]	protected Text m_WordText;

	protected Animator m_Animator;

	protected virtual void Awake()
	{
		this.Init();
	}

	public virtual void Init()
	{
		//UI
		this.m_BackgroundImage = this.transform.Find("Container").GetComponent<Image>();
		this.m_AvatarImage = this.transform.Find("Container/AvatarImage").GetComponent<Image>();
		this.m_WordText = this.transform.Find("Container/WordText").GetComponent<Text>();
		// ANIMATOR
		this.m_Animator = this.GetComponent<Animator>();
	}

	public virtual void Setup(int index, int avatar, string word)
	{
		// BG
		var bgPath = CGameSetting.BACKGROUND_PATHS[index % CGameSetting.BACKGROUND_PATHS.Length];
		this.m_BackgroundImage.sprite = Resources.Load<Sprite>(bgPath);
		// UI
		this.m_AvatarImage.sprite = CGameSetting.GetAvatarSprite(avatar);
		this.m_WordText.text = word;
		// ANIMATOR
		this.m_Animator.SetTrigger("IsShowUp");
	}

}
