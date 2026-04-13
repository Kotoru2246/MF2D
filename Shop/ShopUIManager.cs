using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
public class ShopUIManager : MonoBehaviour
{ 
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI totalgoldtext;
    [SerializeField] private Transform Shopiteamcontainer;
    [SerializeField] private GameObject ShopiteamPrefab;
    [SerializeField] private Button exitbutton;

    private List<ShopIteamUI> spawnedItems = new List<ShopIteamUI>();


    private void Start()
    {
        InitializeShopiteams();
        exitbutton.onClick.RemoveAllListeners();
        exitbutton.onClick.AddListener(() =>
        {
            GameManager.Instance.GoToMainMenu();
        });
    }
    private void Update()
    {
       
     totalgoldtext.text= ShopManager.Instance.currentGold.ToString();

    }

    private void InitializeShopiteams()
    {
        foreach (var item in ShopManager.Instance.allshopiteams)
        {
            GameObject newObj = Instantiate(ShopiteamPrefab, Shopiteamcontainer);
            ShopIteamUI uiScript = newObj.GetComponent<ShopIteamUI>();
            uiScript.Setup(item);
            spawnedItems.Add(uiScript);
        }

    }
    public void RefreshAllItems()
    {
        foreach (ShopIteamUI itemUI in spawnedItems)
        {
            itemUI.UpdateUI();
        }
    }
}
