using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("UI Parameters")]
    public GameObject itemIcon;
    public GameObject priceIcon;
    public TextMeshProUGUI infoText;

    public Button btn;

    [Header("General Parameters")]
    public int ssNumber;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI countText;
    public int price;
    public int count;
    public object TheItem;
    public bool sell = false;

    [Header("Selling Time Parameters")]
    public float sellTimer = 1f;
    public float countdown = 0;

    private void Awake()
    {
        btn = GetComponent<Button>();
        infoText.gameObject.SetActive(true);
        infoText.text = "Sell Something";
        priceIcon.SetActive(false);
        itemIcon.SetActive(false);
        countText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (sell)
        {
            countdown += Time.deltaTime;
            if (countdown > sellTimer) SellItem();
        }
    }

    public void LoadUI()
    {
        if (sell)
        {
            infoText.gameObject.SetActive(false);
            countText.gameObject.SetActive(true);
            priceIcon.SetActive(true);
            itemIcon.SetActive(true);
            priceText.text = price.ToString();
            countText.text = "x" + count.ToString();
            itemIcon.GetComponent<Image>().sprite = Sprites.instance.GetSpriteFromSource(TheItem);
        }
        else
        {
            infoText.gameObject.SetActive(true);
            countText.gameObject.SetActive(false);
            priceIcon.SetActive(false);
            itemIcon.SetActive(false);
        }
    }

    public void SetItem()
    {
        MyShop.instance.SellWindowState(true);
        MyShop.instance.PopulateHodler(MyShop.instance.SellItemHolder);
        MyShop.instance.ssNumber = ssNumber;
    }

    public void SellItem()
    {
        MoneySystem.instance.UpdateCoin(price, out bool s);
        sell = false;
        countdown = 0;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SetItem());
        LoadUI();
        StaticDatas.SaveDatas();
    }
}
