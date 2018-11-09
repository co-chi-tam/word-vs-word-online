using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMainGameScenePanel : CDefaultScene {

	protected Button m_QuitButton;
	protected Animator m_ContainerAnimator;

	protected Text m_CurrentCharacterText;
	protected Text m_GoldDisplayText;
	protected Text m_RoomTimerText;

	protected CPlayerDisplayItem[] m_PlayerDisplayItems;
	protected CWordItem m_WordItemPrefab;
	protected GameObject m_ListWordContent;
	protected List<CWordItem> m_WordLists;
	protected Queue<CWordItem> m_WordCaches;
	protected Transform m_CacheRoot;
	protected ScrollRect m_ListWordScrollRect;
	protected InputField m_WordInputField;
	protected Button m_SubmitButton;
	protected GameObject m_CurrentTurnGrayScreen;

	protected Button m_SuggestButton;
	protected Text m_GoldCostText;

	protected int m_CurrentGameTurn = -1;
	protected int m_CurrentTurnPlayer = -1;
	protected string m_CurrentPrefix = "a";
	protected WaitForSeconds m_WaitToShortTime = new WaitForSeconds(0.25f);

	public CMainGameScenePanel() : base()
	{

	}

	public CMainGameScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
		// ANIMATOR
		this.m_ContainerAnimator = CRootManager.FindObjectWith(GameObject, "Container").GetComponent<Animator>();
		this.m_CurrentCharacterText = CRootManager.FindObjectWith(GameObject, "CurrentCharacterText").GetComponent<Text>();
		this.m_GoldDisplayText = CRootManager.FindObjectWith(GameObject, "GoldDisplayText").GetComponent<Text>();
		this.m_RoomTimerText = CRootManager.FindObjectWith (GameObject, "RoomTimerText").GetComponent<Text>();
		// PLAYER GROUP
		var playerGroup = CRootManager.FindObjectWith(GameObject, "PlayerGroup");
		this.m_PlayerDisplayItems = playerGroup.GetComponentsInChildren<CPlayerDisplayItem>();
		// WORD LIST
		this.m_ListWordContent = CRootManager.FindObjectWith(GameObject, "ListWordContent");
		this.m_WordItemPrefab = Resources.Load<CWordItem>("Items/WordItem");
		this.m_WordLists = new List<CWordItem>();
		this.m_ListWordScrollRect = CRootManager.FindObjectWith(GameObject, "ListWordScrollRect").GetComponent<ScrollRect>();
		this.m_CurrentTurnGrayScreen = CRootManager.FindObjectWith(GameObject, "CurrentTurnGrayScreen");
		// SUGGEST
		this.m_SuggestButton = CRootManager.FindObjectWith(GameObject, "SuggestButton").GetComponent<Button>();
		this.m_GoldCostText = CRootManager.FindObjectWith (GameObject, "GoldCostText").GetComponent<Text>();
		// CACHE
		this.m_WordCaches = new Queue<CWordItem>();
		this.m_CacheRoot = new GameObject("CACHE_ROOT").transform;
		// WORD
		this.m_WordInputField = CRootManager.FindObjectWith(GameObject, "WordInputField").GetComponent<InputField>();
		this.m_SubmitButton = CRootManager.FindObjectWith(GameObject, "SubmitButton").GetComponent<Button>();
		// PLAYER
		this.m_CurrentGameTurn = -1;
		this.m_CurrentTurnPlayer = -1;
		this.m_CurrentPrefix = "a";
		// EVENTS
		CSocketManager.Instance.On("addGold", this.OnAddGold);
		CSocketManager.Instance.On("newJoinRoom", this.OnDisplayLobby);
		CSocketManager.Instance.On("newLeaveRoom", this.OnDisplayLobby);
		CSocketManager.Instance.On("leaveRoom", this.OnClearRoom);
		CSocketManager.Instance.On("clearRoom", this.OnClearRoom);
		CSocketManager.Instance.On("turnIndexSet", this.OnReceiveTurnIndex);
		CSocketManager.Instance.On("startGame", this.OnStartGame);
		CSocketManager.Instance.On("endGameResult", this.OnEndGame);
		CSocketManager.Instance.On("allRoomGetWord", this.OnReceiveNewWord);
		CSocketManager.Instance.On("receiveSuggestWord", this.OnReceiveSuggestWord);
		CSocketManager.Instance.On("countDownTimer", this.OnCountDownTimer);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		this.m_WordLists.Clear();
		this.m_WordLists.TrimExcess();
		// UI
		this.m_QuitButton.onClick.AddListener(this.OnQuitClick);
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
		// WORD SUBMIT
		this.m_SubmitButton.onClick.AddListener(this.OnSendWord);
		// SUGGEST
		this.m_SuggestButton.onClick.AddListener(this.OnRequestSuggestWord);
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		// UI
		this.m_QuitButton.onClick.RemoveAllListeners();
		this.m_SubmitButton.onClick.RemoveAllListeners ();
		// CACHE
		for (int i = 0; i < this.m_WordLists.Count; i++)
		{
			var item = this.m_WordLists[i];
			this.SetToCache(item);
		}
	}

	private void OnQuitClick()
	{
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show("QUIT ROOM", "Do you want quit room ?", "OK", () => {
			// AFTER 1 minute.
			CSocketManager.Instance.EmitAfter(1f, "leaveRoom", () => {
				confirm.OnEscapeObject();
				this.OnQuitRoom();
			});
		}, "CANCEL", () => {
			confirm.OnEscapeObject();
		});
	}

	private void OnAddGold (SocketIO.SocketIOEvent ev)
	{
		 CGameSetting.USER_GOLD += int.Parse (ev.data.GetField("gold").ToString());
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
	}

	private void OnDisplayLobby(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnDisplayLobby " + ev.ToString());
		var room = ev.data.GetField("roomInfo");
		var players = room.GetField("players").list;
		var display = string.Empty;
		for (int i = 0; i < players.Count; i++)
		{
			display += string.Format ("Player: {0} - READY\n", players[i].GetField("playerName"));

		}
		var lobby = CRootManager.Instance.ShowPopup("LobbyPopup") as CLobbyPopup;
		lobby.Show("PLEASE WAIT !!!", display, this.OnQuitClick);
	}

	private void OnQuitRoom()
	{
		CSocketManager.Instance.Emit("leaveRoom");
		Debug.Log ("OnQuitRoom");
	}

	private void OnClearRoom(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnClearRoom " + ev.ToString());
		CRootManager.Instance.BackTo ("RoomDisplayPanel");
	}

	private void OnStartGame(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnStartGame " + ev.ToString());
		// GAME TURN
		this.m_CurrentGameTurn = int.Parse (ev.data.GetField("firstPlayerIndex").ToString());
		this.m_CurrentPrefix = ev.data.GetField("firstCharacter").ToString().Replace("\"", string.Empty);
		this.SetCurrentCharacter(this.m_CurrentPrefix);
		// AVATAR
		var room = ev.data.GetField("roomInfo");
		var players = room.GetField("players").list;
		for (int i = 0; i < players.Count; i++)
		{
			var playerAvatar = int.Parse (players[i].GetField("playerAvatar").ToString());
			this.m_PlayerDisplayItems[i].Setup(playerAvatar);

		}
		// TURN
		this.SetContainerAnimator ("IsYourTurn", this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		this.m_CurrentTurnGrayScreen.SetActive (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		this.SetEnableControl (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		this.m_GoldCostText.text = string.Format ("-{0}", goldCost);
		CRootManager.Instance.BackTo ("MainGamePanel");
	}

	private void OnReceiveNewWord(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnReceiveNewWord " + ev.ToString());
		// ANSWER
		var lastIndex = int.Parse (ev.data.GetField("lastIndex").ToString());
		this.m_PlayerDisplayItems[lastIndex].Active();
		// GAME TURN
		this.m_CurrentGameTurn = int.Parse (ev.data.GetField("turnIndex").ToString());
		this.SetContainerAnimator ("IsYourTurn", this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		this.m_CurrentTurnGrayScreen.SetActive (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		this.SetEnableControl (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		// CURRENT CHARACTER
		this.m_CurrentPrefix = ev.data.GetField("nextCharacter").ToString().Replace("\"", string.Empty);
		this.SetCurrentCharacter(this.m_CurrentPrefix);
		// PLAYER 
		var playerAvatar = int.Parse (ev.data.GetField("playerAvatar").ToString());
		// WORD LIST
		var displayWord = ev.data.GetField("displayWord").ToString().Replace("\"", string.Empty);
		var item = this.DisplayAWord (lastIndex, playerAvatar, displayWord);
		this.m_WordLists.Add (item);
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		this.m_GoldCostText.text = string.Format ("-{0}", goldCost);
		// UI
		this.m_SuggestButton.interactable = true;
		// EVENTS
		CRootManager.Instance.StopCoroutine (this.HandleUpdateWordScrollRect());
		CRootManager.Instance.StartCoroutine (this.HandleUpdateWordScrollRect());
	}

	private void OnReceiveSuggestWord(SocketIO.SocketIOEvent ev)
	{
		// WORD SUGGEST
		var wordSuggest = ev.data.GetField("wordSuggest").ToString().Replace("\"", string.Empty);
		this.m_WordInputField.text = wordSuggest;
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		CGameSetting.USER_GOLD -= goldCost;
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
	}
	
	protected IEnumerator HandleUpdateWordScrollRect()
	{
		yield return this.m_WaitToShortTime;
		this.m_ListWordScrollRect.verticalNormalizedPosition = 0;
	}

	private void OnCountDownTimer(SocketIO.SocketIOEvent ev)
	{
		var roomTimer = int.Parse (ev.data.GetField("roomTimer").ToString());
		var minute = roomTimer / 60;
		var second = roomTimer % 60;
		this.m_RoomTimerText.text = string.Format("{0}:{1}", minute.ToString("d2"), second.ToString("d2"));
	}

	private CWordItem DisplayAWord(int answerIndex, int avatar, string displayWord)
	{
		var item = this.GetFromCache();
		item.transform.SetParent(this.m_ListWordContent.transform);
		item.transform.localPosition = Vector3.zero;
		item.transform.localRotation = Quaternion.identity;
		item.transform.localScale = Vector3.one;
		item.Setup (answerIndex, avatar, displayWord);
		item.gameObject.SetActive(true);
		return item;
	}

	private void OnReceiveTurnIndex(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnReceiveTurnIndex " + ev.ToString());
		// PLAYER
		this.m_CurrentTurnPlayer = int.Parse (ev.data.GetField("turnIndex").ToString());
	}

	private void OnSendWord()
	{
		if (this.m_CurrentGameTurn != this.m_CurrentTurnPlayer)
			return;
		var textAnswer = this.m_WordInputField.text;
		if (string.IsNullOrEmpty (textAnswer))
		{
			return;
		}
		this.SendWord(textAnswer);
		this.m_WordInputField.text = string.Empty;
	}

	private void OnRequestSuggestWord()
	{
		var wordData = new JSONObject();
		wordData.AddField ("prefix", this.m_CurrentPrefix);
		CSocketManager.Instance.Emit ("suggestWord", wordData);
		this.m_SuggestButton.interactable = false;
	}

	private void SendWord(string value)
	{
		var wordData = new JSONObject();
		wordData.AddField ("turnIndex", this.m_CurrentTurnPlayer);
		wordData.AddField ("word", value);
		CSocketManager.Instance.Emit ("sendWord", wordData);
	}

	private void OnEndGame(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnEndGame " + ev.ToString());
		var results = ev.data.GetField("results").list;
		var display = string.Empty;
		for (int i = 0; i < results.Count; i++)
		{
			display += string.Format ("{0} -> {1}\n", results[i].GetField("playerName"), results[i].GetField("sum"));
		}
		var lobby = CRootManager.Instance.ShowPopup("LobbyPopup") as CLobbyPopup;
		lobby.Show("RESULTS !!!", display, this.OnReceveiResults);
	}

	private void OnReceveiResults()
	{
		CRootManager.Instance.BackTo ("RoomDisplayPanel");
	}

	public void SetCurrentCharacter(string prefix)
	{
		this.m_CurrentCharacterText.text = prefix.ToUpper();
		this.SetContainerAnimator("IsChangeWord");
	}

	public void SetEnableControl(bool value)
	{
		this.m_SubmitButton.interactable = value;
		this.m_WordInputField.interactable = value;
		this.m_SuggestButton.interactable = value;
	}

	public void SetToCache(CWordItem item)
	{
		item.transform.SetParent(this.m_CacheRoot);
		item.transform.localPosition = Vector3.zero;
		item.transform.localRotation = Quaternion.identity;
		item.transform.localScale = Vector3.one;
		item.gameObject.SetActive(false);
		this.m_WordCaches.Enqueue(item);
	}

	public CWordItem GetFromCache()
	{
		if (this.m_WordCaches.Count > 0)
		{
			return this.m_WordCaches.Dequeue();
		}
		var item = GameObject.Instantiate (this.m_WordItemPrefab);
		return item;
	}

	public virtual void SetContainerAnimator(string name, object value = null)
	{
		if (this.m_ContainerAnimator == null)
			return;
		if (value is bool)
		{
			this.m_ContainerAnimator.SetBool (name, (bool) value);
		}
		else if (value is int)
		{
			this.m_ContainerAnimator.SetInteger (name, (int) value);
		}
		else if (value is float)
		{
			this.m_ContainerAnimator.SetFloat (name, (int) value);
		}
		else if (value == null)
		{
			this.m_ContainerAnimator.SetTrigger(name);
		}
	}

}
