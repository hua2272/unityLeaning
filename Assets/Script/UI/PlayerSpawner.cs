using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    
    void Start()
    {
        // 只在当前场景没有玩家时生成
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            Debug.Log("--------->>>>>>>>spawnplayer");
            SpawnPlayer();
        }
        else
        {
            Debug.Log("nobody--------->>>>>>>>");
        }
    }

    public void SpawnPlayer()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        
        if (spawnPoint != null)
        {
            GameObject player = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);
            
            // 确保玩家正确初始化
            player.SetActive(true);
            Debug.Log($"玩家已生成在 {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogError("未找到重生点！");
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}