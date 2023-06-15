using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour   // #4 버섯 #5 나뭇잎
{

// 아이템 등장
    public enum ITEM_TYPE {MUSHROOM = 1, LEAF};    // #4 아이템 타입
    public ITEM_TYPE itemType;              // #4 인스펙터창에서 각 프리팹을 직접 설정하도록

    private bool comeUpComplete = false;        // #4 완전히 올라왔는지 체크  
    private Vector3 startPos;
    private Vector3 destPos;
    private float moveTimer = 0f;       // #4 아이템 자연스럽게 등장하도록(Lerp 함수 이용)
    private float comeUpTimer;   // #4 
    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // #4 커브 처리를 이용해 업 다운 적용

    private CircleCollider2D itemColl;         
        /* 
        참고 : 그냥 Collider2D를 사용해도 좋지만, 
        (1) 프로그램 성능 효율 측면 - 불필요한 검색 작업 줄여줌. 형태 변환 필요 없음. 
        (2) 가독성과 유지 보수성 - 다른 개발자들이 이해하기 쉬움
        등의 이유로 
        콜라이더가 명확히 정해져있다면, 정확히 지정하는 것이 더 좋음.
        */
// 아이템 이동
    private bool dirRight = true;      // #4 이동 방향 - Block.cs에서 결정해주도록
    private float moveSpeed;    // #4 이동 속도
/* 
    디테일 : 
    플레이어 headCheck가 물음표 박스 중심보다 왼쪽에서 치면, 버섯은 오른쪽으로 이동
                물음표 박스 중심보다 오른쪽에서 치면, 버섯은 왼쪽으로 이동
*/
    private Rigidbody2D rBody;  // #4 움직이도록

    void Awake()
    {
        itemColl = GetComponent<CircleCollider2D>();
        itemColl.isTrigger = true;  // 완전히 위로 올라오기 전까지는, 아이템 블록과 충돌처리 하지 않도록

        rBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

        switch(itemType)
        {
            case ITEM_TYPE.MUSHROOM :
                comeUpTimer = 1f;       // 다 올라오는 데 1초 걸림       
                moveSpeed = 3f;

                startPos = transform.position;          // #4 버섯 등장하기
                destPos = transform.position;
                destPos.y += 1f;    

                if(!dirRight)   // 만약 처음 이동 방향이 왼쪽이라면, 왼쪽 방향부터 이동 시작하도록 설정
                    Flip();                
                break;
            
            case ITEM_TYPE.LEAF :
                comeUpTimer = 0.5f;     // 다 올라오는 데 0.3초 걸림
                moveSpeed = 80f;         // 살랑거리는 움직임 연출
                
                rBody.mass = 0.5f;        // 무게 가볍게 설정

                rBody.gravityScale = -1f; // 첫 등장할 땐, 위로 붕 뜨도록  // 중력으로 조절? OR AddForce
                rBody.AddForce(Vector3.up * 10f);


                break;
        }

        StartCoroutine(ItemComeUp(itemType));           // #4 #5 아이템 등장 모습


    }

    void FixedUpdate()
    {
        switch(itemType)
        {
            case ITEM_TYPE.MUSHROOM :
                //#4 장애물과 충돌하면 방향 바꾸도록

                if(comeUpComplete)      // #4 완전히 위로 올라오면, 그때부터 이동 시작
                {
                    // rBody.velocity = new Vector2(transform.localPosition.x * moveSpeed, rBody.velocity.y);
                    rBody.velocity = new Vector2(Mathf.Sign(rBody.velocity.x) * moveSpeed, rBody.velocity.y);   // #9 
                        //이 방식 사용하면 frontCheck 없이도 반동을 느끼면, 방향 바꿔서 이동함

                }
                break;

            case ITEM_TYPE.LEAF :       // #5
                break;
        }

    }

    void Flip() // #4 이동 방향 바꿈
    {
        dirRight = !dirRight;   //#5 나뭇잎 바라보는 방향 변경 - 가해지는 힘의 방향도 다르게 하기 위함(ChangeDirection함수)

        Vector3 itemScale = transform.localScale;
        itemScale.x *= -1;
        transform.localScale = itemScale;
    }

    IEnumerator ItemComeUp(ITEM_TYPE _type) // #4 버섯 등장
    {
        switch(_type)
        {
            case ITEM_TYPE.MUSHROOM : 
                while(true)
                {
                    if(moveTimer < comeUpTimer)
                    {
                        transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/comeUpTimer));
                    }
                    else
                    {
                        itemColl.isTrigger = false; // 완전히 위로 올라온 뒤부터는, 아이템 블록과 충돌처리 하도록
                        comeUpComplete = true;
                        yield break;
                    }
                    moveTimer += Time.deltaTime;
                    yield return null;
                }
            
            case ITEM_TYPE.LEAF :       //#5
                while(true)
                {
                    if(moveTimer >= comeUpTimer)
                    {
                        rBody.velocity = new Vector2(rBody.velocity.x, 0f); // 바로 직전에 위로 붕 뜨는 그 속도가 다음 움직임에 영향을 주지 않도록
                        // rBody.AddForce(Vector3.down * moveSpeed*2);  // 역으로 힘을 가하는 게 아니라, 위 코드처럼 속도 자체를 조절해야 함.
                        
                        rBody.gravityScale = 1f;  // 그 이후로는 천천히 아래로 떨어지도록
                        comeUpComplete = true;

                        StartCoroutine(ChangeDirection());  //#5 살랑거리는 움직임 연출
                        yield break;
                    }
                    moveTimer += Time.deltaTime;
                    yield return null;
                }
        }
    }

    IEnumerator ChangeDirection()   // #5 나뭇잎 등장 - 1초마다 방향 바꾸면서 살랑 거리며 떨어짐 // #9 위치만 약간 바꿈
    {
        while(true)
        {
            if(dirRight)
            {
                rBody.AddForce(Vector2.right * moveSpeed);
            }    
            else
            {
                rBody.AddForce(Vector2.left * moveSpeed);
            }

            yield return new WaitForSeconds(0.2f);
            
            rBody.AddForce(Vector2.up * 100f);   // 깃털 효과 - 떨어질 때 곡선을 그리도록

            yield return new WaitForSeconds(0.5f);  
            
            rBody.velocity = new Vector2(0f, 0f); // 이전에 반대편으로 살랑거렸던 그 움직임이(속도가) 다음 움직임에 영향을 주지 않도록

            Flip();
        }
    }
}
