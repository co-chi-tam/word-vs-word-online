using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CShopItem : MonoBehaviour {

	protected int m_ItemIndex;
	public int itemIndex 
	{ 
		get { return this.m_ItemIndex; }
		set { this.m_ItemIndex = value; }
	}

	protected Image m_ShopItemImage;
	protected Button m_BuyOrUseButton;
	protected GameObject m_BuyImage;
	protected GameObject m_UseImage;
	protected Text m_PriceText;

	public enum EShopType: int
	{
		NONE = 0,
		FRAME = 1,
		AVATAR = 2
	}

	public class CShopData {
		public string displaySprite;
		public int price;
		public EShopType type;
		public int value; 
		public bool isUsing;

		public CShopData()
		{
			this.displaySprite = string.Empty;
			this.price = 100;
			this.type = EShopType.NONE;
			this.value = 0;
			this.isUsing = false;
		}
	}

	protected virtual void Awake()
	{
		this.Init();
	}

	public virtual void Init()
	{
		this.m_ShopItemImage = this.transform.Find("Container/ShopItemImage").GetComponent<Image>();
		this.m_BuyOrUseButton = this.transform.Find("Container/BuyOrUseButton").GetComponent<Button>();
		this.m_BuyImage 	= this.transform.Find("Container/BuyOrUseButton/BuyImage").gameObject;
		this.m_UseImage 	= this.transform.Find("Container/BuyOrUseButton/UseImage").gameObject;
		this.m_PriceText 	= this.transform.Find("Container/SpricePanel/PriceText").GetComponent<Text>();
	}

	public virtual void Setup(int index, Sprite display, bool isUsing, int price, System.Action<int> callback)
	{
		this.m_ItemIndex = index;
		this.m_ShopItemImage.sprite = display;
		this.UpdateItem (isUsing);
		this.m_PriceText.text = string.Format ("x{0}", price);
		// EVENTS
		this.m_BuyOrUseButton.onClick.RemoveAllListeners();
		this.m_BuyOrUseButton.onClick.AddListener(() => {
			if (callback != null)
			{
				callback (this.m_ItemIndex);
			}
		});
	}

	public virtual void UpdateItem(bool isUsing)
	{
		this.m_BuyOrUseButton.interactable = !isUsing;
		this.m_BuyImage.SetActive (!isUsing);
		this.m_UseImage.SetActive (isUsing);
	}

}
