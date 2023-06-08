using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour //#1 플레이어 컨트롤(움직임 관련)
{
    private Animator anim;
    private Rigidbody2D Rbody;
    public enum MODE_TYPE {LEVEL1 = 1, LEVEL2, LEVEL3 };      // 플레이어 타입
    public enum MODE_STATE {IDLE = 1, HURT, LEVELCHANGE};
        // 다친 상태인지 체크 - 다친 상태라면 점프 불가능, 일정 시간동안 공격 안 받음
    public MODE_TYPE playerType = MODE_TYPE.LEVEL1;
    public MODE_STATE playerState = MODE_STATE.IDLE;
    private bool dirRight = true;           // 플레이어가 바라보는 방향(오른쪽 : 1, 왼쪽 : -1)

    private float moveForce = 50f;          // 이동 속도
    private float maxSpeed = 5f;            // 달리기 가속도. 최고 속도

    private float jumpTimer;
    private float jumpTimeLimit = 0.3f;
    private bool jump;                      // 점프 가능한지 체크
    public float jumpForce = 70f;           // 점프 가속도. 누르는 동안 더해지는 높이
    public float minJump = 100f;            // 최소 점프 높이

    private bool grounded;                  // 땅 밟았는지 체크
    public Transform groundCheck;           // 땅 밟았는지 체크

    public float velocityY;
    private bool fallDown;                  // 지금 추락하고 있는지 체크

// 오디오 ==================================
    public AudioClip jumpClip;

// 충돌 처리 - 점프할 땐, LargeBlock과 부딪히지 않도록
    private GameObject level1Obj;
    private GameObject level2Obj;
    private GameObject level3Obj;

    void Awake()
    {
        Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식
        Transform secondChild = transform.GetChild(1);
        Transform thirdChild = transform.GetChild(2);

        anim = firstChild.GetComponent<Animator>();
        Rbody = firstChild.GetComponent<Rigidbody2D>();

        groundCheck = firstChild.Find("groundCheck");   // 0번째 자식 오브젝트의 자식들 중에서 groundCheck를 찾기

        level1Obj = firstChild.gameObject;
        level2Obj = secondChild.gameObject;
        level3Obj = thirdChild.gameObject;
    }
    

    void Update()
    {  
        // 땅 밟았는지 체크
        grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("Ground"))
                    || Physics2D.Linecast(transform.position, groundCheck.position, 1<<LayerMask.NameToLayer("LargeBlock"));

        // 점프 가속도   // 한번 스페이스바 누르면 > 최소 minJump만큼은 점프하도록
        if(Input.GetButtonDown("Jump") && grounded && (playerState != MODE_STATE.HURT))     
        {
            jump = true;
            Rbody.AddForce(Vector2.up * minJump);                       // 위로 
            anim.SetTrigger("Jump");                                    // 애니메이션
            AudioSource.PlayClipAtPoint(jumpClip, transform.position);  // 효과음
        }

        if(fallDown)   // 추락하고 있을 땐, 다시 부딪히는 레이어로 변경
        {
            level1Obj.layer = 11;
            level2Obj.layer = 11;
            level3Obj.layer = 11;
        }
        else
        {
            // 추락하지 않는 동안에는 큰 블록들(Layer : LargeBlock) 그냥 통과하도록
            level1Obj.layer = 10;
            level2Obj.layer = 10;
            level3Obj.layer = 10;
        }
    }

    void FixedUpdate()
    {   
    //달리기 가속도 ===============================
        float h = Input.GetAxis("Horizontal");
        anim.SetFloat("Speed", Mathf.Abs(h));

        if(h*Rbody.velocity.x < maxSpeed) //최고 속도 도달하기 전이면, 속도 계속 증가
            Rbody.AddForce(Vector2.right * h * moveForce);

        if(Mathf.Abs(Rbody.velocity.x) > maxSpeed)
            Rbody.velocity = new Vector2(Mathf.Sign(Rbody.velocity.x) * maxSpeed, Rbody.velocity.y);

    //점프 가속도 ===============================
        if(jump)
        {
            Rbody.AddForce(Vector2.up * jumpForce);
            jumpTimer += Time.deltaTime;

            if(fallDown)            // 블록->블록으로 점프하고 있는 경우 고려
                fallDown = false;   // 점프하고 있을 때 = 추락하고 있지 않을 때

            if(!Input.GetButton("Jump") || jumpTimer > jumpTimeLimit)   //점프 가속도 최대값 도달하면 -> 그 다음은 밑으로 추락
            {
                jump = false;
                jumpTimer = 0f;
            }
        }
        

        if(Rbody.velocity.y <0 && !fallDown)    // 추락하고 있을 때
        {
            fallDown = true;    
            Debug.Log("#1 fallDown = true");
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

}
