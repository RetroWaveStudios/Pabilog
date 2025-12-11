using TMPro;
using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public static MoneySystem instance;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI crystalText;

    private void Awake()
    {
        instance = this;
        moneyText.text = StaticDatas.PlayerData.PlayerInfos.Coin.ToString();
        crystalText.text = StaticDatas.PlayerData.PlayerInfos.Crystal.ToString();
    }

    public void UpdateCoin(int amount)
    {
        StaticDatas.PlayerData.PlayerInfos.Coin += amount;
        moneyText.text = StaticDatas.PlayerData.PlayerInfos.Coin.ToString();
        StaticDatas.SaveDatas();
    }

    public void UpdateCyrstal(int amount)
    {
        StaticDatas.PlayerData.PlayerInfos.Crystal += amount;
        crystalText.text = StaticDatas.PlayerData.PlayerInfos.Crystal.ToString();
        StaticDatas.SaveDatas();
    }

    public void UpdateXp(int amount)
    {
        StaticDatas.PlayerData.PlayerInfos.XP += amount;
        PlayerProfile.instance.UpdateLevelBar();
        StaticDatas.SaveDatas();
    }

    public bool hasEnough(Currency cur, int amount)
    {
        if(cur == Currency.Coin)
        {
            if (StaticDatas.PlayerData.PlayerInfos.Coin >= amount) return true;
            else { PushNotice.instance.Push("No Enough Coin", PushType.Alert); return false; }
        }
        else if (cur == Currency.Crystal)
        {
            if (StaticDatas.PlayerData.PlayerInfos.Crystal >= amount) return true;
            else { PushNotice.instance.Push("No Enough Crystal", PushType.Alert); return false; }
        }
        PushNotice.instance.Push("No Enough Money", PushType.Alert);
        return false;
    }
}
