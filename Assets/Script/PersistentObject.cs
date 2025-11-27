using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    public static PersistentObject instance;
    
    [Header("Persistent")] 
    public GameObject[] PersistObj;


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
            return;
        }
        foreach (GameObject obj in PersistObj)
        {
            if (obj != null)
                DontDestroyOnLoad(obj);
        }
    }
}
