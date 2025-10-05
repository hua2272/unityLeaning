using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Player player;
    public PlayerStatus playerStatus;
    public PlayerNPCDetector playerNpcDetector;

    private void Awake()
    {
        Debug.Log("-------PlayerManager instance------");
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
        GameDataManager gameData = GameDataManager.instance;
        if (gameData != null)
        {
            player.transform.position = new Vector3(gameData.playerPosition.x, gameData.playerPosition.y, gameData.playerPosition.z);
        }
    }
}
