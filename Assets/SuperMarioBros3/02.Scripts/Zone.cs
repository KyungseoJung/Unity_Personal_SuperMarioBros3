using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum ZONE_TYPE { DESTROY = 1, DIE};     // ZONE의 타입: 아예 소멸시켜버리는 존, 플레이어 죽게 하는 존
    public ZONE_TYPE zoneType = ZONE_TYPE.DESTROY;

    LobbyManager lobbyManager;

    private void Awake()
    {
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // #78 - 게임 오버 연속 실행되지 않도록 gameOver bool형 체크 목적
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        switch(zoneType)
        {
            case ZONE_TYPE.DESTROY:
                if ((col.gameObject != null) && (col.gameObject.tag != "Player"))    // #74 에러 방지 - 플레이어는 Destroy 되지 않도록
                    Destroy(col.gameObject);
                break;
            case ZONE_TYPE.DIE:
                if ((col.gameObject.tag == "Player") && (!lobbyManager.gameOver))   // #78 게임 오버 연속으로 실행되지 않도록 gameOver bool형 체크
                    col.gameObject.GetComponent<PlayerLife>().PlayerDie(true);          // #78 플레이어가 DIE ZONE에 들어가면 - 플레이어 죽도록
                break;
        }
        

    }



}
