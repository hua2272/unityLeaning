using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class GameSaveManager : MonoBehaviour
{
    // 游戏数据类（可自定义你需要保存的数据）
    [System.Serializable]
    public class GameData
    {
        public int playerLevel;
        public float playerHealth;
        public Vector3 playerPosition;
        public string[] inventoryItems;
        // 添加其他需要保存的字段...
    }

    // 当前游戏数据
    private GameData currentGameData = new GameData();

    // 保存游戏方法
    public void SaveGame()
    {
        // 准备要保存的数据（这里只是示例，实际应从游戏各处获取数据）
        currentGameData.playerLevel = 5;
        currentGameData.playerHealth = 85.5f;
        currentGameData.playerPosition = new Vector3(10.2f, 0.5f, 15.7f);
        currentGameData.inventoryItems = new string[] { "Sword", "Potion", "Key" };
        string jsonData = JsonUtility.ToJson(currentGameData, prettyPrint: true);

        // 获取保存路径
        string savePath = GetSavePath();
        
        try
        {
            File.WriteAllText(savePath, jsonData);
            Debug.Log("游戏保存成功: " + savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("保存游戏失败: " + e.Message);
        }
        
    }

    // 获取保存路径
    private string GetSavePath()
    {
        // 获取游戏可执行文件所在目录
        string gameDirectory = Path.GetDirectoryName(Application.dataPath);
        
        // 如果是在编辑器中运行，路径会有所不同
        if (Application.isEditor)
        {
            gameDirectory = Application.persistentDataPath;
        }
        
        // 创建保存目录（如果不存在）
        string saveDirectory = Path.Combine(gameDirectory, "Saves");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        // 返回完整的保存文件路径
        return Path.Combine(saveDirectory, "gameSave.dat");
    }
}