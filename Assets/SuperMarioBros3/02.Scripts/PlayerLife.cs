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
// #36 레벨 변경시
    private BoxCollider2D boxCollider2D;        // #36
    

    void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();

        boxCollider2D = GetComponent<BoxCollider2D>();  // #36
    }

    public void GetHurt()   // #17 플레이어 다침 - 현재 레벨에 따라 다르게 적용 
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
                    LevelDown();
                    lastHitTime = Time.time;
                    break;

            }
        }
    }

    void LevelDown()       // #17 플레이어 레벨 하락    // TakeDamage() -> LevelDown() 이름 변경
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

        ChangeLevel();      // #36 레벨 변경될 때 고려되는 요인들 변경
    }

    void ReturnToNormal()   // Invoke로 호출
    {   
        playerState = MODE_STATE.IDLE;
    }

    public void LevelUp()       // #36 레벨업
    {
        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL1:
                playerLevel = MODE_TYPE.LEVEL2;
                break;
            
            case MODE_TYPE.LEVEL2:
                playerLevel = MODE_TYPE.LEVEL3;
                break;
            
            case MODE_TYPE.LEVEL3:
                break;
        }
        ChangeLevel();
    }

    public void ChangeLevel()   // #36 레벨 변경 - 콜라이더 y길이(1<->1.6), 해당 오브젝트만 켜기, 그라운드체크 재설정 
                                // 헤드체크 위치는 상관 없음. 플레이어에서 조정하는 경우가 없기 때문에, 태그만 잘 붙어있으면 됨.
    {
        // Debug.Log("//#36 레벨 변경");
        
        Vector2 size = boxCollider2D.size;

        Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식
        Transform secondChild = transform.GetChild(1);
        Transform thirdChild = transform.GetChild(2);

        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL1:                          // 레벨1로 변경됐다면
                size.y = 0.9f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                secondChild.gameObject.SetActive(false);    // 오브젝트 끄고 켜기
                thirdChild.gameObject.SetActive(false);
                firstChild.gameObject.SetActive(true);  

                playerCtrl.groundCheck = firstChild.Find("groundCheck");
                playerCtrl.anim = firstChild.GetComponent<Animator>();  // #36

                break;

            case MODE_TYPE.LEVEL2:                          // 레벨2로 변경됐다면
                size.y = 1.6f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                firstChild.gameObject.SetActive(false);     // 오브젝트 끄고 켜기
                thirdChild.gameObject.SetActive(false);
                secondChild.gameObject.SetActive(true);

                playerCtrl.groundCheck = secondChild.Find("groundCheck");
                playerCtrl.anim = secondChild.GetComponent<Animator>(); // #36

                break;
            
            case MODE_TYPE.LEVEL3:                          // 레벨3로 변경됐다면
                size.y = 1.6f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                firstChild.gameObject.SetActive(false);     // 오브젝트 끄고 켜기
                secondChild.gameObject.SetActive(false);
                thirdChild.gameObject.SetActive(true);  

                playerCtrl.groundCheck = thirdChild.Find("groundCheck");
                playerCtrl.anim = thirdChild.GetComponent<Animator>();  // #36

                break;
        }

    }
    
}
