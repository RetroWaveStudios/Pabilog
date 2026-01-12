using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ATask : MonoBehaviour
{
    public Task task;
    public TaskState slotState;
    public int slotNumber;
    public string EmTime;

    private Button btn;

    public List<Sprite> stateImages;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }
    private void Start()
    {
        slotState = TasksLogic.instance.TaskList.Tasks[slotNumber].state;
        UpdateBtn();
        SetDetails();
    }

    private void Update()
    {
        if(slotState == TaskState.Empty) { CheckSpawnTimer(); UpdateTimer(); }
    }

    private void UpdateBtn()
    {
        btn.onClick.RemoveAllListeners();
        if (slotState == TaskState.HasTask)
            btn.onClick.AddListener(() => ShowDetails());
    }

    public void SetDetails()
    {
        transform.Find("Empty").gameObject.SetActive(false);
        transform.Find("HasTask").gameObject.SetActive(false);
        if (slotState == TaskState.Empty)
        {
            transform.Find("Empty").gameObject.SetActive(true);
            gameObject.GetComponent<Image>().sprite = stateImages[transform.Find("Empty").gameObject.activeInHierarchy ? 0 : 1];
            gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
        else
        {
            transform.Find("HasTask").gameObject.SetActive(true);
            gameObject.GetComponent<Image>().sprite = stateImages[transform.Find("HasTask").gameObject.activeInHierarchy ? 1 : 0];
            gameObject.GetComponent<Image>().color = new Color32(200, 170, 0, 255);
            transform.Find("HasTask/Money/Icon").GetComponent<Image>().sprite = Sprites.instance.sprites.currencies.Find(e => e.Currency == task.currency).sprite;

            transform.Find("HasTask/Money/Reward").GetComponent<TextMeshProUGUI>().text = task.Money.ToString();
            transform.Find("HasTask/Xp/Reward").GetComponent<TextMeshProUGUI>().text = task.Xp.ToString();
        }
        TasksLogic.instance.CheckTasks();
    }

    private void ShowDetails()
    {
        TasksLogic.instance.PopulateInfoHolder(slotNumber);
    }

    private void CheckSpawnTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TasksLogic.instance.TaskList.Tasks[slotNumber].EmptyTime, "task slot " + slotNumber.ToString(), out startTime))
        { startTime = DateTime.UtcNow; TasksLogic.instance.TaskList.Tasks[slotNumber].EmptyTime = startTime.ToString("o"); EmTime = startTime.ToString("o"); SaveState(); }

        TimeSpan elapsed = DateTime.UtcNow - startTime;
        double elapsedMinutes = elapsed.TotalMinutes;
        double elapsedSeconds = elapsed.TotalSeconds;

        /*
        float progress = Mathf.Clamp01((float)(elapsedSeconds / (TasksLogic.instance.TaskList.Tasks[slotNumber].timeToSpawn * 60)));
        // GrowthTime assumed in minutes → multiply by 60 for seconds

        Image filler = waterTimer.GetComponent<Image>();
        filler.fillAmount = 1f - progress;
        */

        // --- Check if ready to harvest ---
        if (elapsedMinutes >= TasksLogic.instance.TaskList.Tasks[slotNumber].timeToSpawn)
            SkipTimer(false);
    }

    public void SkipTimer(bool skipping)
    {
        bool enought = true;
        if (skipping) MoneySystem.instance.UpdateCyrstal(-1, out enought);
        if(enought)
        {
            TaskType chosen = TasksLogic.instance.CreateTaskToRequest(slotNumber, false);
            task = TasksLogic.instance.SendRequest(chosen, TasksLogic.instance.totalPoints[chosen]);
            if (task.difficulty != TaskType.None) slotState = TaskState.HasTask;
            EmTime = "";
            UpdateBtn();
            SetDetails();
            SaveState();
            TasksLogic.instance.CheckTasks();
        }
    }

    private void UpdateTimer()
    {
        DateTime startTime;
        if (!StaticDatas.TryGetStartTime(TasksLogic.instance.TaskList.Tasks[slotNumber].EmptyTime, "task slot " + slotNumber.ToString(), out startTime)) return;

        double totalSecondsRequired = TasksLogic.instance.TaskList.Tasks[slotNumber].timeToSpawn * 60;
        double elapsedSeconds = (DateTime.UtcNow - startTime).TotalSeconds;
        string timeString = StaticDatas.convertToTimer(totalSecondsRequired, elapsedSeconds);
        transform.Find("Empty/Timer").GetComponent<TextMeshProUGUI>().text = timeString;
    }

    public void CompleteTask()
    {
        Debug.Log($"Send clicked for {slotNumber}");
        if (task.Plants != null && task.Plants.Count > 0)
            for (int i = 0; i < task.Plants.Count; i++) { Debug.Log($"Deleting {task.Plants[i].Plant} at count {task.Plants[i].count}"); Storage.instance.UpdateThingCount(task.Plants[i].Plant, -task.Plants[i].count); }

        if (task.Fruits != null && task.Fruits.Count > 0)
            for (int i = 0; i < task.Fruits.Count; i++) { Debug.Log($"Deleting {task.Fruits[i].Fruit} at count {task.Fruits[i].count}"); Storage.instance.UpdateThingCount(task.Fruits[i].Fruit, -task.Fruits[i].count); }

        if (task.AnimalFoods != null && task.AnimalFoods.Count > 0)
            for (int i = 0; i < task.AnimalFoods.Count; i++) { Debug.Log($"Deleting {task.AnimalFoods[i].amount} at count {task.AnimalFoods[i].amount}"); Storage.instance.UpdateThingCount(task.AnimalFoods[i].food, -task.AnimalFoods[i].amount); }

        if (task.AnimalProducts != null && task.AnimalProducts.Count > 0)
            for (int i = 0; i < task.AnimalProducts.Count; i++) { Debug.Log($"Deleting {task.AnimalProducts[i].animal_products} at count {task.AnimalProducts[i].count}"); Storage.instance.UpdateThingCount(task.AnimalProducts[i].animal_products, -task.AnimalProducts[i].count); }

        if (task.Products != null && task.Products.Count > 0)
            for (int i = 0; i < task.Products.Count; i++) { Debug.Log($"Deleting {task.Products[i].product} at count {task.Products[i].count}"); Storage.instance.UpdateThingCount(task.Products[i].product, -task.Products[i].count); }

        if (task.Items != null && task.Items.Count != 0)
            for (int i = 0; i < task.Items.Count; i++) { Debug.Log($"Deleting {task.Items[i].item} at count {task.Items[i].count}"); Storage.instance.UpdateThingCount(task.Items[i].item, -task.Items[i].count); }

        Debug.Log($"Adding {task.Money} coins");
        Debug.Log($"Adding {task.Xp} xp");
        MoneySystem.instance.UpdateCoin(task.Money, out bool e);
        MoneySystem.instance.UpdateXp(task.Xp);

        LuckyBox.instance.TryToFindBox(1f);

        DeleteTask();
    }

    public void DeleteTask()
    {
        Debug.Log($"Reseting task slot to empty");
        task = new Task()
        {
            difficulty = TaskType.None,
            currency = Currency.Coin
        };
        slotState = TaskState.Empty;
        EmTime = "";
        UpdateBtn();
        SetDetails();
        PlantsHolder.instance.PopulatePlantsHolder();
        TasksLogic.instance.PopulateInfoHolder(1000);
        SaveState();
    }

    private void SaveState()
    {
        TasksLogic.instance.TaskList.Tasks[slotNumber].state = slotState;
        TasksLogic.instance.TaskList.Tasks[slotNumber].Task = task;
        TasksLogic.instance.TaskList.Tasks[slotNumber].EmptyTime = EmTime;
        TasksLogic.instance.SaveDatas();
    }
}