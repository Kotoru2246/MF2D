using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ShopIteamUI : MonoBehaviour
{
    [Header("UI Reference")]
     [SerializeField]private Image IteamImage;
    [SerializeField]private TextMeshProUGUI _nametext;
    [SerializeField]private TextMeshProUGUI _leveltext;
    [SerializeField]private TextMeshProUGUI _costtext;
    [SerializeField]private Button _buyer;
     private ShopIteamdata _shop;

    public void Setup(ShopIteamdata data)
    {
        _shop = data;
        IteamImage.sprite = data.sprite;
        _nametext.text = data.IteamName;
        
        _buyer.onClick.RemoveAllListeners();
        _buyer.onClick.AddListener(OnBuyButtonClicked);

        UpdateUI(); 
    }
    public void UpdateUI()
    {
        int currentLevel = ShopManager.Instance.GetItemLevel(_shop.IteamID);
        int cost = _shop.GetCostForNextLevel(currentLevel);

        if (currentLevel >= _shop.maxLevel)
        {
            _leveltext.text = "MAX";
            _costtext.text = "H?t hàng";
            _buyer.interactable = false; 
        }
        else
        {
            _leveltext.text = $"Lv: {currentLevel}/{_shop.maxLevel}";
            _costtext.text = cost.ToString() + " Vàng";

          
            _buyer.interactable = (ShopManager.Instance.currentGold >= cost);
        }
    }

    // X? lý khi b?m nút mua
    private void OnBuyButtonClicked()
    {
      
        bool success = ShopManager.Instance.BuyUpgrade(_shop);

        if (success)
        {
           
            UpdateUI();

           
        }
    }
}
