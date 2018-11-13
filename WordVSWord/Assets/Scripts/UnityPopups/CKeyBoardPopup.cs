using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CKeyBoardPopup : CDefaultPopup {

	#region FIELDS

	protected static string[] CHARS = new string [] {
		"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P"
		, "A", "S", "D", "F", "G", "H", "J", "K", "L"
		, "Z", "X", "C", "V", "B", "N", "M"
	};
	protected CKeyBoardItem[] m_Buttons;

	protected Button m_BackspaceButton;
	protected Button m_SpacebarButton;
	protected Button m_SubmitWordButton;
	protected Button m_QuitButton;

	protected Text m_InputFieldText;
	protected string m_InputString = "";
	protected int m_InputLength = 18;

	protected System.Action<string> OnSubmitString;

	#endregion

	#region CONDTRUCTOR

	public CKeyBoardPopup(): base()
	{
		
	}

	public CKeyBoardPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public override void OnInitObject()
	{
		base.OnInitObject();
		// KEYBOARD
		this.m_Buttons = GameObject.GetComponentsInChildren<CKeyBoardItem>();
		for (int i = 0; i < this.m_Buttons.Length; i++)
		{
			this.m_Buttons[i].Init();
			this.m_Buttons[i].Setup(this.OnKeyPressed);
		}
		// UI
		this.m_BackspaceButton = CRootManager.FindObjectWith (GameObject, "BackspaceButton").GetComponent<Button>();
		this.m_BackspaceButton.onClick.AddListener (this.OnBackspaceClick);
		
		this.m_SpacebarButton = CRootManager.FindObjectWith (GameObject, "SpacebarButton").GetComponent<Button>();
		this.m_SpacebarButton.onClick.AddListener (this.OnSpacebarClick);

		this.m_SubmitWordButton = CRootManager.FindObjectWith (GameObject, "SubmitWordButton").GetComponent<Button>();
		this.m_SubmitWordButton.onClick.AddListener (this.OnSubmitWordClick);
		
		this.m_QuitButton = CRootManager.FindObjectWith (GameObject, "QuitButton").GetComponent<Button>();
		this.m_QuitButton.onClick.AddListener (this.OnQuitClick);

		this.m_InputFieldText = CRootManager.FindObjectWith (GameObject, "InputFieldText").GetComponent<Text>();
		this.m_InputFieldText.text = string.Empty;
		this.m_InputString = string.Empty;
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// INPUT
		this.m_InputFieldText.text = string.Empty;
		this.m_InputString = string.Empty;
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	#endregion

	#region PUBLIC

	public virtual void Setup(System.Action<string> callback)
	{
		this.OnSubmitString = callback;
	}

	#endregion

	#region PRIVATE 

	private void OnKeyPressed(string value)
	{
		if (this.m_InputString.Length >= this.m_InputLength)
			return;
		this.m_InputString += value;
		this.m_InputFieldText.text = this.m_InputString;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnBackspaceClick()
	{
		if (this.m_InputString.Length == 0)
			return;
		this.m_InputString = this.m_InputString.Substring(0, this.m_InputString.Length - 1);
		this.m_InputFieldText.text = this.m_InputString;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnSpacebarClick()
	{
		if (this.m_InputString.Length >= this.m_InputLength)
			return;
		this.m_InputString += " ";
		this.m_InputFieldText.text = this.m_InputString;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnSubmitWordClick()
	{
		if (this.OnSubmitString != null)
		{
			this.OnSubmitString.Invoke(this.m_InputString);
		}
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnQuitClick()
	{
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}

	#endregion
	
}
