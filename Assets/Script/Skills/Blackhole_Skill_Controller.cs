using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Blackhole_Skill_Controller : MonoBehaviour
{
    [SerializeField] private GameObject hotkeyPrefab;
    [SerializeField] private List<KeyCode> keyCodeList;
    public float maxSize;
    public float growSpeed;
    public bool canGrow;
    
    public List<Transform> targets;

    private void Update()
    {
        if (canGrow)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, new Vector2(maxSize, maxSize), Time.deltaTime * growSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>() != null)
        {
            //targets.Add(collision.transform);
            //todo 冻结，时停敌人
            GameObject newHotkey = Instantiate(hotkeyPrefab, collision.transform.position + new Vector3(0,2), Quaternion.identity);
            KeyCode chooseKey = keyCodeList[Random.Range(0, keyCodeList.Count)];
            keyCodeList.Remove(chooseKey);
            Blackhole_Hotkey_Controller newHotkeyScript = newHotkey.GetComponent<Blackhole_Hotkey_Controller>();
            newHotkeyScript.SetupHotkey(chooseKey);
        }
    }
}
