using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour  // 물음표 블록
{
    public enum BLOCK_TYPE {COIN = 1, ITEM, FRAGILE, PBUTTON };      // 블록 타입    // #25 깨지기 쉬운 블록 타입 추가  // #26 PBUTTON 블록 타입 추가
    public BLOCK_TYPE blockType;

// #2 블록 업다운
    private Vector3 startPos;
    private Vector3 destPos;
    private float moveTimer = 0f;
    private float upTimer = 0.2f;
    private float downTimer = 0.4f;

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용
/*
    Lerp()함수를 사용할 때, curve 적용한 목적 : 보간 값에 변화를 주기 위한 목적
    더 다양한 움직임 표현 가능
    (1) : 부드러운 움직임 - 선형 곡선이기 때문에
    (2) : 물리적인 효과 흉내 (중력, 탄성처럼 자연스럽고 현실적이게)
    (3) : 다양한 움직임 패턴 (특정 구간에서 속도 조절 가능)
*/
// 블록 보이는 모습. 애니메이션.
    private Animator anim;  //#6


// 블록 부딪힐 때
    public Sprite brokenBlock;  // 부숴진 블록 이미지
    private bool isTouched = false; // 플레이어 headCheck와 블록이 닿았는지 확인용
    public AudioClip[] blockClips; // 블록 부딪힐 때 사운드  // 1번째 사운드 : 코인 사운드, 2번째 블록 사운드 : 아이템 블록 사운드  

// 코인 or 아이템 등장
    public GameObject coinUi;   // #3 코인 UI가 사라진 뒤에 자동으로 100pointsUi 등장함 (애니메이터에서 설정함)
    public GameObject mushroomObj; // #4
    public GameObject leafObj;


// 플레이어 Level 상태 확인
    private PlayerLife playerLife;

// #25 블록 부딪힐 때 나오는 파편 - layer 번호 더 크게 해서 가장 앞에서 보이도록
    private SpriteRenderer[] fragments;

    void Awake()
    {
        playerLife = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLife>();

        anim = GetComponent<Animator>();    // #6 애니메이션 설정

        switch(blockType)   // #25
        {
            case BLOCK_TYPE.FRAGILE:
                fragments = GetComponentsInChildren<SpriteRenderer>();
                break;
        }
    }
    void Start()
    {
        switch(blockType)           // #25 조건 추가
        {
            case BLOCK_TYPE.COIN:
            case BLOCK_TYPE.ITEM:
            case BLOCK_TYPE.PBUTTON:    // #26
                startPos = transform.position;
                destPos = transform.position;
                destPos.y += 0.7f;  // 업다운 하는 높이 // y값을 따로 빼서 더해줘야 돼 - Vector3 통째로 값을 수정하는 것이 아니라

                break;
        }
    }
    void OnTriggerEnter2D(Collider2D other)      // #2 플레이어가 머리로 박았을 때 (=플레이어와 부딪힘 && 플레이어 위로 점프 중임)
    {       
        switch(blockType)           // #25 조건 추가
        {
            case BLOCK_TYPE.COIN:
            case BLOCK_TYPE.ITEM:// 이 방법을 이용하면, 플레이어 스크립트에서 작성하지 않아도 되겠다~
            case BLOCK_TYPE.PBUTTON:        // #26 PBUTTON 블록을 플레이어가 머리로 쳤을 때

                if(other.gameObject.tag == "HeadCheck" && !isTouched)            // 딱 1번만 실행 //플레이어 headCheck에 부딪힌 거라면   && 아직 부숴진 상태가 아니라면
                {   
                    // Debug.Log("#2 HeadCheck");
                    StartCoroutine(BlockUpDown());

                    // anim.SetBool("IsTouched", true);   // #6 이 방법으로 하면, 애니메이터가 완전히 멈추는 게 아니기 때문에 sprte 이미지가 바뀌지 않음.  
                    anim.enabled = false;                 // #6 블록 이미지 바꾸기 위한 목적

                    transform.GetComponent<SpriteRenderer>().sprite = brokenBlock;  // 딱딱한 블록 이미지로 변경
                    BlockIsTouched();    // 코인 또는 아이템 등장
                }
                break;
        }
    }
    void OnCollisionEnter2D(Collision2D other) 
    {
        switch(blockType)           // #25 조건 추가
        {
            case BLOCK_TYPE.COIN:
            case BLOCK_TYPE.ITEM:
                if(other.gameObject.tag == "Enemy" && !isTouched)       // #22 보완 - 한번만 등장하도록 isTouched 변수 추가
                {
                    if(other.gameObject.GetComponent<EnemyCtrl>().enemyType == EnemyCtrl.ENEMY_TYPE.SHELL)  // #22 거북 등껍질에 부딪혔을 때에도 블록 부숴지도록
                    {
                        StartCoroutine(BlockUpDown());
                        anim.enabled = false;                 // 블록 이미지 바꾸기 위한 목적
                        transform.GetComponent<SpriteRenderer>().sprite = brokenBlock;  // 딱딱한 블록 이미지로 변경
                        BlockIsTouched();    // 코인 또는 아이템 등장
                    }
                }

                break;
        }
    }
    


    IEnumerator BlockUpDown()   // #2 블록 업다운 함수
    {
        if(!isTouched)          // #25 확실히 1번만 실행하기 위해 조건 추가
            isTouched = true;

        // int i=1;
        while(true)
        {
            // Debug.Log( "#2 " + i++ +"번째 블록 업다운 : " + moveTimer);

            if(moveTimer <= upTimer)
                transform.localPosition = Vector3.Lerp(startPos, destPos, curve.Evaluate(moveTimer/upTimer));
            else if(moveTimer <= downTimer) //upTimer 초과이면서 downTimer 이하일 때
                transform.localPosition = Vector3.Lerp(destPos, startPos, curve.Evaluate(moveTimer/downTimer));
            else    // 시간이 모두 지났으면(0.4초 후에) = 업다운 마치고 원위치로 돌아온 후
            {
                // Debug.Log("#2 블록 부숴짐");
                moveTimer = 0f; // 다시 원상복구

                yield break;    // 코루틴 탈출
            }
            moveTimer += Time.deltaTime;

            yield return null;
            // yield return new WaitForSeconds(1f);
        }
    }

    void BlockIsTouched()    // #2 플레이어 headCheck와 블록 부딪힌 후 작동
    {
        switch(blockType)   // 블록 타입에 따라 다르게 작동
        {
            case BLOCK_TYPE.COIN : 
                AudioSource.PlayClipAtPoint(blockClips[0], transform.position);

                // #3 코인 UI 등장  
                Vector3 coinPos;                // 생성 위치 설정
                coinPos = transform.position;
                coinPos.y += 1f;

                Instantiate(coinUi, coinPos, Quaternion.identity);  // 생성
                // Debug.Log("#3 코인 UI 생성 위치 : " + coinPos);

                break;
            case BLOCK_TYPE.ITEM : 
            {
                AudioSource.PlayClipAtPoint(blockClips[1], transform.position);
                switch(playerLife.playerLevel)  // #4 #5 플레이어 레벨에 따라 다른 아이템 등장함
                {
                    case PlayerLife.MODE_TYPE.LEVEL1 : 
                        StartCoroutine(ItemAppears(Item.ITEM_TYPE.MUSHROOM));   // #2 보완 - 코루틴 활용 - 코루틴은 항상 생성 or 소멸에만 사용하라고 배운 기억
                        break;
                    case PlayerLife.MODE_TYPE.LEVEL2 : 
                        StartCoroutine(ItemAppears(Item.ITEM_TYPE.LEAF));
                        break;
                    case PlayerLife.MODE_TYPE.LEVEL3 :
                        StartCoroutine(ItemAppears(Item.ITEM_TYPE.LEAF)); 
                        break;                    
                }
            }
                break;
            case BLOCK_TYPE.PBUTTON :   // #26 
                // anim.SetTrigger("Smoke");    // 연기 효과 ->  Particle System(파티클)로 표현
                this.transform.GetChild(0).gameObject.SetActive(true);  // PBUTTON 활성화 - 등장과 동시에 파티클 시스템으로 Smoke 효과

                break;
        }
    }

    IEnumerator ItemAppears(Item.ITEM_TYPE _type)  // #4 #5 아이템(버섯 or 나뭇잎) 등장
    {
        yield return new WaitForSeconds(downTimer); // #2 #4 블록이 완전히 다 내려온 뒤에 버섯 등장하도록 

        switch(_type)
        {
            case Item.ITEM_TYPE.MUSHROOM : 
                Instantiate(mushroomObj, transform.position, Quaternion.identity); // 생성 (이때 생성된 버섯 or 나뭇잎은 isTrigger 체크되어 있어야 함. 블록과 충돌처리 되지 않도록)
                break;
            case Item.ITEM_TYPE.LEAF : 
                Instantiate(leafObj, transform.position, Quaternion.identity); // 생성 (이때 생성된 버섯 or 나뭇잎은 isTrigger 체크되어 있어야 함. 블록과 충돌처리 되지 않도록)
                break;
        }
    }

    public void FragileBlockBroken()   // #25 SHELL에 부딪히면 약한 블록 부숴짐 - EnemyCtrl에서 실행됨
    {
        FragementsVisible();
        anim.SetTrigger("Broken");  // 부숴지는 파편 효과 - 애니메이션으로 표현
            // 애니메이션 끝날 때 Destroy 효과 적용                
    }
    void FragementsVisible()    // #25 파편 블록들 잘 보이도록
    {
        foreach(SpriteRenderer spr in fragments)
        {
            spr.sortingOrder = 15;  // 다른 블록들보다 더 앞에서 보이도록
        }
    }

}
