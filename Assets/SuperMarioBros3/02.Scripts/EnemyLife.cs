using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour  // #11 적 머리 밟았을 때, 적을 죽이는 기능
{
    public enum ENEMY_STATE {IDLE = 1, DIE};    // #9 Enemy의 상태
    public ENEMY_STATE enemystate = ENEMY_STATE.IDLE;

    private PlayerCtrl playerCtrl;
    
// #15 플레이어에게 머리 밟혔는지 확인용 & 등껍질로 변신하도록
    public bool beStepped = false;          // PlayerCtrl에서 true, false 적용됨
    private EnemyCtrl enemyCtrl;    // #15
    private GameObject trampledBody;       // #15
    private GameObject body;        // #15
    private BoxCollider2D boxCollider2D; // #15

    private Rigidbody2D rBody;      // #19

    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        
        enemyCtrl = GetComponent<EnemyCtrl>();      // #15
        boxCollider2D = GetComponent<BoxCollider2D>();

        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :
            case EnemyCtrl.ENEMY_TYPE.TURTLE : 
                rBody = GetComponent<Rigidbody2D>();    // #19
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)  // 콜라이더 위치상, ㅡIsTrigger 체크가 된 함수 먼저 실행 -> IsTrigger 체크 안 된 함수 실행되기 때문에~
    {
        if(enemystate == ENEMY_STATE.DIE)       // 이미 죽었으면 아래 코드 실행 X   //#9 리팩터링
            return;



        
        if(other.gameObject.tag == "Player")    // #11
        {

            switch(enemyCtrl.enemyType) // #16 등껍질 차는 건, 꼭 적의 머리쪽을 밟지 않아도 되기 때문에 // 아래 if(beStepped) 문보다 먼저 실행되어야 중복 실행 방지 가능 - 밟자마자 등껍질이 날라가는 현상
            {              
                case EnemyCtrl.ENEMY_TYPE.GOOMBA : 
                    if(beStepped)  // #15 만약 플레이어가 Enemy의 머리를 밟은 거라면
                    {
                        Debug.Log("#11 플레이어가 Enemy 머리 밟음");    
                        other.gameObject.GetComponent<PlayerCtrl>().BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음
                        enemystate = ENEMY_STATE.DIE;  //#9 리팩터링
                        IsDie();            // #19 죽었을 때 효과
                    }
                    break;
                
                case EnemyCtrl.ENEMY_TYPE.TURTLE : 
                    if(beStepped)  // #15 만약 플레이어가 Enemy의 머리를 밟은 거라면
                    {
                        Debug.Log("#11 플레이어가 Enemy 머리 밟음");
                        other.gameObject.GetComponent<PlayerCtrl>().BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음
                        IsDie();           // #15 등껍질로 변신
                    }
                    break;

                case EnemyCtrl.ENEMY_TYPE.SHELL :   // #16 등껍질 밟았을 때 
                    this.gameObject.layer = 16;     // #24 껍질 날라갈 때, (LargeBlock 레이어) 블록에 부딪히지 않도록
                    if(other.gameObject.transform.position.x < this.gameObject.transform.position.x)    // 플레이어가 Enemy의 왼쪽에 있을 때
                    {
                        if(enemyCtrl.enemyDir == -1)    // fix: 왼쪽 방향으로 바라보고 있었다면 - 방향 변경하기
                            enemyCtrl.Flip();           // 날라가는 방향 설정 - 플레이어가 왼쪽에서 차면 오른쪽으로 날아가도록

                        Debug.Log("//#16 오른쪽으로 차기");
                    }
                    else
                    {
                        if(enemyCtrl.enemyDir == 1)     // fix: 오른쪽 방향으로 바라보고 있었다면 - 방향 변경하기
                            enemyCtrl.Flip();       
                        Debug.Log("//#16 왼쪽으로 차기");
                    }
                    enemyCtrl.kickShell = true;     // 한쪽 방향으로 날라가기 - EnemyCtrl 스크립트 내 FixedUpdate 에서 실행
                    enemystate = ENEMY_STATE.DIE;  //#9 리팩터링
                    IsDie();
                    break;
            }

            // else
            //     Debug.Log("#11 플레이어랑 그냥 부딪힘");

        }        
    }
    
    private void IsDie()                
    {
        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :      // #19 죽을 때 - 굼바는 찌그러짐
                rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;
                
                body.SetActive(false);              // 기존 바디 비홣성화
                trampledBody.SetActive(true);       // 밟힌 이미지 활성화

                Invoke("DestoryEnemy", 0.3f);       // 1초 후 소멸
                break;

            case EnemyCtrl.ENEMY_TYPE.TURTLE :      // #15 등껍질로 변신

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;

                body.SetActive(false);              // 기존 바디 비홣성화
                trampledBody.SetActive(true);       // 등껍질 이미지 활성화

                Vector2 size = boxCollider2D.size;  // 등껍질로 사이즈 맞추기
                size.y = 0.7f;
                boxCollider2D.size = size;

                enemyCtrl.enemyType = EnemyCtrl.ENEMY_TYPE.SHELL;   // #16 밟으면 상태 변화

                break;
            
            case EnemyCtrl.ENEMY_TYPE.SHELL :       // #19 등껍질 밟거나 차면 3초 후 소멸
                rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록

                Invoke("DestoryEnemy", 2.0f);   
                break;
        }
    }
    private void DestoryEnemy() // #16 Enemy 소멸
    {
        Destroy(this.gameObject);
    }

    // private void OnTriggerEnter2D(Collider2D col)   
    // {
    //     switch(enemyCtrl.enemyType)
    //     {
    //         case EnemyCtrl.ENEMY_TYPE.GOOMBA : 
    //         case EnemyCtrl.ENEMY_TYPE.TURTLE : 
    //             if(col.gameObject.tag == "Player")  // #15 굼바, 거북 Enemy의 머리 위에 있는 headCheck 트리거 콜라이더를 (플레이어에게) 밟혔을 때 
    //             {
    //                 beStepped = true;
    //             }
    //             break;
    //     }
    // }
}
