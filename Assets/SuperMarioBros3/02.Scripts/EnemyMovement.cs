using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour  
    // #67 플레이어가 가까이 다가가야만 Enemy가 움직이도록 
{
    Transform followCamTransform;
    // Transform playerTransform;
    EnemyCtrl enemyCtrlScript;
    void Awake()
    {
        followCamTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
        // playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        enemyCtrlScript = GetComponent<EnemyCtrl>();    // 각 Enemy의 EnemyCtrl 스크립트 가져오기
    }   
    
    void Start()
    {
        EnemyMove(false);   // 처음에는 움직이지 않는 상태로 시작
    }

    void FixedUpdate()
    {
        if(this.transform.position.x - followCamTransform.position.x < 12)  
        // #67 보완: 7.5 -> 12로 변경: 화면에 들어오지 않아도 출발하도록 해서(Enemy가 움직이는 시점인 카메라 위치를 더 넓게) 움직임을 좀 더 자연스럽도록  
        {
            EnemyMove(true);    // 움직이기 시작
        }
    }

    void EnemyMove(bool _move)
    {
        switch(_move)
        {
            case true :
                enemyCtrlScript.enabled = true;
                break;
            case false : 
                enemyCtrlScript.enabled = false;
                break;
        }
    }
}
