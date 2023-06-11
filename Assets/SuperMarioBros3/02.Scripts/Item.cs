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
    private float comeUpTimer = 1f;   // #4 
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
    private float moveSpeed = 0.3f;    // #4 이동 속도
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
        startPos = transform.position;          // #4 버섯 등장하기
        destPos = transform.position;
        destPos.y += 1f;    

        if(!dirRight)   // 만약 처음 이동 방향이 왼쪽이라면, 왼쪽 방향부터 이동 시작하도록 설정
            Flip();

        StartCoroutine(ItemComeUp(itemType));
    }

    void FixedUpdate()
    {
        //#4 장애물과 충돌하면 방향 바꾸도록

        if(comeUpComplete)      // #4 완전히 위로 올라오면, 그때부터 이동 시작
        {
            rBody.velocity = new Vector2(transform.localPosition.x * moveSpeed, rBody.velocity.y);
        }
    }

    void Flip() // #4 이동 방향 바꿈
    {
        Vector3 itemScale = transform.localScale;
        itemScale.x *= -1;
        transform.localScale = itemScale;
    }

    IEnumerator ItemComeUp(ITEM_TYPE _type) // #4
    {
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
    }
}
