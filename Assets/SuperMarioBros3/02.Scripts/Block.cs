using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour  // 물음표 블록
{
    private enum BLOCK_TYPE {COIN = 1, ITEM };      // 블록 타입
    private BLOCK_TYPE blockType;
    
// #2 블록 업다운
    private Vector3 startPos;
    private Vector3 destPos;
    private float moveTimer = 0f;
    private float upTimer = 0.2f;
    private float downTimer = 0.4f;

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용

// 블록 부딪힐 때
    public Sprite brokenBlock;  // 부숴진 블록 이미지
    private bool isTouched = false; // 플레이어 headCheck와 블록이 닿았는지 확인용
    public AudioClip[] blockClips; // 블록 부딪힐 때 사운드  // 1번째 사운드 : 코인 사운드, 2번째 블록 사운드 : 아이템 블록 사운드  

// 코인 or 아이템 등장
    public GameObject coinUi;   // #3 코인 UI가 사라진 뒤에 자동으로 100pointsUi 등장함 (애니메이터에서 설정함)
    public GameObject mushroom;
    public GameObject leaf;

    void Start()
    {
        startPos = transform.position;
        destPos = transform.position;
        destPos.y += 0.7f;  // 업다운 하는 높이 // y값을 따로 빼서 더해줘야 돼 - Vector3 통째로 값을 수정하는 것이 아니라
    }
    void OnTriggerEnter2D(Collider2D other)      // #2 플레이어가 머리로 박았을 때 (=플레이어와 부딪힘 && 플레이어 위로 점프 중임)
    {       // 이 방법을 이용하면, 플레이어 스크립트에서 작성하지 않아도 되겠다~
        if(other.gameObject.tag == "HeadCheck" && !isTouched)            // 딱 1번만 실행 //플레이어 headCheck에 부딪힌 거라면   && 아직 부숴진 상태가 아니라면
        {   
            Debug.Log("#2 HeadCheck");
            StartCoroutine(BlockUpDown());

            transform.GetComponent<SpriteRenderer>().sprite = brokenBlock;  // 딱딱한 블록 이미지로 변경
            
            // #3 코인 UI 등장
            Vector3 coinPos;                // 생성 위치 설정
            coinPos = transform.position;
            coinPos.y += 1f;

            Instantiate(coinUi, coinPos, Quaternion.identity);  // 생성
            Debug.Log("#3 코인 UI 생성 위치 : " + coinPos);
            
        }
    }
    
    IEnumerator BlockUpDown()   // #2 블록 업다운 함수
    {
        isTouched = true;
        int i=1;
        while(true)
        {
            Debug.Log( "#2 " + i++ +"번째 블록 업다운 : " + moveTimer);

            if(moveTimer <= upTimer)
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/upTimer));
            else if(moveTimer <= downTimer) //upTimer 초과이면서 downTimer 이하일 때
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(moveTimer/downTimer));
            else    // 시간이 모두 지났으면(0.4초 후에) = 업다운 마치고 원위치로 돌아온 후
            {
                Debug.Log("#2 블록 부숴짐");
                BlockIsBroken();
                moveTimer = 0f; // 다시 원상복구

                yield break;    // 코루틴 탈출
            }
            moveTimer += Time.deltaTime;

            yield return null;
            // yield return new WaitForSeconds(1f);
        }
    }

    void BlockIsBroken()    // #2 플레이어 headCheck와 블록 부딪힌 후 작동
    {
        switch(blockType)   // 블록 타입에 따라 다르게 작동
        {
            case BLOCK_TYPE.COIN : 
                AudioSource.PlayClipAtPoint(blockClips[0], transform.position);
                break;
            case BLOCK_TYPE.ITEM : 
                AudioSource.PlayClipAtPoint(blockClips[1], transform.position);
                break;
        }
    }

    void ItemAppears()
    {

    }
}
