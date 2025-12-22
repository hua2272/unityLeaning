using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class UILangue : MonoBehaviour
{
    public static UILangue instance { get; private set; }
    public int currentLangue;

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
            switch (PlayerPrefs.GetInt("Langue"))
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

    public void ChangeLanguage(int language)
    {
        currentLangue = language;
        // PlayerPrefs.SetInt("Langue", language);
        // PlayerPrefs.Save();
    }
}