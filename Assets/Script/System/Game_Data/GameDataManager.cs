using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;
    
    public string saveTime = "";
    public string playTime = "";
    public int playerLevel;
    public int playerHealth;
    public int equippedSlotId;
    public string scene;
    public Vector3 playerPosition;
    public string[] inventoryItems;
    public GameData.SerializableDictionary unlockedSkills;
    
    // 新增：当前选择的存档槽位
    private int currentSaveSlot = 0;
    
    void Awake()
    {
        Debug.Log("<color=#FF0000>-------GameDataManager instance-------</color>");
        if (instance == null)
        {
            instance = this;
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
        saveTime = gameData.saveTime;
        playTime = gameData.playTime;
        currentSaveSlot = gameData.saveSlotId;
    }
}