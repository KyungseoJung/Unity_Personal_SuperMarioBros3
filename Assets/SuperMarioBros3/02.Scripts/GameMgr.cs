﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocreInfo
{
    public int score = 0;              // #29 플레이어 획득 점수

}
public class GameMgr : MonoBehaviour    // #29 플레이어 게임 관리 싱글톤 클래스
{
    private SocreInfo scoreInfo;

    private static GameMgr mgr = null;  // 싱글톤 객체 (인스턴스)
    public static GameMgr Mgr           // 싱글톤 프로퍼티
    {
        get
        {
            if(mgr == null)
            {
                mgr = GameObject.FindObjectOfType(typeof(GameMgr)) as GameMgr;
                    //이런 타입을 가진 오브젝트가 있다면, 그 오브젝트를 InfoManager로서 객체화 해라
                if(mgr == null)
                {
                    mgr = new GameObject("Singleton_GameMgr", typeof(GameMgr)).GetComponent<GameMgr>();
                    DontDestroyOnLoad(mgr);
                }

            }
            return mgr;
        }
    }
    void Awake()    //Start에 적으면 다른 것들보다 늦게 실행돼서 Null 에러 발생함.
    {
        scoreInfo = new SocreInfo();

    }

    public int score
    {
        get {return scoreInfo.score; }
        set {scoreInfo.score = value; }
    }

}