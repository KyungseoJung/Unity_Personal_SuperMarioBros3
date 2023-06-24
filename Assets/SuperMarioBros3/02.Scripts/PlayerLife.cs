using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    public enum MODE_TYPE {LEVEL1 = 1, LEVEL2, LEVEL3 };      // 플레이어 타입  // PlayerCtrl -> PlayerLevel로 위치 이동
    public MODE_TYPE playerLevel = MODE_TYPE.LEVEL1;

    public enum MODE_STATE {IDLE = 1, HURT, LEVELCHANGE};
        // 다친 상태인지 체크 - 다친 상태라면 점프 불가능, 일정 시간동안 공격 안 받음   // PlayerCtrl -> PlayerLevel로 위치 이동
    public MODE_STATE playerState = MODE_STATE.IDLE;

    
    private float lastHitTime =0f;
    private float repeatDamagePeriod = 2.0f;    // #17 다친 후 쿨타임(무적시간)
    private float hurtForce = 10f;        
    
    public AudioClip hurtClip;

    private PlayerCtrl playerCtrl;
    private Vector2 lifeScale;

    void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    public void GetHurt()  // #17 플레이어 다침 - 현재 레벨에 따라 다르게 적용 
    {
        if(Time.time > lastHitTime + repeatDamagePeriod)    // #17 다친 후 일정 시간동안 무적 상태
        {
            switch(playerLevel)
            {
                case MODE_TYPE.LEVEL1 : // 죽음
                    Debug.Log("플레이어 죽음");
                    break;
                case MODE_TYPE.LEVEL2 : 
                case MODE_TYPE.LEVEL3 :
                    TakeDamage();
                    lastHitTime = Time.time;
                    break;

            }
        }
    }

    void TakeDamage()      // #17 플레이어 레벨 하락
    {
        playerState = MODE_STATE.HURT;         //다치는 순간은 점프하지 못하도록
        Invoke("ReturnToNormal", 0.3f); //0.3초 후 다시 회복
        
        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL2 : 
                playerLevel = MODE_TYPE.LEVEL1;
                break;
            case MODE_TYPE.LEVEL3 : 
                playerLevel = MODE_TYPE.LEVEL2;
                break;
        }
        AudioSource.PlayClipAtPoint(hurtClip, transform.position);
    }

    void ReturnToNormal()   // Invoke로 호출
    {   
        playerState = MODE_STATE.IDLE;
    }

    
}
