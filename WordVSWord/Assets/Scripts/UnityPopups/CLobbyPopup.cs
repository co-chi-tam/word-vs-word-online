using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CLobbyPopup : CDefaultPopup {

	#region FIELDS

	protected Text m_TitleText;
	protected Text m_BodyText;

	protected Button m_QuitButton;

	protected WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

	#endregion

	#region CONDTRUCTOR

	public CLobbyPopup(): base()
	{
		
	}

	public CLobbyPopup(string popName, string popObject): base(popName, popObject)
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
		// QUIT
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		this.m_QuitButton.onClick.RemoveAllListeners();
	}

	public override void OnBackPress()
	{
		// base.OnEscapeObject();
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
		// TITLE
		this.m_TitleText.text = title;
		// BODY
		this.m_BodyText.text = body;
	}

	public virtual void Show(string title, string body, System.Action quitSubmit)
	{
		Debug.Log (body);
		// TITLE && BODY
		this.Show(title, body);
		// BUTTONS
		this.m_QuitButton.onClick.RemoveAllListeners();
		this.m_QuitButton.onClick.AddListener(() => { 
			if (quitSubmit != null)
			{
				quitSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
		});
	} 

	#endregion

	#region PRIVATE 

	#endregion
	
}
