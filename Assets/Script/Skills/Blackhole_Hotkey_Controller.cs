using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Blackhole_Hotkey_Controller : MonoBehaviour
{
    private KeyCode myHotkey;
    private TextMeshProUGUI myText;

    public void SetupHotkey(KeyCode _myNewHotkey)
    {
        myText = GetComponentInChildren<TextMeshProUGUI>();
        myHotkey = _myNewHotkey;
        myText.text = _myNewHotkey.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(myHotkey))
        {
            Debug.Log("hotkey is " + myHotkey);
        }
    }
}
