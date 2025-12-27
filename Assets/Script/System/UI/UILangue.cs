using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class UILangue : MonoBehaviour
{
    public static UILangue instance { get; private set; }
    public int currentLangue;
    public Dictionary<string, string> Content4Menu = new Dictionary<string, string>();

    void Awake()
    {
        Debug.Log("<color=#FF0000>-------UILangue instance-------</color>");
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentLangue = PlayerPrefs.GetInt("Langue");
        GetContent4Menu();
    }

    public string Content(int id)
    {
        string query = $@"
            SELECT zh, en, jp
            FROM UI_langue 
            WHERE id = {id}";

        IDataReader reader = DatabaseManager.instance.ExecuteQuery(query);
        string content = "";
        while (reader.Read())
        {
            switch (currentLangue)
            {
                case 0:
                    content = reader.GetString(reader.GetOrdinal("zh"));
                    //Debug.Log("id = " + id + "content = " + content);
                    break;
                case 1:
                    content = reader.GetString(reader.GetOrdinal("en"));
                    //Debug.Log("id = " + id + "content = " + content);
                    break;
            }
        }
        reader.Close();
        return content;
    }

    public Dictionary<string, string> GetContent4Menu()
    {
        Content4Menu.Clear();
        switch (currentLangue)
        {
            case 0:
                Content4Menu.Add("continue", "继续游戏");
                Content4Menu.Add("select", "读取存档");
                Content4Menu.Add("save", "保存游戏");
                Content4Menu.Add("setting", "设置");
                break;
            case 1:
                Content4Menu.Add("continue", "continue");
                Content4Menu.Add("select", "select");
                Content4Menu.Add("save", "save");
                Content4Menu.Add("setting", "setting");
                break;
        }
        return Content4Menu;
    }

    public void ChangeLanguage(int language)
    {
        currentLangue = language;
        // PlayerPrefs.SetInt("Langue", language);
        // PlayerPrefs.Save();
    }
}