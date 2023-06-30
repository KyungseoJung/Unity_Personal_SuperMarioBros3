using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour  // #9 몬스터 움직임
{
// 몬스터 종류
    public enum ENEMY_TYPE {GOOMBA = 1, TURTLE, SHELL, FLOWER};
    public enum WINGS_TYPE {YES = 1, NO};    // 날개 달려 있는지

    public ENEMY_TYPE enemyType;
    public WINGS_TYPE wingsType;

// 몬스터 이동
    public int enemyDir = -1;     // 오른쪽 : 1, 왼쪽 : -1 // 처음엔 왼쪽으로 이동
    public float moveSpeed = 2f;        // 이동 속도
    private Rigidbody2D rBody;      
    private Transform frontCheck;   // #18 부딪혔을 때 이동 방향 바꾸도록 확인용

// #12 꽃 움직임
    public bool isMoving = true;               // 움직여도 되는지 확인용 bool 변수
    private IEnumerator flowerUpCor;
    private IEnumerator flowerDownCor;
    private Vector3 startPos;
    private Vector3 destPos;

    private float moveTimer = 0f;           // 현재 움직임 파악
    private float upDownTimer = 1f;         // 위로 올라와서 등장하는 데 1초 소모

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용  // 곡선 이용 - (0,0)에서 (1.1)로 가는 자연스러운 움직임 연출

    private Transform playerTransform;  // #13 꽃 Enemy - 쳐다보는 방향(좌/ 우) 플레이어 위치에 따라 방향 바꾸도록
    public Sprite lookingUp;            // #13 위 바라보는 이미지
    public Sprite lookingDown;          // #13 아래 바라보는 이미지
// #14 꽃 Enemy 파이어 볼 공격
    public Rigidbody2D fireball;        // #14 꽃 Enemy - 파이어볼 공격하기    // GameObject가 아닌 Rigidbody로 받는다. 속도 접근 쉽도록
    private float shootDist;            // 삼항연산자 - 플레이어와 Enemy 위치에 따라 다른 각도로 파이어 볼 쏘기
    private int shootHeight;            // 삼항연산자

// #16 거북 껍질 발로 차기
    public bool kickShell;
    private float kickSpeed = 15f;       // 플레이어가 발로 찼을 때 날라가는 속도 // fix: public으로 하면 초기 선언할 때 인스펙터 값이 우선 적용됨. private으로 하거나 확실히 TURTLE -> SHELL로 변할 때 값을 설정해주자.
// #19 죽은 후 상태에 따른 함수 실행
    private EnemyLife enemyLife;
    
    void Awake()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA :
            case ENEMY_TYPE.TURTLE : 
                rBody = GetComponent<Rigidbody2D>();    
                frontCheck = transform.GetChild(1).GetComponent<Transform>();   
                break;
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        enemyLife = GetComponent<EnemyLife>();
    }

    void Start()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA :
            case ENEMY_TYPE.TURTLE : 
                moveSpeed = 2f;
                break;
            
            case ENEMY_TYPE.FLOWER : 
                startPos = transform.position;  // #12 시작 위치, 도착 위치 설정
                destPos = transform.position;
                destPos.y += 1f;
                moveSpeed = 3f;                 // #14 파이어볼 속도
                
                StartCoroutine(FlowerUp());     // #12 꽃 움직임 시작

                break;
        }
    }

    void FixedUpdate()
    {


        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
            case ENEMY_TYPE.TURTLE : 
                if(enemyLife.enemystate == EnemyLife.ENEMY_STATE.DIE)  //#9 리팩터링
                    return;
                    
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
                break;
            case ENEMY_TYPE.FLOWER : 
                CheckDirection();       // #13 바라보는 방향 (좌/우)(위/아래) 체크
                break;
        }

    }

    public void Flip()
    {
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
            case ENEMY_TYPE.SHELL :
                if(Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Obstacle"))
                    || Physics2D.Linecast(transform.position, frontCheck.position, 1<<LayerMask.NameToLayer("Ground")))
                {
                    if(col.gameObject.tag == "FragileBlock")   // #25 SHELL의 frontCheck가 부숴지는 블록과 부딪힌 거라면 - 블록이 부숴지도록
                    {
                        col.gameObject.GetComponent<Block>().FragileBlockBroken();
                    }
                    Flip();
                }
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D col)   
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.FLOWER : 
                if(col.gameObject.tag == "Player")  // #12 꽃 Enemy 주위에 플레이어가 있다면 꽃 등장 X 
                {
                    // Debug.Log("#12 플레이어가 꽃 가까이 들어왔다");
                    isMoving = false;

                }
                break;
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
        switch(enemyType)
        {
            case ENEMY_TYPE.FLOWER : 
                if(col.gameObject.tag == "Player")  // #12 플레이어가 다시 멀어졌으니, 올라오기 다시 시작해라
                {
                    // Debug.Log("#12 플레이어가 꽃 멀리 벗어났다");

                    StopCoroutine(FlowerUp());      // 중복 실행을 막기 위해 코루틴 종료시킨 후, 새로 시작하기
                    StopCoroutine(FlowerDown());

                    isMoving = true;
                    StartCoroutine(FlowerUp());   
 
                }
                break;
        }

    }
    IEnumerator FlowerUp() // #12 꽃 - 위로 올라오기
    {
        if(!isMoving)       // 움직이면 안 되는 상태라면 코루틴 아예 종료
            yield break;

        while(true)  // 올라가도 될 때에만 올라가도록
        {
            // Debug.Log("#12 업 함수 실행//" + moveTimer);
            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                moveTimer = 0f;
                StartCoroutine(FlowerDown());      // 다시 내려가도록
                yield break;    // 현재 코루틴 종료
            }
            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기
        }
    }

    IEnumerator FlowerDown()    // #12 꽃 - 밑으로 내려가기
    {
        ShootFireball();                   // #14 내려가기 직전에 파이어볼 쏘기

        while(true)
        {
            // Debug.Log("#12 다운 함수 실행" + moveTimer);
            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                moveTimer = 0f;

                if(isMoving)
                    StartCoroutine(FlowerUp());    // 다시 올라오도록

                yield break;    // 현재 코루틴 종료
            }
            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기

        }
    }

    void CheckDirection()
    {
        bool playerDir = playerTransform.position.x > transform.position.x; // Enemy 기준으로 플레이어가 오른쪽에 있는지 확인

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

}
