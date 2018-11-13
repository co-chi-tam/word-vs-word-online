using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMatchResultPopup : CDefaultPopup {

	#region Fields

	protected GameObject m_BodyPanel;
	protected Button m_QuitButton;

	protected CResultItem m_ResultItemPrefab;
	protected CResultItem[] m_ResultItems;

	protected Animator m_Animator;
	
	#endregion 

	#region CONDTRUCTOR

	public CMatchResultPopup(): base()
	{
		
	}

	public CMatchResultPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion 

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// BODY
		this.m_BodyPanel = CRootManager.FindObjectWith(GameObject, "BodyPanel");
		// QUIT
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
		// RESULTS
		this.m_ResultItemPrefab = Resources.Load<CResultItem>("Items/ResultItem");
		this.m_ResultItems = new CResultItem[4];
		// ANIMATOR
		this.m_Animator = CRootManager.FindObjectWith(GameObject, "Container").GetComponent<Animator>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	#endregion 

	#region Public

	public virtual void Show(List<JSONObject> results, System.Action quitSubmit)
	{
		// RESULTS
		if (this.m_ResultItems.Length != results.Count)
		{
			this.m_ResultItems = new CResultItem[results.Count];
		}
		for (int i = 0; i < this.m_ResultItems.Length; i++)
		{
			if (this.m_ResultItems[i] == null)
			{
				this.m_ResultItems[i] = GameObject.Instantiate(this.m_ResultItemPrefab);
				this.m_ResultItems[i].transform.SetParent(this.m_BodyPanel.transform);
				this.m_ResultItems[i].transform.localPosition = Vector3.zero;
				this.m_ResultItems[i].transform.localRotation = Quaternion.identity;
				this.m_ResultItems[i].transform.localScale = Vector3.one;
			}
			var player = results[i].GetField("player");
			var avatar = int.Parse (player.GetField("playerAvatar").ToString());
			var playerName = player.GetField("playerName").ToString().Replace("\"", string.Empty);
			var score = int.Parse (results[i].GetField("sum").ToString());
			this.m_ResultItems[i].Setup(avatar, playerName, score);
		}
		// ANIMATOR
		this.SetAnimator ("IsShow");
		// BUTTON
		this.m_QuitButton.onClick.RemoveAllListeners();
		this.m_QuitButton.onClick.AddListener(() => { 
			if (quitSubmit != null)
			{
				quitSubmit();
			}
			CSoundManager.Instance.Play ("sfx_click");
		});
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

	#endregion 

}
