using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;
    
    public int playerLevel;
    public int playerHealth;
    public int equippedSlotId;
    public string scene;
    public Vector3 playerPosition;
    public string[] inventoryItems;
    //public Dictionary<string, int> unlockedSkills;
    public GameData.SerializableDictionary unlockedSkills;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DataPersistence(GameData gameData)
    {
        playerLevel =  gameData.playerLevel;
        playerHealth = gameData.playerHealth;
        equippedSlotId = gameData.equippedSlotId;
        scene = gameData.scene;
        playerPosition = gameData.playerPosition;
        inventoryItems = gameData.inventoryItems;
        unlockedSkills = gameData.unlockedSkills;
    }
}
