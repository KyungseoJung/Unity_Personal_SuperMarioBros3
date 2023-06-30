using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour //#1 플레이어 컨트롤(움직임 관련)
{
    private PlayerLife playerLife;          // #17

    private Animator anim;
    private Rigidbody2D Rbody;

    private bool dirRight = true;           // 플레이어가 바라보는 방향(오른쪽 : 1, 왼쪽 : -1)

    private float moveForce = 50f;          // 이동 속도
    private float maxSpeed = 5f;            // 달리기 가속도. 최고 속도

    private float jumpTimer;
    private float jumpTimeLimit = 0.25f;
    private bool isJumping;                      // 점프 가능한지 체크
    public float jumpForce = 70f;           // 점프 가속도. 누르는 동안 더해지는 높이
    public float minJump = 100f;            // 최소 점프 높이
    private float bounceJump =600f;        // 살짝 튀어오를 때 점프 높이 - 예 : Enemy 밟았을 때

    private bool grounded;                  // 땅 밟았는지 체크
    // public bool steppingOnEnemy;         // #11 적 밟았는지 확인   -> // #15로 변경
    public bool pushPButton;                // #27 P버튼 밟았는지 체크
    public Transform groundCheck;           // 땅 밟았는지 체크

    public float velocityY;
    private bool fallDown;                  // 지금 추락하고 있는지 체크

// 오디오 ==================================
    public AudioClip jumpClip;
    public AudioClip coinClip;              // 코인 획득 클립

// // 충돌 처리 - 점프할 땐, LargeBlock과 부딪히지 않도록   // #21 버그 수정 (콜라이더 위치를 최상위 부모로 바꿨으니, 레이어 변경 코드 대상도 최상위 부모로 수정 필요)
//     private GameObject level1Obj;
//     private GameObject level2Obj;
//     private GameObject level3Obj;

// #8 플레이어 X좌표 위치 제한
    private Vector3 playerPos;

// #28 임의의 점수 변수 - 코인 획득 시 점수 증가 
    private int score;

    void Awake()
    {
        playerLife = GetComponent<PlayerLife>();        // #17

        Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식
        Transform secondChild = transform.GetChild(1);
        Transform thirdChild = transform.GetChild(2);

        anim = firstChild.GetComponent<Animator>();
        Rbody = GetComponent<Rigidbody2D>(); // 레벨 바꿀 때, 변경해줘도 되니까~    // #7 수정 - 지금까지 자식 오브젝트 위치가 이동하고 있었음

        groundCheck = firstChild.Find("groundCheck");   // 0번째 자식 오브젝트의 자식들 중에서 groundCheck를 찾기   // 레벨 바꿀 때, 이 값도 변경해야 할 듯

        // level1Obj = firstChild.gameObject;           // #21 버그 수정
        // level2Obj = secondChild.gameObject;
        // level3Obj = thirdChild.gameObject;
    }
    
    void Update()
    {  
        CheckGroundCheck();

        // 점프 가속도   // 한번 스페이스바 누르면 > 최소 minJump만큼은 점프하도록
        if(Input.GetButtonDown("Jump") && grounded && (playerLife.playerState != PlayerLife.MODE_STATE.HURT))     
        {
            isJumping = true;
            Rbody.AddForce(Vector2.up * minJump);                       // 위로 
            // anim.SetTrigger("Jump");                                    // 애니메이션
            AudioSource.PlayClipAtPoint(jumpClip, transform.position);  // 효과음
        }

        if(fallDown)   // 추락하고 있을 땐, 다시 부딪히는 레이어로 변경 // #21 버그 수정
        {
            gameObject.layer = 11;  // "FallDownPlayer" 레이어
            // level1Obj.layer = 11;   
            // level2Obj.layer = 11;
            // level3Obj.layer = 11;
        }
        else
        {
            gameObject.layer = 10;  // 추락하지 않는 동안에는 큰 블록들(Layer : LargeBlock) 그냥 통과하도록
            // level1Obj.layer = 10;   // "Player" 레이어
            // level2Obj.layer = 10;
            // level3Obj.layer = 10;
        }
    }

    void FixedUpdate()
    {   
        if(transform.position.x < -6.65)    // #8 맵 기준으로 왼쪽 맨 끝까지 갈 수 없도록
        {
            playerPos = transform.position;
            playerPos.x = -6.65f;
            transform.position = playerPos;
        }

    //달리기 가속도 ===============================
        float h = Input.GetAxis("Horizontal");
        // anim.SetFloat("Speed", Mathf.Abs(h));

        if(h*Rbody.velocity.x < maxSpeed) //최고 속도 도달하기 전이면, 속도 계속 증가
            Rbody.AddForce(Vector2.right * h * moveForce);

        if(Mathf.Abs(Rbody.velocity.x) > maxSpeed)
            Rbody.velocity = new Vector2(Mathf.Sign(Rbody.velocity.x) * maxSpeed, Rbody.velocity.y);

    //점프 가속도 ===============================
        if(isJumping)
        {
            Rbody.AddForce(Vector2.up * jumpForce);
            jumpTimer += Time.deltaTime;

            if(fallDown)            // 블록->블록으로 점프하고 있는 경우 고려
                fallDown = false;   // 점프하고 있을 때 = 추락하고 있지 않을 때

            if(!Input.GetButton("Jump") || jumpTimer > jumpTimeLimit)   //점프 가속도 최대값 도달하면 -> 그 다음은 밑으로 추락
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
    }

    void OnCollisionEnter2D(Collision2D col)  // #17 플레이어가 Enemy와 그냥 부딪혔을 때
    {
        if(col.gameObject.tag == "Enemy")
        {
            Debug.Log("//#17 플레이어가 Enemy랑 부딪힘. 다침");
            if(! col.gameObject.GetComponent<EnemyLife>().beStepped)
                playerLife.GetHurt();
        }

        if(col.gameObject.tag == "Coin")    // #28  코인 획득
        {
            Destroy(col.gameObject);        // 코인 사라져
            score += 100;                   // 점수 획득
            AudioSource.PlayClipAtPoint(coinClip, transform.position);  // 효과음

        }

    }
    public void BounceUp() // #16 약간 위로 튀어오르기 - 예 : 몬스터 밟았을 때
    {
        Rbody.AddForce(Vector2.up * bounceJump);
        Debug.Log("//#16 플레이어 살짝 위로 튀어오르기");
    }
}
