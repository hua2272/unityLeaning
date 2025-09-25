using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;
    public Dash_Skill dash { get; private set; }
    public Sword_Skill sword { get; private set; }
    public FeiLeiShen_Skill feiLeiShen { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        dash = GetComponent<Dash_Skill>();
        sword = GetComponent<Sword_Skill>();
        feiLeiShen = GetComponent<FeiLeiShen_Skill>();
    }

    public void UpgradeSkill(string skillName, int level)
    {
        switch (skillName)
        {
            case "dash":
                dash.skillLevel = level;
                break;
            case "feiLeiShen":
                feiLeiShen.skillLevel = level;
                break;
        }
    }
}