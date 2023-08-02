using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour //#1 플레이어 컨트롤(움직임 관련)
{
    private PlayerLife playerLife;          // #17

    public Animator anim;                   // #36 플레이어 애니메이션 (접근 범위 변경)
    private Rigidbody2D Rbody;
    private BoxCollider2D boxCollider2D;    // #39 웅크릴 때 콜라이더 크기도 바뀌어야지

    private bool dirRight = true;           // 플레이어가 바라보는 방향(오른쪽 : 1, 왼쪽 : -1)

    private float moveForce = 30f;          // 이동 속도 (50 > 20)
    private float maxSpeed = 5f;            // 달리기 가속도. 최고 속도

    private bool pressingX = false;           // #32 빠르게 달리기 (X키)
    private bool runSlowDown = false;       // #41 함수 중복 실행 방지 목적의 bool형 변수
    private float normalRunSpeed = 5f;      // #32 원래 최고 속도
    private float maxRunSpeed = 15f;        // #32 더 빠르게 달리기 가속도. 최고 속도
    private bool playMaxRunClip = false;    // #40 빨리 달리는 소리 
    private float currSpeed;                // #41 현재 속도
    private float fallSpeed;                // 떨어질 때 속도

    private float jumpTimer;
    private float jumpTimeLimit = 0.25f;
    private bool isJumping;                 // 점프 가능한지 체크
    public float jumpForce = 70f;           // 점프 가속도. 누르는 동안 더해지는 높이
    public float minJump = 100f;            // 최소 점프 높이
    private float bounceJump =600f;         // 살짝 튀어오를 때 점프 높이 - 예 : Enemy 밟았을 때

    private bool grounded;                  // 땅 밟았는지 체크
    // public bool steppingOnEnemy;         // #11 적 밟았는지 확인   -> // #15로 변경
    public bool pushPButton;                // #27 P버튼 밟았는지 체크
    public Transform groundCheck;           // 땅 밟았는지 체크

    private bool fallDown;                  // 지금 추락하고 있는지 체크

// #42 날기(Fly)
    private bool isFlying;                  // 날고 있는지 체크
    private float flyTimeCheck = 0f;         
    private float flyTimeLimit = 5.0f;      
    IEnumerator enumerator;                 // 코루틴 지정용
    private float flyForce = 285f;          // #42
    // private float slowFallForce = 280f;     // #43
// #49 순간이동하기(Teleport)
    public bool isInUnderground = false;   // #48 지하에 있는지 체크
    private bool isTeleporting = false;    // #49 순간이동 하고 있는지 체크
    private float moveTimer = 0f;
    private float teleportInTimer = 1f;      // #49 순간이동 하는 타이머
    private float teleportOutTimer = 2f;     // #49 순간이동 하는 타이머

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 부드러운 움직임 적용
    Vector3 startInPos = new Vector3(0, 0, 0); // switch문에서 할당되지 않았다고 에러 뜨는 거 방지용
    Vector3 destInPos = new Vector3(0, 0, 0); // switch문에서 할당되지 않았다고 에러 뜨는 거 방지용
    Vector3 startOutPos = new Vector3(0, 0, 0); // switch문에서 할당되지 않았다고 에러 뜨는 거 방지용

// 오디오 ==================================
    public AudioClip jumpClip;
    public AudioClip raccoonTailClip;       // #43
    public AudioClip coinClip;              // 코인 획득 클립
    public AudioSource maxRunAudioSource;
    public AudioClip maxRunClip;            // #40 최고 속도로 달릴 때 사운드 클립
    public AudioClip teleportClip;          // #49 순간이동할 때 사운드 클립

    private Music music;                    // #53 게임 백그라운드 음악(BGM) 설정

// // 충돌 처리 - 점프할 땐, LargeBlock과 부딪히지 않도록   // #21 버그 수정 (콜라이더 위치를 최상위 부모로 바꿨으니, 레이어 변경 코드 대상도 최상위 부모로 수정 필요)
//     private GameObject level1Obj;
//     private GameObject level2Obj;
//     private GameObject level3Obj;

// #8 플레이어 X좌표 위치 제한
    private Vector3 playerPos;

// #28 임의의 점수 변수 - 코인 획득 시 점수 증가 
    private int score;
// #35
    private LobbyManager lobbyManager;           // #35 점수 체크용
    private FollowCamera followCam;              // #44 플레이어 따라오는 카메라


    void Awake()
    {
        playerLife = GetComponent<PlayerLife>();        // #17

        Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식

        anim = firstChild.GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody2D>(); // 레벨 바꿀 때, 변경해줘도 되니까~    // #7 수정 - 지금까지 자식 오브젝트 위치가 이동하고 있었음
        boxCollider2D = GetComponent<BoxCollider2D>();  // #39
        
        maxRunAudioSource = gameObject.AddComponent<AudioSource>(); // #40 오디오소스 없기 때문에, 추가해서 지정해줘야 함
        music = GameObject.FindGameObjectWithTag("Music").GetComponent<Music>();    // #53 BGM 설정

        groundCheck = firstChild.Find("groundCheck");   // 0번째 자식 오브젝트의 자식들 중에서 groundCheck를 찾기   // 레벨 바꿀 때, 이 값도 변경해야 할 듯

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // 오브젝트 이름도 LobbyManager이기 때문에
        followCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();    // #44
        
        // level1Obj = firstChild.gameObject;           // #21 버그 수정
        // level2Obj = secondChild.gameObject;
        // level3Obj = thirdChild.gameObject;
    }
    
    void Update()
    {  
        CheckGroundCheck();

        // 점프 가속도   // 한번 스페이스바 누르면 > 최소 minJump만큼은 점프하도록
        if(Input.GetKeyDown(KeyCode.Z) && grounded && (playerLife.playerState != PlayerLife.MODE_STATE.HURT))     // #1 리팩토링 점프 키 변경 (Input.GetButtonDown("Jump")) -> Input.GetKeyDown(KeyCode.Z)
        {
            isJumping = true;
            Rbody.AddForce(Vector2.up * minJump);                       // 위로 
            // anim.SetTrigger("Jump");                                    // 애니메이션
            AudioSource.PlayClipAtPoint(jumpClip, transform.position);  // 효과음
        }
// 하늘 날기 (레벨3)
        if(Input.GetKeyDown(KeyCode.Z) && !grounded && isFlying && (playerLife.playerLevel == PlayerLife.MODE_TYPE.LEVEL3))  // #42
        {
            // Debug.Log("//#42 위로! ");
            Rbody.AddForce(Vector2.up * flyForce);
            AudioSource.PlayClipAtPoint(raccoonTailClip, transform.position);  // 효과음

            anim.SetTrigger("PressingZ");   // 더 위로 올라가는 애니도 함께 작용
        }
        if((anim.GetBool("Fly")) && grounded && isFlying && (playerLife.playerLevel == PlayerLife.MODE_TYPE.LEVEL3)) //#45 날고 있어야 하고, 땅에 닿는데, bool형이 true라면 잠시 애니도 false로 
        {
            anim.SetBool("Fly", false);
        }
        else if((!anim.GetBool("Fly")) && !grounded && isFlying && (playerLife.playerLevel == PlayerLife.MODE_TYPE.LEVEL3)) // #45 날고 있어야 하고, 땅에도 닿지 않아 있는데, bool형이 false라면 - 날고 있는 애니 true
        {
            anim.SetBool("Fly", true);
        }

        //# 44 하늘을 날 때, 날지 않을 때 카메라 위치 조정
        if((isFlying && transform.position.y > 2) && (!isInUnderground) )
            followCam.SetMaxY(12f);
        else if((!isFlying && transform.position.y < -5.7) && (!isInUnderground) )
            followCam.SetMaxY(-3f);

        

// 느리게 떨어지기 (레벨3) #43
        if(Input.GetKeyDown(KeyCode.Z) && fallDown && !grounded && !isFlying && (playerLife.playerLevel == PlayerLife.MODE_TYPE.LEVEL3))
        {
            Debug.Log("//#43 느리게 떨어지기");
            // Rbody.AddForce(Vector2.up * slowFallForce);         // 너무 부자연스러움 - 아이템 기능에 맞지도 않을 뿐더러
            Rbody.velocity = new Vector2(Rbody.velocity.x, 0f);    // #43 느리게 떨어지도록 - 속도0으로

            anim.SetTrigger("PressingZ");
            AudioSource.PlayClipAtPoint(raccoonTailClip, transform.position);  // 효과음
        }

        if(fallDown && (gameObject.layer != 11))   // 추락하고 있을 땐, 다시 부딪히는 레이어로 변경 // #21 버그 수정
        {
            gameObject.layer = 11;  // "FallDownPlayer" 레이어
            // level1Obj.layer = 11;   
            // level2Obj.layer = 11;
            // level3Obj.layer = 11;
        }
        else if(!fallDown && (gameObject.layer != 10))
        {
            gameObject.layer = 10;  // 추락하지 않는 동안에는 큰 블록들(Layer : LargeBlock) 그냥 통과하도록 -> 블록 자체가 아니라, ForegroundCollider라는 이름의 오브젝트를 만들어서 설정해줌
                                    // 왜냐하면, 추락하지 않을 때 블록을 통과하게 하면, 그냥 블록 위를 걸어다닐 때 밑으로 통과할 수 있기 때문.
            // level1Obj.layer = 10;   // "Player" 레이어
            // level2Obj.layer = 10;
            // level3Obj.layer = 10;
        }

        if(Input.GetKey(KeyCode.X)) // #32 X키 누르고 있는 동안은 달리는 속도 더 빨라지도록
            pressingX = true;
        else                        // 키를 누르고 있지 않다면
        {
            pressingX = false;

            if(playMaxRunClip && !isFlying)      // X키 안누르고 있고 && 날고 있는 상태도 아닌데, 빨리 달릴 때 클립 나오고 있다면
            {
                playMaxRunClip = false; // #40 빨리 달릴 때 효과음 중단

                maxRunAudioSource.Stop();
            }
        }

        if(Input.GetKey(KeyCode.Z) && ! (anim.GetBool("PressingZ")))     // #45
            anim.SetBool("PressingZ", true);
        else if(Input.GetKey(KeyCode.Z) && (anim.GetBool("PressingZ")))
            anim.SetBool("PressingZ", false);


        if(playerLife.playerLevel != PlayerLife.MODE_TYPE.LEVEL1)   // #39 아래 방향키를 누르고 있는 동안은 웅크리도록 (단, 레벨2, 레벨3에서만)
        {
            Vector2 size = boxCollider2D.size;  // 웅크리니까 콜라이더 크기도 변경

            if(Input.GetKey(KeyCode.DownArrow)) 
            {
                anim.SetBool("CurlUp", true);

                size.y = 0.9f;
                boxCollider2D.size = size;
            }
            else
            {
                anim.SetBool("CurlUp", false);
                
                size.y = 1.6f;
                boxCollider2D.size = size;
            }
        }
        
    }

    void FixedUpdate()
    {   
        if(transform.position.x < -6.65)    // #8 맵 기준으로 왼쪽 맨 끝까지 갈 수 없도록
        {
            playerPos = transform.position;
            playerPos.x = -6.65f;
            transform.position = playerPos;

            Rbody.velocity = new Vector2(0f, Rbody.velocity.y);    // #40 맨 끝에 도달하면 속도 0으로 떨어지도록
        }

        if(transform.position.y > 18)    // #42 갈 수 있는 범위 제한
        {
            playerPos = transform.position;
            playerPos.y = 18f;
            transform.position = playerPos;
        }

    //달리기 가속도 ===============================
        float h = Input.GetAxis("Horizontal");  // 좌우 키
        anim.SetFloat("RunSpeed", Mathf.Abs(h));   // #37 속도 적용되도록 - 애니메이션 적용
    
        currSpeed = h*Rbody.velocity.x;         // #41 속도에 따라 속도 표시계 다르게 나타나도록    
    
        fallSpeed = Rbody.velocity.y;           // #38 점프(떨어질 때) 속도
        anim.SetFloat("JumpSpeed", fallSpeed);  // 양수, 음수 모두 각각 다르게 작동해야 하므로
        
    // #46 급 방향 전환
        if(((Rbody.velocity.x>0) && (h<0)) || (Rbody.velocity.x<0) && (h>0))    // 움직이는 방향과 방향키의 방향이 다르다면
        {
            if(/*(currSpeed < -0.1) &&*/ !anim.GetBool("SuddenChangeDir"))       // #46 만약 급 방향전환 상태라면(Flip을 하기 전, 속도가 꽤 높은 편이라면) 
                                // - 움직이는 방향과 다른 방향의 버튼을 누르는 상태이므로 음수(-)/ 방향 바꾸는 상태이므로 상대적으로 값이 작음을 고려
            {
                // Debug.Log("// #46 급 방향전환");
                // anim.SetTrigger("SuddenChangeDir");
                anim.SetBool("SuddenChangeDir", true);
            }
        }
        else if(((Rbody.velocity.x>0) && (h>0)) || ((Rbody.velocity.x<0) && (h<0))) // 움직이는 방향과 방향키의 방향이 같다면
        {
            if(anim.GetBool("SuddenChangeDir"))
            {
                anim.SetBool("SuddenChangeDir", false);
            }
        }



    // #37 플레이어 이미지 뒤집기
        if(((h>0) && !dirRight) || ((h<0) && dirRight) // 움직이는 방향과 바라보는 방향이 다르다면
            /* && !anim.GetBool("SuddenChangeDir")*/ && (currSpeed > -0.1))  // 속도가 어느정도 높아진 후라면
        {
            // Debug.Log("//#46 현재 속도 : " + currSpeed);
            Flip();
        } 

    // 빨리 달리기 ================================
        if(pressingX)                       // #32 더 빠르게 달리도록 최고 속도 높이기
        {
            // Debug.Log("//#31 더 빠르게");
            maxSpeed = maxRunSpeed;     
        }
        else    // X키 누르고 있지 않다면
        {
            maxSpeed = normalRunSpeed;
        }

    //#41 속도 표시계 설정 =========================

        if(pressingX && ((Input.GetKey(KeyCode.LeftArrow)) || Input.GetKey(KeyCode.RightArrow)))        // #41 X키를 누름과 동시에 좌우 한쪽 방향으로 향하고 있다면
        {
            if(currSpeed < maxRunSpeed * 1/7)
                lobbyManager.SetSpeedUp(-1);          // 아무 표시도 들어오지 않도록
            else if(currSpeed < maxRunSpeed * 2/7)
                lobbyManager.SetSpeedUp(0);           // 0번째 표시는 들어오도록
            else if(currSpeed < maxRunSpeed * 3/7)
                lobbyManager.SetSpeedUp(1);
            else if(currSpeed < maxRunSpeed * 4/7)
                lobbyManager.SetSpeedUp(2);
            else if(currSpeed < maxRunSpeed * 5/7)
                lobbyManager.SetSpeedUp(3);
            else if(currSpeed < maxRunSpeed * 6/7)
                lobbyManager.SetSpeedUp(4);
            else if(currSpeed >maxRunSpeed * 6/7)
                lobbyManager.SetSpeedUp(5, true);
                

            runSlowDown = false;                     // #41 빠르게 달릴 때에만 false로 해제해주기
        }
        // else if(pressingX && (!Input.GetKey(KeyCode.LeftArrow) && (!Input.GetKey(KeyCode.RightArrow))))
        // // X키는 누르고 있는데, 화살표는 안 누르고 있다면(어느 한 쪽으로 달려가고 있는 게 아니라면)
        // {
        //     if(!runSlowDown)
        //     {
        //         runSlowDown = true;
        //         lobbyManager.SetSpeedDown();
        //     }
        
        // }
        else    // #41 X키 누르지 않거나 OR 좌우 방향키 아무것도 안 누르고 있다면
        {
            if(!runSlowDown)              // if문 안의 함수를 딱 한번만 실행하기 위한 bool형 변수
            {
                runSlowDown = true;
                lobbyManager.SetSpeedDown();
            }
        }


        if(h*Rbody.velocity.x < maxSpeed) //최고 속도 도달하기 전이면, 속도 계속 증가
        {
            Rbody.AddForce(Vector2.right * h * moveForce); 
        }    

        if((h*Rbody.velocity.x < normalRunSpeed) && playMaxRunClip && !isFlying) // 속도 느려졌고, 날고 있는 상태도 아닌데, 빨리 달릴 때 클립 나오고 있다면
        {
            playMaxRunClip = false; // #40 빨리 달릴 때 효과음 중단

            maxRunAudioSource.Stop();
        }

        if((h*Rbody.velocity.x > normalRunSpeed) && playMaxRunClip) // 어느정도 빠르게 달리고 있고, 클립도 나오고 있다면
        {
    // #42 레벨3의 경우 5초동안 하늘 날기 가능 - Z키 누를 때마다(누르고 있는 상태는 취급 X -> GetKey가 아닌 GetKeyDown)
            if(Input.GetKeyDown(KeyCode.Z) && (playerLife.playerLevel == PlayerLife.MODE_TYPE.LEVEL3))
            {
                Debug.Log("//#42 날기 시작");
                isFlying = true;
                anim.SetBool("Fly", true);     // #45 isFlying = true와 함께 하늘을 나는 애니 시작

                enumerator = FlyStop();
                StopCoroutine(enumerator);     // #42 시작 전에 혹시나 실행중인 것이 있다면 종료하기
                StartCoroutine(enumerator);    // #42 5초 뒤에 날기 종료
            }
        }

        if(Mathf.Abs(Rbody.velocity.x) > maxSpeed)
        {   
            Rbody.velocity = new Vector2(Mathf.Sign(Rbody.velocity.x) * maxSpeed, Rbody.velocity.y);
            if(pressingX && !playMaxRunClip) // #40 X키 누르는 상태에서 최고 속도라면 && 효과음 아직 안 나오고 있다면
            {
                maxRunAudioSource.clip = maxRunClip;
                maxRunAudioSource.volume = 0.1f;        // 소리 너무 커.. 줄이자..
                maxRunAudioSource.Play();

                playMaxRunClip = true;
            }
        }    



        
    //점프 가속도 ===============================
        if(isJumping)
        {
            Rbody.AddForce(Vector2.up * jumpForce);
            jumpTimer += Time.deltaTime;

            if(fallDown)            // 블록->블록으로 점프하고 있는 경우 고려
                fallDown = false;   // 점프하고 있을 때 = 추락하고 있지 않을 때

            if(!Input.GetKey(KeyCode.Z) || (jumpTimer > jumpTimeLimit))   //점프 가속도 최대값 도달하면 -> 그 다음은 밑으로 추락  // // #1 리팩토링 점프 키 변경(Input.GetButton("Jump")) -> Input.GetKey(KeyCode.Z)
            {
                isJumping = false;
                jumpTimer = 0f;
            }
        }
        

        if(Rbody.velocity.y <0 && !fallDown)    // 추락하고 있을 때
        {
            fallDown = true;    
            // Debug.Log("#1 fallDown = true");
        }    
    }

    void Flip() // 플레이어 바라보는 방향 
    {
        Debug.Log("//#37 방향 바꾸기");
        // Debug.Log("뒤집어");
        dirRight = !dirRight;   //바라보는 방향 변경

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void CheckGroundCheck() // #11 따로 함수 추가. 
    {
        // 땅 밟았는지 체크
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Ground"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("LargeBlock"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Obstacle"));
        // Debug.Log("grounded : " + grounded);
        // steppingOnEnemy = Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Enemy"));  // #11   -> // #15로 변경

    }

    void OnTriggerEnter2D(Collider2D col) // #15 플레이어가 몬스터(굼바, 거북)의 headCheck를 밟았을 때
    {
        if(col.gameObject.tag == "EnemyHeadCheck")   
        {
            if(!col.gameObject.GetComponentInParent<EnemyLife>().beStepped) //  아직 beStepped가 true가 아니라면
            {
                // Debug.Log("//#15 플레이어가 Enemy 머리 밟음");
                col.gameObject.GetComponentInParent<EnemyLife>().beStepped = true;
            }
        }
        
        if(col.gameObject.tag == "ButtonHeadCheck") // #27 P버튼의 머리 부분에 부딪혔다면 (=밟았다면) && 아직 눌린 상태가 아니라면
        {
            GameObject parentObj = col.gameObject.transform.parent.transform.parent.gameObject;
            Debug.Log("//#27 bePushed는 true?: " + parentObj.GetComponent<Block>().bePushed);
            if(!parentObj.GetComponent<Block>().bePushed)
            {
                Debug.Log("//#27 P버튼 밟음");
                parentObj.GetComponent<Block>().bePushed = true;
                parentObj.GetComponent<Block>().PushButton();  

                GameObject[] blocks = GameObject.FindGameObjectsWithTag("FragileBlock");
                foreach(GameObject obj in blocks)
                {
                    obj.GetComponent<Block>().TurnsIntoCoin();  // #27 현재 존재하는 FRAGILE 블록들은 모두 코인으로 변하도록
                }
            }
        }

        if(col.gameObject.tag == "Coin")    // #28  코인 획득
        {
            Destroy(col.gameObject);        // 코인 사라져
            // score += 50;                 // 점수 획득
            GameMgr.Mgr.score += 50;        // #30 점수 획득
            lobbyManager.CheckPoint();      // #35 포인트 확인용

            AudioSource.PlayClipAtPoint(coinClip, transform.position);  // 효과음

        }
    
        if(col.gameObject.tag == "Goal")    // #53
        {
            col.gameObject.GetComponent<Goal>().ReachTheGoal(); // 플레이어가 골 지점에 닿았다!
            music.LevelCompleted(); // 게임 종료 BGM
        }
    }

    void OnTriggerStay2D(Collider2D col) // #47
    {
// #49 파이프 안으로 들어가기 =======================================
        if((col.gameObject.tag == "Teleport") && !isTeleporting) //#47 #49 아직 순간이동하는 중이 아니라면
        {            
            // Debug.Log("//#47 텔레포트 범위에 들어와있음");
            Teleport.WORKING_KEYDIR keyDir = col.gameObject.GetComponent<Teleport>().workingKeyDir; // #49 이동 방향 파악

            if( (keyDir == Teleport.WORKING_KEYDIR.DOWN) 
                && Input.GetKeyDown(KeyCode.DownArrow)) // 아래 화살표 누를 때 작동하는 텔레포트에서 && 아래 화살표 누르면
            {
                isTeleporting = true;   // #49 한번만 실행되도록 하기 위함

                Vector3 destPos = col.gameObject.GetComponent<Teleport>().StartTeleporting(); // 플레이어 순간이동

                StartCoroutine(TeleportInLerp(keyDir, destPos));

            }
            else if( (keyDir == Teleport.WORKING_KEYDIR.UP)
                && Input.GetKeyDown(KeyCode.UpArrow))   // 위 화살표 누를 때 작동하는 텔레포트에서 && 위 화살표 누르면
            {
                isTeleporting = true;   // #49 한번만 실행되도록 하기 위함

                Vector3 destPos = col.gameObject.GetComponent<Teleport>().StartTeleporting(); // 플레이어 순간이동

                StartCoroutine(TeleportInLerp(keyDir, destPos));            
                }
        }
// #49 파이프 밖으로 나오기 =======================================
        if((col.gameObject.tag == "Teleport") && isTeleporting)
        {
            Teleport.WORKING_KEYDIR keyDir = col.gameObject.GetComponent<Teleport>().workingKeyDir; // #49 출구인지 파악

            if(keyDir == Teleport.WORKING_KEYDIR.NONE)
            {
                isTeleporting = false;
                
                col.gameObject.GetComponent<Teleport>().SetCameraRange();   // 출구에 따라 카메라 위치, 범위 이동
            }

        }

    }

    void OnCollisionEnter2D(Collision2D col)  // #17 플레이어가 Enemy와 그냥 부딪혔을 때
    {
        if(col.gameObject.tag == "Enemy")
        {
            Debug.Log("//#17 플레이어가 Enemy랑 부딪힘. 다침");
            if(! col.gameObject.GetComponent<EnemyLife>().beStepped)
                playerLife.GetHurt();
        }

        // #50 아래 코드 - OnTriggerEnter2D로 이동
        // if(col.gameObject.tag == "Coin")    // #28  코인 획득
        // {
        //     Destroy(col.gameObject);        // 코인 사라져
        //     // score += 50;                 // 점수 획득
        //     GameMgr.Mgr.score += 50;        // #30 점수 획득
        //     lobbyManager.CheckPoint();      // #35 포인트 확인용

        //     AudioSource.PlayClipAtPoint(coinClip, transform.position);  // 효과음
        // }

    }

    public void BounceUp() // #16 약간 위로 튀어오르기 - 예 : 몬스터 밟았을 때
    {
        Rbody.AddForce(Vector2.up * bounceJump);
        Debug.Log("//#16 플레이어 살짝 위로 튀어오르기");
    }

    IEnumerator FlyStop()  // #42
    {
        flyTimeCheck = 0f;  // 날고 있는 시간 리셋

        while(flyTimeCheck < flyTimeLimit)
        {
            flyTimeCheck += Time.deltaTime;
            yield return null;
        }

        isFlying = false;
        anim.SetBool("Fly", false);     // #45 isFlying = false와 함께 하늘을 나는 애니도 끝

        Debug.Log("//#42 날기 멈춤");

    }

    IEnumerator TeleportInLerp(Teleport.WORKING_KEYDIR _keydir, Vector3 destOutPos)  // #49 
    {
        // boxCollider2D.enabled = false;      // 파이프 통과할 땐, 콜라이더 잠시 비활성화
        anim.SetBool("Teleport", true);     // 순간이동 중 - 플레이어 애니
        

//  갖가지 위치 지정
        switch(_keydir)
        {
            case Teleport.WORKING_KEYDIR.UP : 
            // 파이프 속으로 들어가는 위치 지정
                startInPos = transform.position;    
                destInPos = startInPos;
                destInPos.y += 0.5f;

            // 파이프 밖으로 나오는 위치 지정
                startOutPos = destOutPos; 
                startOutPos.y -= 0.5f;              // 아래에서 위로 이동 - 시작점은 더 아래에 있겠지

                break;

            case Teleport.WORKING_KEYDIR.DOWN : 
            // 파이프 속으로 들어가는 위치 지정
                startInPos = transform.position;    
                destInPos = startInPos;
                destInPos.y -= 0.5f;

            // 파이프 밖으로 나오는 위치 지정
                startOutPos = destOutPos; 
                startOutPos.y += 0.5f;              // 위에서 아래로 이동 - 시작점은 더 위에 있겠지

                break;
            // case Teleport.WORKING_KEYDIR.LEFT : 
            //     break;
            // case Teleport.WORKING_KEYDIR.RIGHT : 
            //     break;
        }

//  파이프 안으로 들어가기 =======================================
        AudioSource.PlayClipAtPoint(teleportClip, transform.position);  // 순간이동 효과음

        while(true)
        {
            if(moveTimer <= teleportInTimer)
            {
                transform.localPosition = Vector3.Lerp(startInPos, destInPos, curve.Evaluate(moveTimer/teleportInTimer));
            }
            else
            {
                StartCoroutine(TeleportOutLerp(_keydir, destOutPos));
                yield break;
            }

            moveTimer += Time.deltaTime;
            yield return null;
        }
    }

//  파이프 밖으로 나오기 =======================================
    IEnumerator TeleportOutLerp(Teleport.WORKING_KEYDIR _keydir, Vector3 destOutPos)
    {
        AudioSource.PlayClipAtPoint(teleportClip, transform.position);  // 순간이동 효과음

        while(true)
        {
            if(moveTimer <= teleportOutTimer)
            {
                transform.localPosition = Vector3.Lerp(startOutPos, destOutPos, curve.Evaluate(moveTimer/teleportOutTimer));
            }
            else
            {
                moveTimer = 0f; // 타이머 초기화
                // boxCollider2D.enabled = true;  // 파이프 통과하고 나서는, 다시 콜라이더 활성화
                // isTeleporting = false;  // 순간이동 종료 - 출구 나오는 순간, OnTriggerStay2D에서 false 처리되도록 만듦.
                anim.SetBool("Teleport", false);  // 순간이동 종료 - 플레이어 애니

                Rbody.velocity = new Vector2(Rbody.velocity.x, 0f);    // 느리게 떨어지도록 - 너무 빠른 가속도에 바닥으로 꺼지지 않도록
                yield break;
            }

            moveTimer += Time.deltaTime;
            yield return null;
        }
    }
}