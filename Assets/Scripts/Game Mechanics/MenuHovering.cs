using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuHovering : MonoBehaviour
{
    public List<GameObject> Menus;

    private void Start()
    {
        SetIndexs(transform.Find("Menus"));
    }

    public void OpenMenu(int index)
    {
        ResetStates();
        for (int i = 0; i < Menus.Count; i++)
        {
            if (i == index) StaticDatas.AdjustCanvasGroup(Menus[i].GetComponent<CanvasGroup>(), true);
            else StaticDatas.AdjustCanvasGroup(Menus[i].GetComponent<CanvasGroup>(), false);
        }
    }

    private void SetIndexs(Transform t)
    {
        Debug.Log($"setting indexes for {t.name}");
        foreach (Transform item in t)
        {
            Debug.Log($"SetIndexs: item.name = {item.name}");
            Button btn = item.GetComponent<Button>();
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();

            for (int i = 0; i < Menus.Count; i++)
            {
                int index = i;
                Transform found = Menus[i].transform;
                if (found != null && item.name == found.name)
                {
                    Debug.Log($"SetIndexs: match found = {found.name}");
                    Debug.Log($"sending index {index} for {found.name}");
                    btn.onClick.AddListener(() => OpenMenu(index));
                    break;
                }
            }
        }
    }

    public void ResetStates()
    {
        if(FarmLogic.instance != null)
            FarmLogic.instance.ResetSituation();

        if (AnimalsLogic.instance != null)
            AnimalsLogic.instance.ResetSituation();

        if (ProductionLogic.instance != null)
            ProductionLogic.instance.ResetSituation();

        if (MyShop.instance != null)
            MyShop.instance.ResetSituation();

        if (ForestLogic.instance != null)
            ForestLogic.instance.ResetSituation();

        PlayerProfile.instance.infoWindow.SetActive(false);
    }
}