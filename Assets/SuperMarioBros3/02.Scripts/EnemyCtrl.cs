using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.AI;

public class EnemyCtrl : MonoBehaviour  // #9 몬스터 움직임
{
// 몬스터 종류
    public enum ENEMY_TYPE {GOOMBA = 1, TURTLE, SHELL, FLOWER};
    public enum WINGS_TYPE {YES = 1, NO};    // 날개 달려 있는지

    public ENEMY_TYPE enemyType;
    public WINGS_TYPE wingsType;
    // #19 죽은 후 상태에 따른 함수 실행
    private EnemyLife enemyLife;
    // #34 몬스터 애니메이션
    private Animator anim;

// 몬스터 이동 ==========================
    public int enemyDir = -1;     // 오른쪽 : 1, 왼쪽 : -1 // 처음엔 왼쪽으로 이동
    public float moveSpeed = 2f;        // 이동 속도
    private Rigidbody2D rBody;      
    [SerializeField]
    private Transform frontCheck;   // #18 부딪혔을 때 이동 방향 바꾸도록 확인용
    
    private bool grounded;              // #33 땅 밟았는지 체크
    private Transform groundCheck;      // #33 땅 밟았는지 체크 - Enemy 각각 자기 자신의 그라운드체크 가져오기 
    private float turtleJumpForce = 13000f;   // #33 거북 - 날개가 달려서 점프하면서 다니는 Enemy
    private float goombaJumpForce = 1000f;    // #69 굼바 - 날개가 달려서 점프하면서 다니는 Enemy
    private float jumpTimeCheck = 0f;   // #69 0.1초마다 뛰도록 - Time.deltaTime 이용 (전 프레임이 완료되기까지 걸린 시간)
    private int jumpNum = 0;            // #69 점프한 횟수 - 날개 달린 굼바의 경우 3번 낮게 뛰고, 1번 높게 뛰기 때문에

// 꽃 ==========================
// #12 꽃 움직임
    private bool canMovingUp = true;               // 움직여도 되는지 확인용 bool 변수
    private bool isMovingUp = false;               // #12 위로 움직이고 있는지 확인용
    private bool isMovingDown = false;             // #12 이레러 음직이고 있는지 확인용
    [HideInInspector]
    public bool hideInPipe = true;      // #57 파이프 안에 숨었는지 체크
    private bool playerDir;             // #70 확인
    
    private float moveTimer = 0f;           // 현재 움직임 파악
    private float upDownTimer = 1f;         // 위로 올라와서 등장하는 데 1초 소모

    private IEnumerator flowerUpEnumerator;        // 코루틴 지정용
    private IEnumerator flowerDownEnumerator;
    private Vector3 startPos;
    [HideInInspector]
    public Vector3 destPos;             // #57 public으로 변경 - 꽃 Enemy의 공격 적용 여부 판단

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용  // 곡선 이용 - (0,0)에서 (1.1)로 가는 자연스러운 움직임 연출

    private Transform playerTransform;  // #13 꽃 Enemy - 쳐다보는 방향(좌/ 우) 플레이어 위치에 따라 방향 바꾸도록
    public Sprite lookingUp;            // #13 위 바라보는 이미지
    public Sprite lookingDown;          // #13 아래 바라보는 이미지
// #14 꽃 Enemy 파이어 볼 공격
    public Rigidbody2D fireball;        // #14 꽃 Enemy - 파이어볼 공격하기    // GameObject가 아닌 Rigidbody로 받는다. 속도 접근 쉽도록
    private float shootDist;            // 삼항연산자 - 플레이어와 Enemy 위치에 따라 다른 각도로 파이어 볼 쏘기
    private int shootHeight;            // 삼항연산자

// 거북 껍질 ==========================
// #16 거북 껍질 발로 차기
    public bool kickShell;
    private float kickSpeed = 18f;       // 플레이어가 발로 찼을 때 날라가는 속도 // fix: public으로 하면 초기 선언할 때 인스펙터 값이 우선 적용됨. private으로 하거나 확실히 TURTLE -> SHELL로 변할 때 값을 설정해주자.

// #75 게임 오버 체크
    private LobbyManager lobbyManager;           // #75 점수 체크용



    void Awake()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA :
            case ENEMY_TYPE.TURTLE : 
                rBody = GetComponent<Rigidbody2D>();    
                frontCheck = transform.GetChild(1).GetComponent<Transform>();   
                
                groundCheck = transform.Find("groundCheck");    // #33
                break;
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        enemyLife = GetComponent<EnemyLife>();
        anim = GetComponent<Animator>();        // #34

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // #75 오브젝트 이름도 LobbyManager이기 때문에
    }

    void Start()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA :
            case ENEMY_TYPE.TURTLE :   
                if(wingsType == WINGS_TYPE.YES) // #34 날개가 있는 거북이라면
                {
                    anim.SetBool("Fly", true);  // 처음부터 날개 애니메이션 실행    // #69 굼바도 적용
                } 

                moveSpeed = 2f;

                break;
            
            case ENEMY_TYPE.FLOWER : 
                startPos = transform.position;  // #12 시작 위치, 도착 위치 설정
                destPos = transform.position;
                destPos.y += 2.3f;
                moveSpeed = 3f;                 // #14 파이어볼 속도
                
                flowerUpEnumerator = FlowerUp();        // #12 fix 코루틴 지정
                StartCoroutine(flowerUpEnumerator);     // #12 꽃 움직임 시작

                break;
        }
    }

    void Update()
    {
        if((enemyLife.enemystate == EnemyLife.ENEMY_STATE.DIE)  //#9 리팩터링
            || lobbyManager.gameOver)   // #75 만약 게임오버 상태라면 - Enemy 움직임 멈추도록
            return;

// #33 #69 Enemy 점프 조정 =================================
        if(jumpTimeCheck < 0.1)
        {
            jumpTimeCheck += Time.deltaTime;
            return;
        }

        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
                CheckGroundCheck(); // #33
                // Debug.Log("//#69 땅 밟았나?: " + grounded);

                if(grounded && (wingsType == WINGS_TYPE.YES)) // 땅에 닿아있을 때에만 점프!
                {
                    jumpTimeCheck = 0;
                    grounded = false;

                    if(jumpNum <3)      //#69 점프 - 처음 3번은 낮게, 그 후 1번은 높게 뛰기
                    {
                        jumpNum++;
                        Jump(enemyType);
                    }
                    else
                        JumpHigh();
                }
                break;
            case ENEMY_TYPE.TURTLE : 
                CheckGroundCheck(); // #33
                // Debug.Log("//#69 땅 밟았나?: " + grounded);

                if(grounded && (wingsType == WINGS_TYPE.YES)) // 땅에 닿아있을 때에만 점프!
                {
                    jumpTimeCheck = 0;
                    grounded = false;

                    Jump(enemyType);
                }
                break;
        }

    }

    void FixedUpdate()
    {
        if(enemyLife.enemystate == EnemyLife.ENEMY_STATE.DIE)  //#9 리팩터링
            return;

        if(lobbyManager.gameOver)   // #75 만약 게임오버 상태라면 - Enemy 움직임 멈추도록
        {
            Debug.Log("//#75 EnemyCtrl - FixedUpdate에서 게임 오버 확인");

            if((anim != null) && (anim.enabled))   // Enemy3의 경우 anim이 없기 때문에 에러 방지 목적으로 anim!=null 조건 추가
            {
                anim.enabled = false;   // #75
                Debug.Log("//#75 EnemyCtrl - anim 멈춰");
            }    

            return;
        }    

        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
            case ENEMY_TYPE.TURTLE : 
                // #69 위치 변경 (switch문 이후 -> 이전으로)
                // if(enemyLife.enemystate == EnemyLife.ENEMY_STATE.DIE)  //#9 리팩터링
                //     return;

                rBody.velocity = new Vector2(enemyDir * moveSpeed, rBody.velocity.y);
                    // #9 Mathf.Sign : 부호를 반환하는 함수
                break;
            // case ENEMY_TYPE.SHELL :
            //     if(kickShell)
            //         rBody.velocity = new Vector2(enemyDir * kickSpeed, rBody.velocity.y);   // #16 한쪽 방향으로 날라가도록
            //     break;
            case ENEMY_TYPE.SHELL :
                if(kickShell)
                    rBody.velocity = new Vector2(enemyDir * kickSpeed, rBody.velocity.y);   // #16 한쪽 방향으로 날라가도록
                    // Debug.Log("//#16 속도 : " + rBody.velocity);
                // #16 fix -> EnemyLife.cs로 코드 이동 // if(enemyLife.beStepped) // #59
                //     Invoke("TurnToWeapon", 0.5f);   // 옆에서 민 다음에 무기로 바뀌도록 - 옆으로 미는 순간 플레이어가 다치는 걸 방지 
                break;
            case ENEMY_TYPE.FLOWER : 
                if((transform.position.y > destPos.y -1.5) && hideInPipe) // #57 어느정도 올라와있다면
                    hideInPipe = false;
                else if((transform.position.y < destPos.y -1.5) && !hideInPipe)   // #57 어느정도 내려갔다면
                    hideInPipe = true;


                CheckDirection(ENEMY_TYPE.FLOWER);       // #13 바라보는 방향 (좌/우)(위/아래) 체크
                break;
        }

    }

    public void Flip()
    {
        // Debug.Log("//#64 Enemy 뒤집힘");
        
        enemyDir *= -1;

        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;
    }
    
    void OnCollisionEnter2D(Collision2D col) 
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
            case ENEMY_TYPE.TURTLE : 
            case ENEMY_TYPE.SHELL : // #25 보완

                // if(Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Obstacle"))
                //     || Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Ground")))
                if(col.gameObject.tag == "Obstacle"    // #64 보완 : Tag 방식으로 변경// 너무 빠르게 부딪히고 튕겨서 Linecast를 잘 못 가져오는 것 같아서
                    || col.gameObject.tag == "FragileBlock")    // #34 추가: FragileBlock에 부딪혀도 Flip해야지
                {
                    Debug.Log("//#64 보완 : 장애물, 땅에 부딪힘");
                    Flip();

                    if(kickShell)   // #16 보완 : 발로 차인 거북 껍질이라면, 장애물에 부딪혔을 때 튕겨나가도록 AddForce 해주기 - 장애물에 끼는 일이 없도록
                    {
                        Debug.Log("//#64 보완 : kickShell 확인됨");

                        if(col.gameObject.transform.position.x < this.transform.position.x)
                        {
                            rBody.AddForce(Vector2.right * 50f);    // 너무 무거워서 안 튕기는 상황을 방지하기 위함
                            Debug.Log("//#16 //#64 보완 : 오른쪽으로 튕겨");
                        }
                        else
                        {
                            rBody.AddForce(Vector2.left * 50f);
                            Debug.Log("// #16 //#64 보완 : 왼쪽으로 튕겨");
                        }
                    }
                }

                break;

    // #25 보완 : 아래 코드는 Block.cs에서 실행되도록 - 그 방식이 더 적합하다고 생각 - 하나하나 col에 접근하는 것보다는, 처음부터 Block.cs에서 실행하는 게 낫지
            // case ENEMY_TYPE.SHELL :
            //     if(Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Obstacle"))
            //         || Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Ground")))
            //     {
            //         if(col.gameObject.tag == "FragileBlock")   // #25 SHELL의 frontCheck가 부숴지는 블록과 부딪힌 거라면 - 블록이 부숴지도록
            //         {
            //             col.gameObject.GetComponent<Block>().FragileBlockBroken();
            //         }
                    
            //         Flip();
            //     }
                
            //     break;
        }
    }
    private void OnTriggerEnter2D(Collider2D col)   
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.FLOWER : 
                if(col.gameObject.tag == "Player")  // #12 꽃 Enemy 주위에 플레이어가 있다면 꽃 등장 X 
                {
                    Debug.Log("#12 보완 플레이어가 꽃 가까이 들어왔다");
                    canMovingUp = false; // #12

                }
                break;
            case ENEMY_TYPE.GOOMBA :    // #66 굼바도 Flip 하도록
            case ENEMY_TYPE.TURTLE :
                if(col.gameObject.tag == "Cliff")   // #18 낭떠러지에서 이동 방향 바꾸도록
                {
                    Flip();
                }
                break;
        }



    }
    
    private void OnTriggerExit2D(Collider2D col) 
    {
        if(lobbyManager.gameOver)   // #75 게임 오버되면, 이동하지 않도록
            return;

        switch(enemyType)
        {
            case ENEMY_TYPE.FLOWER : 
                if(col.gameObject.tag == "Player")  // #12 플레이어가 다시 멀어졌으니, 올라오기 다시 시작해라
                {
                    // Debug.Log("#12 플레이어가 꽃 멀리 벗어났다");

                    flowerUpEnumerator = FlowerUp();        // #12 fix 코루틴 지정
                    flowerDownEnumerator = FlowerDown();    // #12 fix 코루틴 지정
                    
                    // #12 보완 : 아래 코드를 주석처리 하기 전 - 꽃 Enemy가 계속 중복해서 올라오고 내려오는 버그 발생했었음
                    // StopCoroutine(flowerUpEnumerator);      // 중복 실행을 막기 위해 코루틴 종료시킨 후, 새로 시작하기
                    // StopCoroutine(flowerDownEnumerator);

                    canMovingUp = true;
                    if(!isMovingUp && !isMovingDown)    // #12 보완 : 꽃이 올라오지도, 내려가지고 않고 있다면 직접 코루틴을 작동시켜주자
                        StartCoroutine(flowerUpEnumerator);   
 
                }
                break;
        }

    }
    IEnumerator FlowerUp() // #12 꽃 - 위로 올라오기
    {
        Debug.Log("//#12 보완 : 꽃 Enemy 올라온다");

        while(true && (!lobbyManager.gameOver))  // 올라가도 될 때에만 올라가도록   // #75
        {
            if(!canMovingUp)       // 움직이면 안 되는 상태라면 코루틴 아예 종료
            {
                yield return new WaitForSeconds(0.5f);
                moveTimer = 0f;

                flowerDownEnumerator = FlowerDown();       
                StartCoroutine(flowerDownEnumerator);      // 다시 내려가도록

                if(isMovingUp)  // #12 보완 : 올라오고 있지 않음을 체크
                    isMovingUp = false; 

                yield break;    // 현재 코루틴 종료
            }

            // Debug.Log("#12 업 함수 실행" + moveTimer + "// isMoving은 true? : " + isMoving);
            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                moveTimer = 0f;

                flowerDownEnumerator = FlowerDown();       // #12 fix 코루틴 지정
                StartCoroutine(flowerDownEnumerator);      // 다시 내려가도록

                if(isMovingUp)  // #12 보완 : 올라오고 있지 않음을 체크
                    isMovingUp = false; 

                yield break;    // 현재 코루틴 종료
            }
            
            if(!isMovingUp)     // #12 보완 : 올라오고 있음을 체크
                isMovingUp = true;

            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기
        }
    }

    IEnumerator FlowerDown()    // #12 꽃 - 밑으로 내려가기
    {
        ShootFireball();                   // #14 내려가기 직전에 파이어볼 쏘기

        Debug.Log("//#12 보완 : 꽃 Enemy 내려간다");

        while(true && (!lobbyManager.gameOver)) // #75
        {
            // Debug.Log("#12 다운 함수 실행" + moveTimer + "// isMoving은 true? : " + isMoving);
            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                moveTimer = 0f;

                if(canMovingUp)
                {
                    flowerUpEnumerator = FlowerUp();       // #12 fix 코루틴 지정
                    StartCoroutine(flowerUpEnumerator);    // 다시 올라오도록
                }
                // Debug.Log("//#12 다운 함수 종료");

                if(isMovingDown)    // #12 보완 : 내려가고 있지 않음을 체크
                    isMovingDown = false;

                yield break;    // 현재 코루틴 종료
            }

            if(!isMovingDown)   // #12 보완 : 내려가고 있음을 체크
                isMovingDown = true;    

            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기

        }
    }

    void CheckDirection(ENEMY_TYPE _type)
    {
        switch(_type)
        {
            case ENEMY_TYPE.FLOWER :    //#70 코드 위치만 조금 수정 - 같은 함수에 GOOMBA 케이스도 넣기 위해서
                playerDir = playerTransform.position.x > transform.position.x; // Enemy 기준으로 플레이어가 오른쪽에 있는지 확인

                if((!playerDir && (enemyDir == 1))  // Enemy 기준으로 플레이어가 왼쪽에 있고 && Enemy가 보는 방향이 오른쪽이라면
                    || (playerDir && (enemyDir == -1)) )   // 플레이어가 오른쪽에 있고 && Enemy가 보는 방향이 왼쪽이라면
                {
                    Flip();
                }    

                if(playerTransform.position.y > transform.position.y)   // 플레이어가 Enemy보다 위에 위치한다면
                {
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lookingUp;
                }
                else
                {
                    transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lookingDown;
                }

                break;

            case ENEMY_TYPE.GOOMBA :    // #70 날아다니는 굼바 - JumpHigh 하고 나서 플레이어 위치 확인 후, 플레이어 따라가도록
                playerDir = playerTransform.position.x > transform.position.x; // Enemy 기준으로 플레이어가 오른쪽에 있는지 확인

                if((!playerDir && (enemyDir == 1))         // Enemy 기준으로 플레이어가 왼쪽에 있고 && Enemy가 보는 방향이 오른쪽이라면
                    || (playerDir && (enemyDir == -1)) )   // 플레이어가 오른쪽에 있고 && Enemy가 보는 방향이 왼쪽이라면
                {
                    Flip();
                }    

                break;

        }
    }

    void ShootFireball()    // #14
    {
        Rigidbody2D fireInstance = Instantiate(fireball, transform.position, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;

        float playerDist = (transform.position.x - playerTransform.position.x) * (transform.position.x - playerTransform.position.x);

// 삼항 연산자
        shootDist = ( playerDist < (5.5)*(5.5))? 1 : 0.5f;    // 가까운 위치이면 1, 먼 위치이면 1/2
        shootHeight = (playerTransform.position.y < transform.position.y) ? -1 : 1; // 플레이어가 아래에 있으면 -1, 위에 있으면 +1
        

        fireInstance.velocity = new Vector2(moveSpeed * enemyDir, moveSpeed * shootDist * shootHeight);    

    }

    void CheckGroundCheck() // #33 따로 함수 추가. 
    {
        // 땅 밟았는지 체크
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Ground"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("LargeBlock"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Obstacle"));
    }

    void Jump(ENEMY_TYPE _type) // #33 껑충껑충 뛰어다니는
    {
        switch(_type)
        {
            case ENEMY_TYPE.GOOMBA :
                Debug.Log("//#69 굼바 낮게 점프");

                rBody.AddForce(Vector2.up * goombaJumpForce);
                // grounded = false;   // 점프 1번 한 후에는 false로 - 이미 Update 문에서 실행함

                break;

            case ENEMY_TYPE.TURTLE :
                Debug.Log("//#33 거북 점프");

                rBody.AddForce(Vector2.up * turtleJumpForce);
                // grounded = false;   // 점프 1번 한 후에는 false로
                
                break;
            
            
        }
    }

    void JumpHigh() // #69 높게 점프하기 - 날개 달린 굼바의 경우에만 해당
    {
        Debug.Log("//#69 굼바 높게 점프");

        rBody.AddForce(Vector2.up * goombaJumpForce * 4);   // 평소보다 2배는 높게 점프하도록
        jumpNum = 0;    // 다시 초기화 - 다시 낮게 뛰기 시작하도록

        CheckDirection(ENEMY_TYPE.GOOMBA);  // #70 날아다니는 굼바가 플레이어 따라다니도록
    }

    // void TurnToWeapon() //#59    // #16 fix : EnemyLife.cs로 코드 이동
    // {
    //     Debug.Log("//#59 무기로 바뀜");
    //     enemyLife.beStepped = false;    // 무기로 바뀌어서 이제 거북 껍질에 닿으면 - PlayerCtrl에서는 GetHurt() 실행됨
    // }

}
