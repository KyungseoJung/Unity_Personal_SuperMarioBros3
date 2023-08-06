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
    private bool getHitByTail = false;      // #57 꼬리에 한번만 치이도록 하기 위한 bool형 변수
    private EnemyCtrl enemyCtrl;    // #15
    private GameObject trampledBody;       // #15
    private GameObject body;        // #15
    private BoxCollider2D boxCollider2D; // #15

    private Rigidbody2D rBody;      // #19
// #19 몬스터 죽인 후 등장하는 PointUi
    public GameObject pointUi;
// #34 몬스터 애니메이션
    private Animator anim;
// #35
    private LobbyManager lobbyManager;           // #35 점수 체크용

    private float hitForce = 5000f; // #57 플레이어 꼬리에 맞을 때 위로 차이는 힘


    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        
        enemyCtrl = GetComponent<EnemyCtrl>();      // #15
        boxCollider2D = GetComponent<BoxCollider2D>();

        anim = GetComponent<Animator>();            // #34

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // 오브젝트 이름도 LobbyManager이기 때문에
        
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
                        // Debug.Log("#11 플레이어가 Enemy 머리 밟음");    
                        playerCtrl.BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음   // #16 리팩토링: PlayerCtrl 변수 사용
                        enemystate = ENEMY_STATE.DIE;  //#9 리팩터링
                        IsDieByBeingTrampled();            // #19 죽었을 때 효과
                    }
                    break;
                
                case EnemyCtrl.ENEMY_TYPE.TURTLE : 
                    if(beStepped)  // #15 만약 플레이어가 Enemy의 머리를 밟은 거라면
                    {
                        if(enemyCtrl.wingsType == EnemyCtrl.WINGS_TYPE.YES) // #34 밟았는데, 그게 날개 달린 거북이었다면
                        {
                            enemyCtrl.wingsType = EnemyCtrl.WINGS_TYPE.NO;  // 날개 없애기
                            anim.SetBool("Fly", false);                     // 날아다니는 애니메이션 취소

                            break;
                        }

                        Debug.Log("#11 플레이어가 Enemy 머리 밟음");
                        playerCtrl.BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음   // #16 리팩토링: PlayerCtrl 변수 사용
                        IsDieByBeingTrampled();               // #15 등껍질로 변신
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
                    IsDieByBeingTrampled();
                    break;
            }

            // else
            //     Debug.Log("#11 플레이어랑 그냥 부딪힘");

        }        
    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("//#57 트리거 발생");

        if(other.gameObject.tag == "PlayerTail" && !getHitByTail)    // #57
        {
            Debug.Log("//#57 꼬리에 맞음");
            getHitByTail = true;
            HitByTail(other.gameObject.transform.position);               
        }
    }

    private void IsDieByBeingTrampled()             // # 밟혀서 죽을 때                
    {
        GameMgr.Mgr.score += 100;                   // #30 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
        lobbyManager.CheckPoint();                  // #35 포인트 확인용

        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :      // #19 죽을 때 - 굼바는 찌그러짐
                rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;
                
                body.SetActive(false);              // 기존 바디 비활성화
                trampledBody.SetActive(true);       // 밟힌 이미지 활성화

                Invoke("DestroyEnemy", 0.3f);       // 1초 후 소멸

                ShowPointUi();                      // #19 획득 점수 표시

                break;

            case EnemyCtrl.ENEMY_TYPE.TURTLE :      // #15 등껍질로 변신

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;

                ChangeTurtleToShell();              // #57 중복해서 쓸 것 같아서 함수화 해버림

                ShowPointUi();                      // #19 획득 점수 표시

                break;
            
            case EnemyCtrl.ENEMY_TYPE.SHELL :       // #19 등껍질 밟거나 차면 3초 후 소멸
                rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록

                Invoke("DestroyEnemy", 2.0f);   
                break;
        }
    }

    private void HitByTail(Vector3 _pos)  // #57 꼬리에 맞을 때
    {
        switch(enemyCtrl.enemyType)     // 꽃 Enemy만 제외하고 실행할 내용 (위로 튀어오르기, 상태 뒤집기)
        {   
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :  // 굼바
            case EnemyCtrl.ENEMY_TYPE.TURTLE :  // 거북
            case EnemyCtrl.ENEMY_TYPE.SHELL :   // 거북 등껍질
            // 위로 튕기기
                if(_pos.x < this.gameObject.transform.position.x)    // 플레이어가 Enemy의 왼쪽에 있을 때
                {
                    rBody.AddForce(Vector2.right * 50f);    
                }
                else
                {
                    rBody.AddForce(Vector2.left * 50f);
                }
                rBody.AddForce(Vector2.up * hitForce);  // 플레이어 위치에 따라 위로 올라갔다가 추락함
                break;

        }


    // 기타 설정 (포인트 획득, 상태 뒤집기, 죽음, 껍질로 변신)
        Vector3 theScale = transform.localScale;
        Debug.Log("//#57 y값 : " + transform.localScale.y);

        switch(enemyCtrl.enemyType) 
        {              
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :  // 굼바가 꼬리에 맞았을 때 : 꼬리로 맞자마자 죽음
                Debug.Log("//#57 굼바 꼬리에 맞음");
                enemystate = ENEMY_STATE.DIE;
                
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;
            // 콜라이더 없애기
                boxCollider2D.enabled = false;      // 콜라이더 비활성화 - 땅으로 꺼지도록

                GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
                lobbyManager.CheckPoint();          // #35 포인트 확인용
                Invoke("DestroyEnemy", 3f);       // 3초 후 소멸
                ShowPointUi();                      // #19 획득 점수 표시

                break;
                
            case EnemyCtrl.ENEMY_TYPE.FLOWER :  // 꽃이 꼬리에 맞았을 때 : 꼬리로 맞자마자 죽음

                enemystate = ENEMY_STATE.DIE;

                GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
                lobbyManager.CheckPoint();          // #35 포인트 확인용
                Invoke("DestroyEnemy", 0.5f);       // 0.5초 후 소멸
                ShowPointUi();                      // #19 획득 점수 표시

                break;
            
            case EnemyCtrl.ENEMY_TYPE.TURTLE :  // 거북이 꼬리에 맞았을 때 : 꼬리에 맞자마자 죽지는 않고, 거북 등 껍질만 뒤집혀 있음/ 위로 튕겼다가 아래로 떨어짐
                Debug.Log("//#57 거북 꼬리에 맞음");
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;
                Debug.Log("//#57 y값 : " + transform.localScale.y);

                ChangeTurtleToShell();


                break;

            case EnemyCtrl.ENEMY_TYPE.SHELL :   // 등껍질이 플레이어의 꼬리에 맞았을 때 : 그 모습 그대로 잠깐 위로 튕겼다가 아래로 떨어짐. 탄성 약간 O
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;

                break;
        } 

    }

    private void DestroyEnemy() // #16 Enemy 소멸
    {
        Destroy(this.gameObject);
    }

    private void ShowPointUi()  // #19 획득 점수 표시
    {
        Vector3 pointPos;
        pointPos = transform.position;
        pointPos.y +=1f;

        Instantiate(pointUi, pointPos, Quaternion.identity);
    }

    private void ChangeTurtleToShell()  
    {
        body = transform.GetChild(0).gameObject;
        trampledBody = transform.GetChild(2).gameObject;
        body.SetActive(false);              // 기존 바디 비활성화
        trampledBody.SetActive(true);       // 등껍질 이미지 활성화

        Vector2 size = boxCollider2D.size;  // 등껍질로 사이즈 맞추기
        size.y = 0.7f;
        boxCollider2D.size = size;

        enemyCtrl.enemyType = EnemyCtrl.ENEMY_TYPE.SHELL;   // #16 밟으면 상태 변화
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
