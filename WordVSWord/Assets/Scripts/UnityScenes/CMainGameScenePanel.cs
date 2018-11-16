using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMainGameScenePanel : CDefaultScene {

	#region Fields

	protected Button m_QuitButton;
	protected Animator m_ContainerAnimator;
	protected Text m_YourTurnText;

	protected Button m_ShowTutorialButton;
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
	protected Text m_WordInputText;
	protected string m_WordInputString;
	protected Button m_WordInputButton;
	protected Button m_SubmitButton;
	protected GameObject m_CurrentTurnGrayScreen;

	protected Button m_SuggestButton;
	protected Text m_GoldCostText;

	protected int m_CurrentGameTurn = -1;
	protected int m_CurrentTurnPlayer = -1;
	protected string m_CurrentPrefix = "a";
	protected WaitForSeconds m_WaitToShortTime = new WaitForSeconds(0.25f);

	protected string m_EMPTY_WORD = "Enter word here...";

	public static int CURRENT_GOLD_MATCH = 0;

	#endregion


	#region Constructor

	public CMainGameScenePanel() : base()
	{

	}

	public CMainGameScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
		// YOUR TURN PANEL
		this.m_ContainerAnimator = CRootManager.FindObjectWith(GameObject, "Container").GetComponent<Animator>();
		this.m_YourTurnText = CRootManager.FindObjectWith(GameObject, "YourTurnText").GetComponent<Text>();
		this.m_ShowTutorialButton = CRootManager.FindObjectWith(GameObject, "ShowTutorialButton").GetComponent<Button>();
		// GAME DISPLAY
		this.m_GoldDisplayText = CRootManager.FindObjectWith(GameObject, "GoldDisplayText").GetComponent<Text>();
		this.m_RoomTimerText = CRootManager.FindObjectWith (GameObject, "RoomTimerText").GetComponent<Text>();
		this.m_CurrentCharacterText = CRootManager.FindObjectWith(GameObject, "CurrentCharacterText").GetComponent<Text>();
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
		this.m_WordInputText = CRootManager.FindObjectWith(GameObject, "WordInputText").GetComponent<Text>();
		this.m_WordInputText.text = this.m_EMPTY_WORD;
		this.m_WordInputButton = CRootManager.FindObjectWith(GameObject, "WordInputButton").GetComponent<Button>();
		this.m_SubmitButton = CRootManager.FindObjectWith(GameObject, "SubmitButton").GetComponent<Button>();
		// PLAYER
		this.m_CurrentGameTurn = -1;
		this.m_CurrentTurnPlayer = -1;
		this.m_CurrentPrefix = "a";
		// EVENTS
		CSocketManager.Instance.On("msgError", this.ReceiveMessageError);
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
		CSocketManager.Instance.On("counterDownAnswer", this.OnCountDownAnswerTimer);
		CSocketManager.Instance.On("onePassTurn", this.OnOnePassTurn);
		// UI
		this.m_ShowTutorialButton.onClick.AddListener (this.OnShowTutorialClick);
		this.m_QuitButton.onClick.AddListener(this.OnQuitClick);
		// WORD SUBMIT
		this.m_SubmitButton.onClick.AddListener(this.OnSendWord);
		// SUGGEST
		this.m_SuggestButton.onClick.AddListener(this.OnRequestSuggestWord);
		// WordInputButton
		this.m_WordInputButton.onClick.AddListener (this.OnWordInputClick);
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		this.m_WordLists.Clear();
		this.m_WordLists.TrimExcess();
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
		CURRENT_GOLD_MATCH = 0;
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		// CACHE
		for (int i = 0; i < this.m_WordLists.Count; i++)
		{
			var item = this.m_WordLists[i];
			this.SetToCache(item);
		}
		// REMOVE LIST
		this.m_WordLists.Clear();
		this.m_WordLists.TrimExcess();
		// GOLD
		CURRENT_GOLD_MATCH = 0;
	}
	
	#endregion

	#region Private

	private void OnQuitClick()
	{
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show("QUIT ROOM", "Do you want quit room ?", "OK", () => {
			this.OnQuitRoom();
			confirm.OnBackPress();
		}, "CANCEL", () => {
			confirm.OnBackPress();
		});
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnAddGold (SocketIO.SocketIOEvent ev)
	{
		CURRENT_GOLD_MATCH += int.Parse (ev.data.GetField("gold").ToString());
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
	}

	private void OnDisplayLobby(SocketIO.SocketIOEvent ev)
	{
		var room = ev.data.GetField("roomInfo");
		var players = room.GetField("players").list;
		var display = string.Empty;
		for (int i = 0; i < players.Count; i++)
		{
			display += string.Format ("Player: {0} - READY\n", players[i].GetField("playerName"));
		}
		var lobby = CRootManager.Instance.ShowPopup("LobbyPopup") as CLobbyPopup;
		lobby.Show("PLEASE WAIT !!!", display, this.OnQuitClick);
		CSoundManager.Instance.Play ("sfx_new_turn");
	}

	private void OnQuitRoom()
	{
		CSocketManager.Instance.Emit("leaveRoom");
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnClearRoom(SocketIO.SocketIOEvent ev)
	{
		CRootManager.Instance.BackTo ("RoomDisplayPanel");
	}

	private void OnStartGame(SocketIO.SocketIOEvent ev)
	{
		// GAME TURN
		var firstTurn = int.Parse (ev.data.GetField("firstPlayerIndex").ToString());
		this.SetCurrenTurn (firstTurn);
		// PREFIX
		var currentPrefix = ev.data.GetField("firstCharacter").ToString().Replace("\"", string.Empty);
		this.SetCurrentCharacter(currentPrefix);
		// AVATAR
		var room = ev.data.GetField("roomInfo");
		var players = room.GetField("players").list;
		for (int i = 0; i < players.Count; i++)
		{
			var playerAvatar = int.Parse (players[i].GetField("playerAvatar").ToString());
			this.m_PlayerDisplayItems[i].Setup(playerAvatar);
		}
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		this.m_GoldCostText.text = string.Format ("-{0}", goldCost);
		CRootManager.Instance.BackTo ("MainGamePanel");
		CSoundManager.Instance.Play ("sfx_new_word");
	}

	private void OnReceiveNewWord(SocketIO.SocketIOEvent ev)
	{
		// ANSWER
		// var lastIndex = int.Parse (ev.data.GetField("lastIndex").ToString());
		// GAME TURN
		var currentTurn = int.Parse (ev.data.GetField("turnIndex").ToString());
		this.SetCurrenTurn(currentTurn);
		this.m_YourTurnText.text = "YOUR TURN";
		this.m_WordInputString = string.Empty;
		// CURRENT CHARACTER
		var currentPrefix = ev.data.GetField("nextCharacter").ToString().Replace("\"", string.Empty);
		this.SetCurrentCharacter(currentPrefix);
		// PLAYER 
		var playerAvatar = int.Parse (ev.data.GetField("playerAvatar").ToString());
		var frame = int.Parse (ev.data.GetField("playerFrame").ToString());
		// WORD LIST
		var displayWord = ev.data.GetField("displayWord").ToString().Replace("\"", string.Empty);
		var item = this.DisplayAWord (frame, playerAvatar, displayWord);
		this.m_WordLists.Add (item);
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		this.m_GoldCostText.text = string.Format ("-{0}", goldCost);
		// UI
		this.m_SuggestButton.interactable = true;
		// EVENTS
		CRootManager.Instance.StopCoroutine (this.HandleUpdateWordScrollRect());
		CRootManager.Instance.StartCoroutine (this.HandleUpdateWordScrollRect());
		CSoundManager.Instance.Play ("sfx_new_word");
	}

	private void OnReceiveSuggestWord(SocketIO.SocketIOEvent ev)
	{
		// WORD SUGGEST
		var wordSuggest = ev.data.GetField("wordSuggest").ToString().Replace("\"", string.Empty);
		this.m_WordInputText.text = CGameSetting.GetDisplayWord(wordSuggest);
		this.m_WordInputString = wordSuggest;
		CRootManager.Instance.BackTo ("MainGamePanel");
		// GOLD COST
		var goldCost = int.Parse (ev.data.GetField("goldCost").ToString());
		CGameSetting.USER_GOLD -= goldCost;
		// GOLD
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
		this.m_SuggestButton.interactable = true;
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

	private void OnCountDownAnswerTimer(SocketIO.SocketIOEvent ev)
	{
		var answerTimer = int.Parse (ev.data.GetField("answerTimer").ToString());
		var minute = answerTimer / 60;
		var second = answerTimer % 60;
		this.m_YourTurnText.text = string.Format("YOUR TURN\n{0}:{1}", minute.ToString("d2"), second.ToString("d2"));
	}

	private void OnOnePassTurn(SocketIO.SocketIOEvent ev)
	{	
		// GAME TURN
		var currentTurn = int.Parse (ev.data.GetField("turnIndex").ToString());
		this.SetCurrenTurn(currentTurn);
		// CURRENT CHARACTER
		var currentPrefix = ev.data.GetField("nextCharacter").ToString().Replace("\"", string.Empty);
		this.SetCurrentCharacter(currentPrefix);
	}

	private CWordItem DisplayAWord(int frame, int avatar, string displayWord)
	{
		var item = this.GetFromCache();
		item.transform.SetParent(this.m_ListWordContent.transform);
		item.transform.localPosition = Vector3.zero;
		item.transform.localRotation = Quaternion.identity;
		item.transform.localScale = Vector3.one;
		item.gameObject.SetActive(true);
		item.Setup (frame, avatar, displayWord);
		return item;
	}

	private void OnReceiveTurnIndex(SocketIO.SocketIOEvent ev)
	{
		// PLAYER
		this.m_CurrentTurnPlayer = int.Parse (ev.data.GetField("turnIndex").ToString());
	}

	private void OnSendWord()
	{
		if (this.m_CurrentGameTurn != this.m_CurrentTurnPlayer)
			return;
		var textAnswer = this.m_WordInputString;
		if (string.IsNullOrEmpty (textAnswer))
		{
			return;
		}
		this.SendWord(textAnswer, CGameSetting.USER_FRAME);
		this.m_WordInputString = string.Empty;
		this.m_WordInputText.text = this.m_EMPTY_WORD;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnRequestSuggestWord()
	{
		var wordData = new JSONObject();
		wordData.AddField ("prefix", this.m_CurrentPrefix);
		CSocketManager.Instance.Emit ("suggestWord", wordData);
		this.m_SuggestButton.interactable = false;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnWordInputClick()
	{
		var keyboard = CRootManager.Instance.ShowPopup("KeyBoardPopup") as CKeyBoardPopup;
		keyboard.Setup((value) => {
			this.m_WordInputString = value;
			this.m_WordInputText.text = CGameSetting.GetDisplayWord(value);
			keyboard.OnBackPress();
		});
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void SendWord(string value, int frame)
	{
		var wordData = new JSONObject();
		wordData.AddField ("turnIndex", this.m_CurrentTurnPlayer);
		wordData.AddField ("word", value);
		CSocketManager.Instance.Emit ("sendWord", wordData);
	}

	private void OnEndGame(SocketIO.SocketIOEvent ev)
	{
		// DISABLE YOUR TURN
		this.SetContainerAnimator ("IsYourTurn", false);
		// SHOW RESULT
		var results = ev.data.GetField("results").list;
		results.Sort((a, b) => {
			var intA = int.Parse (a.GetField("sum").ToString());
			var intB = int.Parse (b.GetField("sum").ToString());
			return intA > intB ? -1 : intA < intB ? 1 : 0;
		});
		var resultPopup = CRootManager.Instance.ShowPopup("MatchResultPopup") as CMatchResultPopup;
		resultPopup.Show(results, this.OnReceiveResults);
		CGameSetting.USER_GOLD += CURRENT_GOLD_MATCH; 
	}

	private void OnReceiveResults()
	{
		CRootManager.Instance.BackTo ("RoomDisplayPanel");
	}

	private void ReceiveMessageError(SocketIO.SocketIOEvent ev)
	{
		if (this.GameObject.activeInHierarchy == false)	
			return;
		// UI
		this.m_SubmitButton.interactable = true;
		CSocketManager.Instance.Invoke("ShowLastErrorPopup", 0f);
	}

	private void OnShowTutorialClick()
	{
		CRootManager.Instance.ShowPopup("TutorialPopup");
	}

	#endregion

	#region Public

	public void SetCurrenTurn(int currentTurn)
	{
		// GAME TURN
		this.m_CurrentGameTurn = currentTurn;
		this.SetContainerAnimator ("IsYourTurn", this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		this.m_CurrentTurnGrayScreen.SetActive (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer);
		if (this.m_CurrentGameTurn == this.m_CurrentTurnPlayer)
			CSoundManager.Instance.Play ("sfx_new_turn");
		for (int i = 0; i < this.m_PlayerDisplayItems.Length; i++)
		{
			this.m_PlayerDisplayItems[i].Active(i == currentTurn);
		}

	}

	public void SetCurrentCharacter(string prefix)
	{
		this.m_CurrentPrefix = prefix;
		this.m_CurrentCharacterText.text = prefix.ToUpper();
		this.SetContainerAnimator("IsChangeWord");
	}

	public void SetEnableControl(bool value)
	{
		this.m_SubmitButton.interactable = value;
		this.m_WordInputButton.interactable = value;
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

	#endregion

}
