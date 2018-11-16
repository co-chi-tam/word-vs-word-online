using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CTutorialPopup : CDefaultPopup {

	#region FIELDS

	protected Animator m_TutorialAnimator;
	protected Button m_TutorialButton;
	// protected Toggle m_DontShowAgainToggle;
	protected Button m_QuitButton;

	private int m_Step = 0;

	#endregion

	#region CONDTRUCTOR

	public CTutorialPopup(): base()
	{
		
	}

	public CTutorialPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_TutorialAnimator = CRootManager.FindObjectWith(GameObject, "Container").GetComponent<Animator>();
		this.m_TutorialButton = CRootManager.FindObjectWith(GameObject, "TutorialButton").GetComponent<Button>();
		this.m_TutorialButton.onClick.AddListener (this.OnTutorialStepClick);
		// this.m_DontShowAgainToggle = CRootManager.FindObjectWith(GameObject, "DontShowToggle").GetComponent<Toggle>();
		// this.m_DontShowAgainToggle.onValueChanged.AddListener (this.OnDontShowAgainToggle);
		this.m_QuitButton = CRootManager.FindObjectWith (GameObject, "QuitButton").GetComponent<Button>();
		this.m_QuitButton.onClick.AddListener (this.OnQuitClick);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		this.m_Step = 0;
		// this.m_DontShowAgainToggle.isOn = !CGameSetting.SHOW_TUTORIAL;
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		this.OnFinishTutorial();
	}

	public override void OnBackPress()
	{
		base.OnBackPress();
	}

	#endregion

	#region PUBLIC

	#endregion

	#region PRIVATE 

	private void OnFinishTutorial()
	{
		this.m_TutorialAnimator.SetTrigger("IsFinish");
		this.m_Step = 0;
		CGameSetting.SHOW_TUTORIAL = false;
	}

	private void OnTutorialStepClick()
	{
		if (this.m_Step == 0)
		{
			this.m_TutorialAnimator.SetTrigger("IsStep_1");
		}
		else if (this.m_Step == 1)
		{
			this.m_TutorialAnimator.SetTrigger("IsStep_2");
		}
		else if (this.m_Step == 2)
		{
			this.m_TutorialAnimator.SetTrigger("IsStep_3");
		}
		else if (this.m_Step == 3)
		{
			this.m_TutorialAnimator.SetTrigger("IsFinish");
			this.OnBackPress();
		}
		this.m_Step ++;
	}

	// private void OnDontShowAgainToggle(bool value)
	// {
	// 	CGameSetting.SHOW_TUTORIAL = !value;
	// }

	private void OnQuitClick()
	{
		CRootManager.Instance.Back();
		this.OnFinishTutorial();
		CSoundManager.Instance.Play ("sfx_click");
	}

	#endregion
	
}
