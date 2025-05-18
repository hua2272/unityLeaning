using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    
    private string dbPath;
    private IDbConnection dbConnection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDatabase()
    {
        // 数据库路径设置
#if UNITY_EDITOR
        dbPath = "URI=file:" + Application.dataPath + "/SQlite/game_data.db";
#elif UNITY_ANDROID || UNITY_IOS
            // 先检查是否已有数据库文件
            string persistentPath = Application.persistentDataPath + "/dialogue_database.db";
            if (!File.Exists(persistentPath))
            {
                // 从StreamingAssets复制到可写路径
                WWW loadDB = new WWW(Application.streamingAssetsPath + "/dialogue_database.db");
                while (!loadDB.isDone) { }
                File.WriteAllBytes(persistentPath, loadDB.bytes);
            }
            dbPath = "URI=file:" + persistentPath;
#endif
        OpenDatabase();
    }

    void OpenDatabase()
    {
        dbConnection = new SqliteConnection(dbPath);
        dbConnection.Open();
        Debug.Log("Connected to database");
    }

    public IDataReader ExecuteQuery(string query)
    {
        IDbCommand dbCmd = dbConnection.CreateCommand();
        dbCmd.CommandText = query;
        return dbCmd.ExecuteReader();
    }

    void OnDestroy()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
            dbConnection = null;
        }
    }
}