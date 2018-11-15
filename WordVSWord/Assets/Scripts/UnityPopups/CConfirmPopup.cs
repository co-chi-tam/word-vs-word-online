using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CConfirmPopup : CDefaultPopup {

	#region FIELDS

	protected Text m_TitleText;
	protected Text m_BodyText;

	protected Button m_LeftButton;
	protected Text m_LeftText;
	protected Button m_MiddleButton;
	protected Text m_MiddleText;
	protected Button m_RightButton;
	protected Text m_RightText;

	protected WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

	#endregion

	#region CONDTRUCTOR

	public CConfirmPopup(): base()
	{
		
	}

	public CConfirmPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public override void OnInitObject()
	{
		base.OnInitObject();
		// TITLE
		this.m_TitleText = CRootManager.FindObjectWith(GameObject, "TitleText").GetComponent<Text>();
		// BODY
		this.m_BodyText = CRootManager.FindObjectWith(GameObject, "BodyText").GetComponent<Text>();
		// LEFT
		this.m_LeftButton = CRootManager.FindObjectWith(GameObject, "LeftButton").GetComponent<Button>();
		this.m_LeftText = CRootManager.FindObjectWith(GameObject, "LeftText").GetComponent<Text>();
		// MIDDLE
		this.m_MiddleButton = CRootManager.FindObjectWith(GameObject, "MiddleButton").GetComponent<Button>();
		this.m_MiddleText = CRootManager.FindObjectWith(GameObject, "MiddleText").GetComponent<Text>();
		// RIGHT
		this.m_RightButton = CRootManager.FindObjectWith(GameObject, "RightButton").GetComponent<Button>();
		this.m_RightText = CRootManager.FindObjectWith(GameObject, "RightText").GetComponent<Text>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		// UI
		this.ResetButtons();
	}

	#endregion

	#region PUBLIC

	public virtual void ShowAndCloseAfter(string title, string body, System.Func<bool> closeAfter)
	{
		this.Show(title, body);
		CRootManager.Instance.StopCoroutine(this.HandleCloseWhen(closeAfter));
		CRootManager.Instance.StartCoroutine(this.HandleCloseWhen(closeAfter));
	}

	protected virtual IEnumerator HandleCloseWhen(System.Func<bool> callback)
	{
		while(true)
		{
			yield return this.m_WaitForFixedUpdate;
			if (callback != null)
			{
				if (callback())
				{
					break;
				}
			}
			else
			{
				break;
			}
		}
		// CLOSE
		this.OnBackPress();
	}

	public virtual void Show(string title, string body)
	{
		// REMOVE ALL EVENTS
		this.ResetButtons();
		this.SetInteractableButtons(true);
		// TITLE
		this.m_TitleText.text = title;
		// BODY
		this.m_BodyText.text = body;
		// UI
		this.m_LeftButton.gameObject.SetActive(false);
		this.m_MiddleButton.gameObject.SetActive(false);
		this.m_RightButton.gameObject.SetActive(false);
	}

	public virtual void Show(string title, string body, string buttonLeft, System.Action leftSubmit)
	{
		// TITLE && BODY
		this.Show(title, body);
		// UI
		this.m_LeftButton.gameObject.SetActive(true);
		this.m_MiddleButton.gameObject.SetActive(false);
		this.m_RightButton.gameObject.SetActive(false);
		// TEXT
		this.m_LeftText.text = buttonLeft;
		// BUTTONS
		this.m_LeftButton.onClick.AddListener(() => { 
			if (leftSubmit != null)
			{
				leftSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
	} 

	public virtual void Show(
		string title, string body,
		string buttonLeft, System.Action leftSubmit,
		string buttonRight, System.Action rightSubmit)
	{
		// TITLE && BODY
		this.Show(title, body);
		// UI
		this.m_LeftButton.gameObject.SetActive(true);
		this.m_MiddleButton.gameObject.SetActive(false);
		this.m_RightButton.gameObject.SetActive(true);
		// TEXT
		this.m_LeftText.text = buttonLeft;
		this.m_RightText.text = buttonRight;
		// BUTTON
		this.m_LeftButton.onClick.AddListener(() => { 
			if (leftSubmit != null)
			{
				leftSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
		this.m_RightButton.onClick.AddListener(() => { 
			if (rightSubmit != null)
			{
				rightSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
	} 

	public virtual void Show(
		string title, string body,
		string buttonLeft, System.Action leftSubmit,
		string buttonMiddle, System.Action middleSubmit,
		string buttonRight, System.Action rightSubmit)
	{
		// TITLE && BODY
		this.Show(title, body);
		// UI
		this.m_LeftButton.gameObject.SetActive(true);
		this.m_MiddleButton.gameObject.SetActive(true);
		this.m_RightButton.gameObject.SetActive(true);
		// TEXT
		this.m_LeftText.text = buttonLeft;
		this.m_MiddleText.text = buttonMiddle;
		this.m_RightText.text = buttonRight;
		// BUTTON
		this.m_LeftButton.onClick.AddListener(() => { 
			if (leftSubmit != null)
			{
				leftSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
		this.m_MiddleButton.onClick.AddListener(() => { 
			if (middleSubmit != null)
			{
				middleSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
		this.m_RightButton.onClick.AddListener(() => { 
			if (rightSubmit != null)
			{
				rightSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
			this.SetInteractableButtons(false);
		});
	} 

	#endregion

	#region PRIVATE 

	private void ResetButtons()
	{
		// BUTTON
		this.m_LeftButton.onClick.RemoveAllListeners();
		this.m_MiddleButton.onClick.RemoveAllListeners();
		this.m_RightButton.onClick.RemoveAllListeners();
	}

	private void SetInteractableButtons(bool value)
	{
		// BUTTON
		this.m_LeftButton.interactable = value;
		this.m_MiddleButton.interactable = value;
		this.m_RightButton.interactable = value;
	}

	#endregion
	
}
