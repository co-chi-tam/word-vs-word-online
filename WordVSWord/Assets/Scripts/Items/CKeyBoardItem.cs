using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CKeyBoardItem : MonoBehaviour {

	public string m_Char = "@@!";
	public string charContent
	{ 
		get { return this.m_Char; } 
		set { this.m_Char = value; }
	}

	protected Button m_Button;

	public virtual void Init()
	{
		var nameSplits = this.name.Split('_');
		this.m_Char = nameSplits[0];
		// BUTTON
		this.m_Button = this.GetComponent<Button>();
	}

	public virtual void Setup(System.Action<string> callback)
	{
		this.m_Button.onClick.RemoveAllListeners();
		this.m_Button.onClick.AddListener(() => {
			if (callback != null)
			{
				callback(this.m_Char);
			}
		});
	}
	
}
