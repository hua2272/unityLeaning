using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] public Player player;
    
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        //if (player == null) {}
        
        // 设置玩家位置
        if (player != null && SceneLoader.sceneSpawnPositions.ContainsKey(currentScene))
        {
            Vector3 spawnPos = SceneLoader.sceneSpawnPositions[currentScene];
            player.transform.position = spawnPos;
            Debug.Log($"在场景 {currentScene} 设置玩家位置: {spawnPos}");
        }
        /*else if (player != null)
        {
            Debug.LogWarning($"未找到场景 {currentScene} 的出生位置，使用默认位置");
        }*/
    }
}
