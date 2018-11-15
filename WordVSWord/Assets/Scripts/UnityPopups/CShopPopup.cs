using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TinyJSON;

public class CShopPopup : CDefaultPopup {

	#region FIELDS

	protected GameObject m_ShopContent;
	protected Button m_QuitButton;
	protected Button m_AdmobButton;
	protected Text m_AdmobRewardText;

	protected CShopItem m_ShopItemPrefab;
	protected TextAsset m_ShopConfig;
	protected List<CShopItem> m_OpenShop;

	protected CShop m_Shop;

	public class CShop {
		public CShopItem.CShopData[] shopItems;

		public CShop()
		{
			this.shopItems = null;
		}
	}

	public System.Action OnUpdateValue;

	#endregion

	#region CONDTRUCTOR

	public CShopPopup(): base()
	{
		
	}

	public CShopPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion

	#region IMPLEMENTATION OBJECT SCENE

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI
		this.m_ShopContent = CRootManager.FindObjectWith (GameObject, "ShopContent");
		// BUTTONS
		this.m_QuitButton = CRootManager.FindObjectWith (GameObject, "QuitButton").GetComponent<Button>();
		this.m_QuitButton.onClick.AddListener (this.OnQuitClick);
		this.m_AdmobButton = CRootManager.FindObjectWith (GameObject, "AdmobButton").GetComponent<Button>();
		this.m_AdmobButton.onClick.AddListener (this.OnAdmobClick);
		this.m_AdmobRewardText = CRootManager.FindObjectWith (GameObject, "AdmobRewardText").GetComponent<Text>();
		this.m_AdmobRewardText.text = string.Format ("+{0}", CGameSetting.GOLD_VIDEO_REWARD);
		// ITEM PREFAB
		this.m_ShopItemPrefab = Resources.Load<CShopItem>("Items/ShopItem");
		this.m_ShopConfig = Resources.Load<TextAsset>("Shop/shop");
		this.m_OpenShop = new List<CShopItem>();
		this.LoadConfig(this.m_ShopConfig.text);
		//Admob
		CAdmobManager.LoadRewardedVideo();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// UPDATE SHOP
		this.OnUpdateShop();
		// ADMOB
		this.m_AdmobButton.interactable = CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD);
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		this.OnUpdateValue = null;
	}

	#endregion

	#region PUBLIC

	public void GetUpdateData(System.Action callback)
	{
		this.OnUpdateValue = callback;
	}

	#endregion

	#region PRIVATE 

	private void LoadConfig(string config)
	{
		this.m_Shop = JSON.Load(config).Make<CShop>();
		var shopItems = this.m_Shop.shopItems;
		for (int i = 0; i < shopItems.Length; i++)
		{
			if (i >= this.m_OpenShop.Count)
			{
				var roomInstantiate = GameObject.Instantiate(this.m_ShopItemPrefab);
				roomInstantiate.transform.SetParent(this.m_ShopContent.transform);
				roomInstantiate.transform.localPosition = Vector3.zero;
				roomInstantiate.transform.localRotation = Quaternion.identity;
				roomInstantiate.transform.localScale = Vector3.one;
				this.m_OpenShop.Add (roomInstantiate);
			}
			var item = shopItems[i];
			var sprite = CGameSetting.GetSprite(item.displaySprite);
			if (item.type == CShopItem.EShopType.FRAME)
			{
				this.m_OpenShop[i].Setup(i, sprite, item.value == CGameSetting.USER_FRAME, item.price, this.OnItemClick);
			}
			else if (item.type == CShopItem.EShopType.AVATAR)
			{
				this.m_OpenShop[i].Setup(i, sprite, item.value == CGameSetting.USER_AVATAR, item.price, this.OnItemClick);
			}
			else
			{
				this.m_OpenShop[i].Setup(i, sprite, item.isUsing, item.price, this.OnItemClick);
			}
		}
	}

	private void OnItemClick(int value)
	{
		// UPDATE SHOP
		this.OnUpdateShop();
		// DATA
		var shopItem = this.m_Shop.shopItems[value];
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		if (CGameSetting.USER_GOLD >= shopItem.price)
		{
			confirm.Show("CONFIRM", "Do you want buy item ?", "OK", () => {
				// UPDATE
				if (shopItem.type == CShopItem.EShopType.FRAME)
				{
					CGameSetting.USER_FRAME = shopItem.value;
				}
				else if (shopItem.type == CShopItem.EShopType.AVATAR)
				{
					CGameSetting.USER_AVATAR = shopItem.value;
				}
				// UPDATE GOLD BUY ITEM
				CGameSetting.USER_GOLD -= shopItem.price;
				// SUBMIt
				this.OnSubmitData();
				// UPDATE ALL ITEMS
				this.m_OpenShop[value].UpdateItem(true);
				// EVENT
				if (this.OnUpdateValue != null)
				{
					this.OnUpdateValue();
				}
				confirm.OnBackPress();
			}, "CANCEL", () => {
				confirm.OnBackPress();
			});
		}
		else
		{
			confirm.Show("CONFIRM", "You not enough gold.", "OK", () => {
				confirm.OnBackPress();
			}, 
			"Ad", () => {
				// ADMOB
				this.OnAdmobClick();
				confirm.OnBackPress();
			});
		}
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnUpdateShop()
	{
		var shopItems = this.m_Shop.shopItems;
		for (int i = 0; i < shopItems.Length; i++)
		{
			var item = shopItems[i];
			if (item.type == CShopItem.EShopType.FRAME)
			{
				this.m_OpenShop[i].UpdateItem(item.value == CGameSetting.USER_FRAME);
			}
			else if (item.type == CShopItem.EShopType.AVATAR)
			{
				this.m_OpenShop[i].UpdateItem(item.value == CGameSetting.USER_AVATAR);
			}
			else
			{
				this.m_OpenShop[i].UpdateItem(false);
			}
		}
	}

	private void OnSubmitData()
	{
		// SET PLAYER NAME
		if (CSetingUserScenePanel.USER_AUTHORIZE_READY)
		{
			CGameSetting.UpdatePlayerData(CGameSetting.USER_NAME, CGameSetting.USER_AVATAR, CGameSetting.USER_FRAME);
		}
	}

	private void OnAdmobClick()
	{
		CAdmobManager.OnVideoAdsReward -= this.OnHandleRewardAddGold;
		if (CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD))
		{
			CAdmobManager.OnVideoAdsReward += this.OnHandleRewardAddGold;
			CAdmobManager.ShowRewardedVideo();
			CGameSetting.ResetTimerToAd();
			this.m_AdmobButton.interactable = false;
		}
	}

	private void OnHandleRewardAddGold()
	{
		CGameSetting.USER_GOLD += CGameSetting.GOLD_VIDEO_REWARD;
		CAdmobManager.OnVideoAdsReward -= this.OnHandleRewardAddGold;
		// ADMOB
		this.m_AdmobButton.interactable = CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD);
		// EVENT
		if (this.OnUpdateValue != null)
		{
			this.OnUpdateValue();
		}
	}

	private void OnQuitClick()
	{
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}

	#endregion
	
}
