using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour  // #11 적 머리 밟았을 때, 적을 죽이는 기능
{
    public enum ENEMY_STATE {IDLE = 1, DIE};    // #9 Enemy의 상태
    public ENEMY_STATE enemystate = ENEMY_STATE.IDLE;

    private PlayerCtrl playerCtrl;
    private PlayerLife playerLife;

// #15 플레이어에게 머리 밟혔는지 확인용 & 등껍질로 변신하도록
    public bool beStepped = false;          // PlayerCtrl에서 true, false 적용됨
    public bool shellBeStepped = false;     // #30 보완
    private bool getHitByTail = false;      // #57 꼬리에 한번만 치이도록 하기 위한 bool형 변수
    // private bool followPlayer = false;   // #64
    private bool caughtByPlayer = false;    // #64 플레이어에게 잡혀있는 상태인지 확인 - ENEMY_TYPE.SHELL 에게만 해당
    private Vector3 offset;                 // #64 플레이어를 따라다니는 라이프 바의 offset

    private Transform playerTransform;      // #64
    private EnemyCtrl enemyCtrl;    // #15
    private GameObject trampledBody;       // #15
    private GameObject body;        // #15
    private BoxCollider2D boxCollider2D; // #15
    private CircleCollider2D circleCollider2D;  // #16 보완

    private Rigidbody2D rBody;      // #19

    public AudioClip tailHitClip;   // #63

// #19 몬스터 죽인 후 등장하는 PointUi
    public GameObject pointUi;
    public GameObject bombUi;   // #57 꽃 Enemy 죽을 때 나타나는 폭탄 모양
    public GameObject hitUi;    // #63 꼬리로 죽일 때 나타나는 연기 모양
    public GameObject shellHitUi;   // #58 껍질에 맞아 죽을 때 나타나는 연기 모양
    
// #34 몬스터 애니메이션
    private Animator anim;
// #35
    private LobbyManager lobbyManager;           // #35 점수 체크용

    private float tailHitForce = 5000f;     // #57 플레이어 꼬리에 맞을 때 위로 차이는 힘
    private float shellHitForce = 3000f;    // #58 거북 껍질에 맞을 때 위로 차이는 힘

    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;         // #64

        enemyCtrl = GetComponent<EnemyCtrl>();      // #15
        boxCollider2D = GetComponent<BoxCollider2D>();

        anim = GetComponent<Animator>();            // #34

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // 오브젝트 이름도 LobbyManager이기 때문에
        
        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :
                rBody = GetComponent<Rigidbody2D>();    // #19
                break;
            case EnemyCtrl.ENEMY_TYPE.TURTLE : 
                rBody = GetComponent<Rigidbody2D>();    // #19
                circleCollider2D = GetComponent<CircleCollider2D>();    // #16 보완
                break;
        }
    }

    private void Start()
    {
        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.TURTLE : // #16 보완
                if(circleCollider2D.enabled)
                {
                    circleCollider2D.enabled = false;   // 써클 콜라이더 - 만약 활성화 되어 있다면, 비활성화로 시작하도록
                }
                break;
        }
    }
    private void Update()
    {
        // if(followPlayer)    // #64 플레이어 따라다니기 시작되면
        // {
        //     transform.position = playerTransform.position + offset;
            
        //     if(!playerCtrl.pressingX)   // 거북 껍질 들고 있는데 X 버튼 놓는다면
        //     {
        //         followPlayer = false;
        //     }
        // }

        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.SHELL :

                if(Input.GetKeyUp(KeyCode.X))   // #64 누르고 있던 X키를 놓았을 때
                {
                    // Debug.Log("//#64 X키 떼었다");
                    
                    if(caughtByPlayer)
                    {
                        caughtByPlayer = false;
                        PlayerReleasing();      // 놓여짐
                    }
                }
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
                        if(enemyCtrl.wingsType == EnemyCtrl.WINGS_TYPE.YES) // #34 밟았는데, 그게 날개 달린 거북이었다면
                        {
                            enemyCtrl.wingsType = EnemyCtrl.WINGS_TYPE.NO;  // 날개 없애기
                            anim.SetBool("Fly", false);                     // 날아다니는 애니메이션 취소

                            Debug.Log("#11 #16보완 플레이어가 Enemy 머리 밟음 - 위로 BounceUp");
                            playerCtrl.BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음   // #16 리팩토링: PlayerCtrl 변수 사용

                            beStepped = false;  // #34 날개 달린 Enemy의 beStpped =false로 해서 완전히 원래 거북으로 돌아가도록

                            break;
                        }
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


                            beStepped = false;  // #34 날개 달린 Enemy의 beStpped =false로 해서 완전히 원래 거북으로 돌아가도록

                            break;
                        }
                        Debug.Log("#11 #16보완 플레이어가 Enemy 머리 밟음 - 위로 BounceUp"); // #34 변경 - 날개 없는 거북 밟아도 플레이어 위로 Bounce 되도록
                        playerCtrl.BounceUp(); // #16 Enemy의 머리 밟으면 플레이어는 약간 위로 튀어오르기 - Shell을 밟았을 땐 튀어오르지 않음   // #16 리팩토링: PlayerCtrl 변수 사용

                        IsDieByBeingTrampled();               // #15 등껍질로 변신
                    }
                    break;

                case EnemyCtrl.ENEMY_TYPE.SHELL :   // #16 등껍질 밟았을 때 

                    if(playerCtrl.pressingX)    // #64 만약 X를 누르고 있는 상태였다면 - 거북 껍질 들어야지
                    {
                        // followPlayer = true;    // 플레이어 옆에 붙어있도록
                        PlayerHolding();    // #64
                        break;
                    }

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
                    gameObject.tag = "ShellWeapon"; // #58 태그 변경
                    // enemystate = ENEMY_STATE.DIE;  //#9 리팩터링 -> #34 fix: 바로 DIE 처리하면, 거북 껍질 안 날라가(EnemyCtrl의 FixedUpdate) -> 소멸되기 직전에 DIE처리해주자
                    IsDieByBeingTrampled();
                    break;
            }

            // else
            //     Debug.Log("#11 플레이어랑 그냥 부딪힘");

        }        

        if(other.gameObject.tag == "ShellWeapon")   // #58
        {
            HitByShell(other.gameObject.transform.position);    // 함수 인자 : 거북 껍질의 위치
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log("//#57 트리거 발생");

        if(other.gameObject.tag == "PlayerTail" && !getHitByTail)    // #57
        {
            Debug.Log("//#57 꼬리에 맞음. 꼬리 위치는 " + other.gameObject.transform.position.y);
            getHitByTail = true;
            HitByTail(other.gameObject.transform.position);               
        }
    }

    private void IsDieByBeingTrampled()             // # 밟혀서 죽을 때                
    {

        switch(enemyCtrl.enemyType)
        {
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :      // #19 죽을 때 - 굼바는 찌그러짐
                rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록 
                Debug.Log("/#34 플레이어 - 껍질 밟았다!");

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;
                
                body.SetActive(false);              // 기존 바디 비활성화
                trampledBody.SetActive(true);       // 밟힌 이미지 활성화

                enemystate = ENEMY_STATE.DIE;       //#62

                Invoke("DestroyEnemy", 0.3f);       // 1초 후 소멸

                GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때 100점씩 획득
                lobbyManager.CheckPoint();          // #35 포인트 확인용
                ShowPointUi();                      // #19 획득 점수 표시

                break;

            case EnemyCtrl.ENEMY_TYPE.TURTLE :      // #15 등껍질로 변신

                body = transform.GetChild(0).gameObject;
                trampledBody = transform.GetChild(2).gameObject;

                ChangeTurtleToShell();              // #57 중복해서 쓸 것 같아서 함수화 해버림

                GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때 100점씩 획득
                lobbyManager.CheckPoint();          // #35 포인트 확인용
                ShowPointUi();                      // #19 획득 점수 표시

                break;
            
            case EnemyCtrl.ENEMY_TYPE.SHELL :       // #19 등껍질 밟거나 차면 3초 후 소멸
                // rBody.velocity = new Vector2(0f, 0f);   // ((발로 찬 그 순간에는)) 가만히 움직이지 않도록 
                Debug.Log("/#34 플레이어 - 껍질 밟았다!");

                if(shellBeStepped)  // #30 보완 : 머리가 밟혀서 죽는 거라면 - 점수 획득 있음
                // #30 보완 : 밟혀서 죽는 게 아닌 방법으로 (옆에서 밀어서) 죽는 거라면 - 점수 획득 없음 
                {
                    GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때 100점씩 획득
                    lobbyManager.CheckPoint();          // #35 포인트 확인용
                    ShowPointUi();                      // #19 획득 점수 표시
                }
                // enemystate = ENEMY_STATE.DIE;           //#62 -> #34 fix: 바로 DIE 처리하면 껍질이 안 나라가 (EnemyCtrl의 FixedUpdate) -> 소멸되기 직전에 DIE 처리 해주자

                Invoke("DestroyEnemy", 2.0f);   
                break;
        }
    }

    private void HitByTail(Vector3 _pos)  // #57 꼬리에 맞을 때
    {
        
        AudioSource.PlayClipAtPoint(tailHitClip, transform.position);   // #63 꼬리로 맞으면 효과음 발생함  - 모든 Enemy 해당

        switch(enemyCtrl.enemyType)     // 꽃 Enemy만 제외하고 실행할 내용 (위로 튀어오르기, 상태 뒤집기)
        {   
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :  // 굼바
            case EnemyCtrl.ENEMY_TYPE.TURTLE :  // 거북
            case EnemyCtrl.ENEMY_TYPE.SHELL :   // 거북 등껍질

                if(_pos.x < this.gameObject.transform.position.x)    // 플레이어가 Enemy의 왼쪽에 있을 때
                {
                // #63 꼬리에 맞을 때 HitUi로 연기 효과 발생
                    Vector3 hitPos;
                    hitPos = transform.position;
                    hitPos.x -= 1f;
                    Instantiate(hitUi, hitPos, Quaternion.identity);
    
                // 위로 튕기기
                    rBody.AddForce(Vector2.right * 50f);    
                }
                else
                {
                // #63 꼬리에 맞을 때 HitUi로 연기 효과 발생
                    Vector3 hitPos;
                    hitPos = transform.position;
                    hitPos.x += 1f;
                    Instantiate(hitUi, hitPos, Quaternion.identity);

                // 위로 튕기기
                    rBody.AddForce(Vector2.left * 50f);
                }
                rBody.AddForce(Vector2.up * tailHitForce);  // 플레이어 위치에 따라 위로 올라갔다가 추락함
                break;
            case EnemyCtrl.ENEMY_TYPE.FLOWER : 
                if((!enemyCtrl.hideInPipe) && 
                    (enemyCtrl.destPos.y -0.5 < _pos.y) )   // #57 파이프에 숨어있지 않다면 && 플레이어 위치가 꽃 Enemy의 destPos 위치에 얼추 비슷하면- 공격 적용
                // (꽃 Enemy는 이미 큰 직사각형으로 Trigger 체크된 콜라이더 있기 때문에 - 그것에 공격 여부 판단이 방해되는 것을 막기 위함)
                {
                    ShowBombUi();    // #57 꽃 Enemy 사라질 때 폭탄 효과 표시
                }                        
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

            case EnemyCtrl.ENEMY_TYPE.FLOWER :  // 꽃이 꼬리에 맞았을 때 : 꼬리로 맞자마자 죽음
                if((!enemyCtrl.hideInPipe) && (enemyCtrl.destPos.y -0.5 < _pos.y) )   // #57 파이프에 숨어있지 않다면 && 플레이어 위치가 꽃 Enemy의 destPos 위치에 얼추 비슷하면- 공격 적용
                {
                    enemystate = ENEMY_STATE.DIE;

                    GameMgr.Mgr.score += 100;           // #30 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
                    lobbyManager.CheckPoint();          // #35 포인트 확인용
                    Invoke("DestroyEnemy", 0.5f);       // 0.5초 후 소멸
                    ShowPointUi();                      // #19 획득 점수 표시

                }

                break;
        } 

    }

    public void HitByShell(Vector3 _pos)   // #58 튕겨다니는 거북 껍질에 맞을 때    // #65 public 변환
    {
        Debug.Log("// #58 " + gameObject + " - Enemy가 거북 껍질에 맞음");
        
        enemystate = ENEMY_STATE.DIE;   // 일단 모두 죽은 상태로

        switch(enemyCtrl.enemyType)     // 꽃 Enemy만 제외하고 실행할 내용 (위로 튀어오르기, 상태 뒤집기)
        {   
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :  // 굼바
            case EnemyCtrl.ENEMY_TYPE.TURTLE :  // 거북
            case EnemyCtrl.ENEMY_TYPE.SHELL :   // 거북 등껍질
                Vector3 shellHitPos = this.gameObject.transform.position;    // #58 연기 나오는 위치
                
                if(_pos.x < this.gameObject.transform.position.x)    // 플레이어가 Enemy의 왼쪽에 있을 때
                {
                    shellHitPos.x -= 0.5f;
                    Instantiate(shellHitUi, shellHitPos, Quaternion.identity);

                    // 위로 튕기기
                    rBody.AddForce(Vector2.right * 50f);    
                }
                else
                {
                    shellHitPos.x += 0.5f;
                    Instantiate(shellHitUi, shellHitPos, Quaternion.identity);

                    // 위로 튕기기
                    rBody.AddForce(Vector2.left * 50f);
                }
                rBody.AddForce(Vector2.up * shellHitForce);  // 플레이어 위치에 따라 위로 올라갔다가 추락함
                break;
            case EnemyCtrl.ENEMY_TYPE.FLOWER : 
                ShowBombUi();   // 폭탄 나타내면서 사라지기
                break;
        }


    // 기타 설정 (포인트 획득, 상태 뒤집기, 죽음, 껍질로 변신)
        Vector3 theScale = transform.localScale;
        switch(enemyCtrl.enemyType) 
        {              
            case EnemyCtrl.ENEMY_TYPE.GOOMBA :  // 굼바가 꼬리에 맞았을 때 : 꼬리로 맞자마자 죽음
                
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;
            // 콜라이더 없애기
                boxCollider2D.enabled = false;      // 콜라이더 비활성화 - 땅으로 꺼지도록

                GameMgr.Mgr.score += 100;           // 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
                lobbyManager.CheckPoint();          // 포인트 확인용
                Invoke("DestroyEnemy", 3f);         // 3초 후 소멸
                ShowPointUi();                      // 획득 점수 표시

                break;
            
            case EnemyCtrl.ENEMY_TYPE.TURTLE :  // 거북이 꼬리에 맞았을 때 : 꼬리에 맞자마자 죽지는 않고, 거북 등 껍질만 뒤집혀 있음/ 위로 튕겼다가 아래로 떨어짐
                Debug.Log("//#57 거북 꼬리에 맞음");
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;
            // 콜라이더 없애기
                boxCollider2D.enabled = false;      // 콜라이더 비활성화 - 땅으로 꺼지도록

                ChangeTurtleToShell();

                break;

            case EnemyCtrl.ENEMY_TYPE.SHELL :   // 등껍질이 플레이어의 꼬리에 맞았을 때 : 그 모습 그대로 잠깐 위로 튕겼다가 아래로 떨어짐. 탄성 약간 O
            // 상태 뒤집기
                theScale.y *= -1;
                transform.localScale = theScale;
            // 콜라이더 없애기
                boxCollider2D.enabled = false;      // 콜라이더 비활성화 - 땅으로 꺼지도록

                break;

            case EnemyCtrl.ENEMY_TYPE.FLOWER :  // 꽃이 꼬리에 맞았을 때 : 꼬리로 맞자마자 죽음

                // enemystate = ENEMY_STATE.DIE;    // #62 이미 함수 내 맨 위에서 DIE 처리 했음

                GameMgr.Mgr.score += 100;           // 굼바, 거북, 껍질 모두 밟을 때/ 찰 때 100점씩 획득
                lobbyManager.CheckPoint();          // 포인트 확인용
                Invoke("DestroyEnemy", 0.5f);       // 0.5초 후 소멸
                ShowPointUi();                      // 획득 점수 표시

                break;
        } 
    
    }

    private void DestroyEnemy() // #16 Enemy 소멸
    {
        enemystate = ENEMY_STATE.DIE;   // #34 fix: 소멸되기 직전에 DIE 처리해야 거북 껍질이 날라가서.. 문제 해결을 위해 여기서 처리해보자
        Destroy(this.gameObject);
    }

    private void ShowPointUi()  // #19 획득 점수 표시
    {
        Debug.Log("//#30 보완 : 점수 획득");
        Vector3 pointPos;
        pointPos = transform.position;
        pointPos.y +=1f;

        Instantiate(pointUi, pointPos, Quaternion.identity);
    }

    private void ShowBombUi()  // #57 폭탄 표시
    {
        Debug.Log("//#57 폭탄 표시");
        transform.GetChild(0).gameObject.SetActive(false); // 기존 바디 안 보이도록

        Vector3 bombPos;
        bombPos = transform.position;
        bombPos.y +=1f;

        Instantiate(bombUi, bombPos, Quaternion.identity);
    }

    private void ChangeTurtleToShell()  
    {
        body = transform.GetChild(0).gameObject;
        trampledBody = transform.GetChild(2).gameObject;
        body.SetActive(false);              // 기존 바디 비활성화
        trampledBody.SetActive(true);       // 등껍질 이미지 활성화

        // Vector2 size = boxCollider2D.size;  // 등껍질로 사이즈 맞추기
        // size.y = 0.7f;
        // boxCollider2D.size = size;
        boxCollider2D.enabled = false;      // #16 보완 : 위 코드 주석하고, 써클 콜라이더 이용하기
        circleCollider2D.enabled = true;    

        enemyCtrl.enemyType = EnemyCtrl.ENEMY_TYPE.SHELL;   // #16 밟으면 상태 변화

        Invoke("ChangeShellToTurtle", 7f);  // #62 - 7초 후 부활 시도
    }

    private void ChangeShellToTurtle()  // #62 껍질에서 다시 거북으로 변화 (일정 시간(7초) 지나면 실행되도록)
    {
        if(enemystate != ENEMY_STATE.DIE)
        {
            Debug.Log("//#62 껍질 -> 거북");

            body = transform.GetChild(0).gameObject;
            trampledBody = transform.GetChild(2).gameObject;
            trampledBody.SetActive(false);       // 등껍질 이미지 비활성화
            body.SetActive(true);              // 기존 바디 활성화

            // Vector2 size = boxCollider2D.size;  // 등껍질로 사이즈 맞추기
            // size.y = 1.164f;
            // boxCollider2D.size = size;
            circleCollider2D.enabled = false;       // #16 보완 : 위 코드 주석하고, 써클 콜라이더 이용하기
            boxCollider2D.enabled = true;      

            enemyCtrl.enemyType = EnemyCtrl.ENEMY_TYPE.TURTLE;   

            if(caughtByPlayer)          // #64 만약 껍질 -> 거북으로 변하려고 하는데 - 껍질이 플레이어에게 잡혀있는 상태라면 - 놓아지도록
            {
                caughtByPlayer = false;
                PlayerReleasing(true);      // 놓여짐
                playerLife.GetHurt();
            }
        }
    }

    private void PlayerHolding()    // #64
    {
        playerCtrl.HoldingShell(true);  // #65 플레이어가 껍질 잡음
        caughtByPlayer = true;      // 플레이어에게 잡혀있는 상태로 설정

        this.gameObject.layer = 17; // 플레이어와 충돌 일어나지 않도록 레이어 변경 (PlayerHolding)

        // Debug.Log("//#64 플레이어 - Shell 들고다니기/ 위치는 : " + transform.position + "//" + transform.localPosition);
        rBody.simulated = false;    // rBody가 살아있어서 자꾸 플레이어를 안 따라오는 것 같아서

        // rBody.velocity = new Vector2(0f, 0f);   // 가만히 움직이지 않도록

        // rBody.gravityScale = 0;
        // rBody.mass=0;
        transform.SetParent(playerTransform);

        // if(playerCtrl.dirRight) // 플레이어가 오른쪽을 보고있다면 껍질도 오른쪽으로 들고 있도록
        offset = new Vector3(0.7f, 0, 0);   
        // else
        //     offset = new Vector3(-0.7f, 0, 0);
            
        transform.localPosition = offset;
    }

    public void PlayerReleasing(bool timeOver = false)  // #64  거북 껍질 손에서 놓기 // 직접 놓은 건지 or 시간 지나서 놓여진 건지 상황 구분    // #65 public 변경
    {
        playerCtrl.HoldingShell(false);  // #65 플레이어가 껍질 놓음

// timeOver가 아니면, 자동으로 OnCollisionEnter2D에서 거북 껍질과 부딪히는 걸로 판단돼서 - 거북 껍질 발로 차는 것처럼 연출됨
        // Debug.Log("//#64 플레이어가 Shell 놓음");

        if(timeOver)    // 시간이 오버돼서 껍질을 놓게 되면 - 플레이어로부터 조금 먼 곳에 떨어지도록
        {   
            offset = new Vector3(1.5f, 0, 0);       // 조금 먼 곳에서 껍질 떨어뜨리도록
            transform.localPosition = offset;
        }

        Debug.Log("//#64 거북이 다시 부모 null");
        transform.parent = null;    // 플레이어에게 잡힌 상태 -> 다시 독립적인 상태로 변경

        rBody.simulated = true;     // 자유롭게 움직이도록

        this.gameObject.layer = 13; // 이제 플레이어랑 부딪힐 수 있음

        if(timeOver)
     {
        if(playerCtrl.dirRight)   // 나아가는 방향 설정
        {
            Debug.Log("//#64 거북이 오른쪽으로 풀어주기");

            // 플레이어가 오른쪽을 보고 있다면 거북도 오른쪽으로 가도록
            enemyCtrl.enemyDir = 1; 

            if(transform.localScale.x > 0)   // 근데 왼쪽 방향 바라보고 있다면(스케일 값이 양수) - 뒤집도록
            {
                Debug.Log("//#64 거북이 방향 조정1");
                Vector3 enemyScale = transform.localScale;
                enemyScale.x *= -1;
                transform.localScale = enemyScale;
            }
        }
        else
        {
            Debug.Log("//#64 거북이 왼쪽으로 풀어주기");
            // 플레이어가 왼쪽을 보고 있다면 거북도 왼쪽으로 가도록
            enemyCtrl.enemyDir = -1;

            if(transform.localScale.x < 0)   // 근데 오른쪽 방향 바라보고 있다면(스케일 값이 음수) - 뒤집도록
            {
                Debug.Log("//#64 거북이 방향 조정2");

                Vector3 enemyScale = transform.localScale;
                enemyScale.x *= -1;
                transform.localScale = enemyScale;
            }
        }

     }

        // if(!timeOver)
        //     enemyCtrl.kickShell = true; // #64 보완 : 그래야 발로 차는 것과 같은 현상 일어남 - 부모를 null로 만든 후에 true로 바꿔줘야 함.
        // 왜냐: 부모를 null로 바꾸기 전에 kickShell = true로 해주면, 부모가 플레이어인채로 날라다님
        // 애초에 해줄 필요가 없었네 - 부모를 null로 바꾸는 동시에, 플레이어와 부딪히며 kickShell = true로 바뀌게 됨



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
