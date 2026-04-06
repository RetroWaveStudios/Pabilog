using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "gamedata.json");

    private static readonly PlayerDatas default_datas = new PlayerDatas()
    {
        saveVersion = 0,
        PlayerInfos = new Player()
        {
            name = null,
            IDnumber = 0,
            ppIndex = 0,

            XP = 0,
            Coin = 1500,
            Crystal = 15,
            FoodLevel = 1,
            Food = new FoodSystem()
            {
                materials = new List<TheFood>(),
                MachState = LandState.Empty,

                InQueue = new(),

                qLimit = 1,
                Amounts = new List<afAmount>()
                {
                    new afAmount()
                    {
                        food = a_f_types.Wheat,
                        amount = 5
                    },
                    new afAmount()
                    {
                        food = a_f_types.Corn,
                        amount = 0
                    },
                    new afAmount()
                    {
                        food = a_f_types.Carrot,
                        amount = 0
                    },
                    new afAmount()
                    {
                        food = a_f_types.Potato,
                        amount = 0
                    }
                },
                sumAmount = 0,
                xp = 20
            },
            WellLevel = 1,
            Water = new WaterSystem()
            {
                fillTime = "",
                fTimer = 0.5, // by minutes
                WateringTimer = 3,
                MaxAmount = 20,
                amount = 20,

                currency = Currency.Coin,
                price = 350
            },
            currentChanceOfLB = 5f
        },
        unlocked_items = new u_items()
        {
            u_plants = new List<Plants>()
            {
                Plants.Wheat,
                Plants.Corn,

            },
            u_fruits = new List<Fruits>()
            {
            },
            u_animals = new List<Animals>()
            {
            },
            u_a_products = new List<AProducts>()
            {
            },
            u_Products = new List<Products>()
            {
            },
            u_machines = new List<Unlocked_Machines>()
            {
            }
        },
        land_slot_count = 3,
        FarmSlots = new List<FarmSlotStats>()
        {
            new()
            {
                state = LandState.Empty,
                PlantDetails = new PD()
                {
                    plant = Plants.None
                }
            }
        },

        TreeSpots = new List<TreeSpotStats>(),
        animal_slot_count = 1,
        AnimalSpots = new List<AnimalsSpotStats>()
        {
            new()
            {
                state = ASpotState.Empty,
                AnimalProductDetails = new APD()
                {
                    animal = Animals.None,
                    theProduct = AProducts.None
                }
            }
        },

        Storage = new StorageItems()
        {
            PlantsInStorage = new List<PlantCount>()
            {
                new()
                {
                    Plant = Plants.Wheat,
                    count = 3
                },
                new()
                {
                    Plant = Plants.Corn,
                    count = 3
                }
            }
        },

        StorageLevel = 1,
        ShopSlotCount = 3
    };
    private const int CURRENT_VERSION = 2;

    public static void SaveGameData(PlayerDatas data)
    {
        data.saveVersion = CURRENT_VERSION;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        StaticDatas.save_queue = 0;
    }

    public static PlayerDatas LoadGameData()
    {
        string path = Path.Combine(Application.persistentDataPath, "gameData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerDatas pd = JsonUtility.FromJson<PlayerDatas>(json);
            pd = SaveDataMerger.MergeWithDefaults(pd, default_datas);
            return pd;
        }
        else
        {
            SaveGameData(default_datas);
            Debug.LogWarning("Save file not found in " + path);
            return default_datas; // Return a new instance with default values
        }
    }
}
