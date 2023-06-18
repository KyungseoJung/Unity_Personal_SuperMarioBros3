using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour  // #9 몬스터 움직임
{
// 몬스터 종류
    public enum ENEMY_TYPE {GOOMBA = 1, TURTLE, FLOWER};
    public enum WINGS_TYPE {YES = 1, NO};    // 날개 달려 있는지

    public ENEMY_TYPE enemyType;
    public WINGS_TYPE wingsType;

// 몬스터 이동
    private int enemyDir = -1;     // 오른쪽 : 1, 왼쪽 : -1 // 처음엔 왼쪽으로 이동
    public float moveSpeed;        // 이동 속도
    private Rigidbody2D rBody;      

// #12 꽃 움직임
    private IEnumerator flowerUpCor;
    private IEnumerator flowerDownCor;
    private Vector3 startPos;
    private Vector3 destPos;

    private float moveTimer = 0f;           // 현재 움직임 파악
    private float upDownTimer = 1f;       // 위로 올라와서 등장하는 데 1초 소모


    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용  // 곡선 이용 - (0,0)에서 (1.1)로 가는 자연스러운 움직임 연출

    void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
        
        flowerUpCor = FlowerUp();       // #12
        flowerDownCor = FlowerDown();
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
                
                StartCoroutine(flowerUpCor);     // #12 꽃 움직임 시작

                break;
        }
    }

    void FixedUpdate()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
            case ENEMY_TYPE.TURTLE : 
                rBody.velocity = new Vector2(enemyDir * moveSpeed, rBody.velocity.y);
                    // #9 Mathf.Sign : 부호를 반환하는 함수
                break;
        }
    }

    void Flip()
    {
        enemyDir *= -1;

        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;
    }

    IEnumerator FlowerUp() // #12 꽃 - 위로 올라오기
    {
        while(true)
        {
            Debug.Log("#12 업 함수 실행//" + moveTimer);

            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                moveTimer = 0f;
                StartCoroutine(FlowerDown());      // 다시 내려가도록

                yield break;    // 코루틴 종료
            }
            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기
        }
    }

    IEnumerator FlowerDown()    // #12 꽃 - 밑으로 내려가기
    {
        while(true)
        {
            Debug.Log("#12 다운 함수 실행" + moveTimer);

            if(moveTimer < upDownTimer)
            {
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(moveTimer/upDownTimer));
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                moveTimer = 0f;
                StartCoroutine(FlowerUp());    // 다시 올라오도록

                yield break;    // 코루틴 종료
            }
            moveTimer += Time.deltaTime;
            yield return null;  // 한 프레임 대기

        }
    }

}
