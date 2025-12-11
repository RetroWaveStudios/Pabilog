using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticDatas : MonoBehaviour
{
    public static PlayerDatas PlayerData;
    public static int save_queue = 0;

    private void Awake()
    {
        LoadDatas();
    }
    public static void SaveDatas()
    {
        save_queue++;
        if(save_queue == 1)
            SaveLoadManager.SaveGameData(PlayerData);
    }

    public static void LoadDatas()
    {
        PlayerData = SaveLoadManager.LoadGameData();
    }

    public static void UpdateFarmSlotDatas()
    {
        List<FarmSlotStats> f_slots = new();
        for (int i = 0; i < StaticDatas.PlayerData.FarmSlots.Count; i++)
        {
            f_slots.Add(StaticDatas.PlayerData.FarmSlots[i]);
        }
        if (StaticDatas.PlayerData.FarmSlots.Count < StaticDatas.PlayerData.land_slot_count)
        {
            StaticDatas.PlayerData.FarmSlots.Add(new FarmSlotStats()
            {
                state = LandState.Empty,
                PlantDetails = new PD()
                {
                    plant = Plants.None
                }
            });
            StaticDatas.PlayerData.FarmSlots[StaticDatas.PlayerData.FarmSlots.Count - 1].PlantDetails.plant = Plants.None;
        }
        SaveDatas();
    }

    public static void UpdateAnimalSpotDatas()
    {
        List<AnimalsSpotStats> a_spots = new();
        for (int i = 0; i < StaticDatas.PlayerData.AnimalSpots.Count; i++)
        {
            a_spots.Add(StaticDatas.PlayerData.AnimalSpots[i]);
        }
        if (StaticDatas.PlayerData.AnimalSpots.Count < StaticDatas.PlayerData.animal_slot_count)
        {
            StaticDatas.PlayerData.AnimalSpots.Add(new AnimalsSpotStats()
            {
                state = ASpotState.Empty,
                AnimalProductDetails = new APD()
                {
                    animal = Animals.None,
                    theProduct = AProducts.None
                }
            });
            StaticDatas.PlayerData.AnimalSpots[StaticDatas.PlayerData.AnimalSpots.Count - 1].AnimalProductDetails.animal = Animals.None;
        }
        SaveDatas();
    }

    public static string convertToTimer(double required, double passed)
    {
        double remainingSeconds = required - passed;
        if (remainingSeconds < 0) remainingSeconds = 0;
        TimeSpan remaining = TimeSpan.FromSeconds(remainingSeconds);
        string timeString;
        if (remaining.Hours > 0) timeString = string.Format("{0:D2}:{1:D2}:{2:D2}", remaining.Hours, remaining.Minutes, remaining.Seconds);
        else timeString = string.Format("{0:D2}:{1:D2}", remaining.Minutes, remaining.Seconds);
        return timeString;
    }

    public static bool TryGetStartTime(string timeString, string productName, out DateTime startTime)
    {
        startTime = default;

        if (string.IsNullOrEmpty(timeString))
        {
            Debug.LogWarning($"StartTime is empty for {productName}");
            return false;
        }

        if (!DateTime.TryParseExact(
                timeString,
                "o",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out startTime))
        {
            Debug.LogError($"Failed to parse startTime: {timeString}");
            return false;
        }

        return true; // success
    }
}
