using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerDisplayItem : MonoBehaviour {

	protected Image m_AvatarImage;
	protected Animator m_Animator;

	protected virtual void Awake()
	{
		this.Init();
	}

	public virtual void Init()
	{
		this.m_AvatarImage = this.transform.Find("Container/AvatarImage").GetComponent<Image>();
		this.m_Animator = this.GetComponent<Animator>();
	}

	public virtual void Setup(int avatarIndex)
	{
		this.m_AvatarImage.sprite = CGameSetting.GetAvatarSprite(avatarIndex);
	}

	public virtual void Active()
	{
		this.SetAnimator("IsActive");
	}

	public virtual void SetAnimator(string name, object value = null)
	{
		if (this.m_Animator == null)
			return;
		if (value is bool)
		{
			this.m_Animator.SetBool (name, (bool) value);
		}
		else if (value is int)
		{
			this.m_Animator.SetInteger (name, (int) value);
		}
		else if (value is float)
		{
			this.m_Animator.SetFloat (name, (int) value);
		}
		else if (value == null)
		{
			this.m_Animator.SetTrigger(name);
		}
	}
	
}
