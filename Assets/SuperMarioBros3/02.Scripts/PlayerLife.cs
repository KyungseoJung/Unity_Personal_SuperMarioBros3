using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;  // #73 


//using UnityEngine.UI; // #73 안 써서 주석 처리

public class PlayerLife : MonoBehaviour
{
    public enum MODE_TYPE {LEVEL1 = 1, LEVEL2, LEVEL3 };      // 플레이어 타입  // PlayerCtrl -> PlayerLevel로 위치 이동
    public MODE_TYPE playerLevel = MODE_TYPE.LEVEL1;

    public enum MODE_STATE {IDLE = 1, HURT, LEVELCHANGE};
        // 다친 상태인지 체크 - 다친 상태라면 점프 불가능, 일정 시간동안 공격 안 받음   // PlayerCtrl -> PlayerLevel로 위치 이동
    public MODE_STATE playerState = MODE_STATE.IDLE;

    
    private float lastHitTime =0f;
    private float repeatDamagePeriod = 2.0f;    // #17 다친 후 쿨타임(무적시간)
    private float hurtForce = 10f;        
    private float stopRadius = 15f;             // #75
    
    public AudioClip hurtClip;
    public AudioClip mushroomObtained;          // #36
    public AudioClip leafObtained;              // #36

    private PlayerCtrl playerCtrl;
    private LobbyManager lobbyManager;          // #73 게임 재시작용
    private Music music;                        // #76 

    private Vector2 lifeScale;
// #36 레벨 변경시
    private BoxCollider2D boxCollider2D;        // #36

// #74 플레이어 레벨 1에서 GetHurt해서 죽을 때
    private Rigidbody2D Rbody;                  // #78  플레이어 죽으면 Rbody 끄기 - 중력을 0으로 만들어서
    private Vector3 startPos;
    private Vector3 upPos;     
    private Vector3 downPos;
    private float moveTimer = 0f;
    private float upTimer = 2f;
    private float downTimer = 6f;               // #74 fix: 떨어지는 시간을 더 짧게 해서, Restart도 빠르게 실행되도록

    AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);   // 커브 처리를 이용해 업 다운 적용
    public Sprite deadPlayer;                   // #74 레벨1에서 GetHurt로 플레이어가 죽을 때의 이미지 

    [SerializeField] private string sortingName;
    [SerializeField] private int sortingOrder;

    private bool haveNoLife = false;    // #78 남아있는 목숨 있는지 확인

    void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // #73
        music = GameObject.Find("Music").GetComponent<Music>();    // #73

        boxCollider2D = GetComponent<BoxCollider2D>();  // #36
        Rbody = GetComponent<Rigidbody2D>();            // #78
    }

    void Start()
    {
        lobbyManager.gameStart = true;  //#50 게임 시작했을 때만 남은 시간 줄어들도록
        sortingName = transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName;   
        sortingOrder = transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder;
    }
    public void GetHurt()   // #17 플레이어 다침 - 현재 레벨에 따라 다르게 적용 
    {
        if(Time.time > lastHitTime + repeatDamagePeriod)    // #17 다친 후 일정 시간동안 무적 상태
        {
            switch(playerLevel)
            {
                case MODE_TYPE.LEVEL1 : // 죽음
                    if(!lobbyManager.gameOver)       // 게임 오버 true 설정
                        {
                            Debug.Log("플레이어 죽음");
                            PlayerDie();
                        }


                    break;
                case MODE_TYPE.LEVEL2 : 
                case MODE_TYPE.LEVEL3 :
                    LevelDown();
                    lastHitTime = Time.time;
                    break;

            }
        }
    }

    void LevelDown()       // #17 플레이어 레벨 하락    // TakeDamage() -> LevelDown() 이름 변경
    {
        playerState = MODE_STATE.HURT;         //다치는 순간은 점프하지 못하도록
        Invoke("ReturnToNormal", 0.3f); //0.3초 후 다시 회복
        
        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL2 : 
                playerLevel = MODE_TYPE.LEVEL1;
                playerCtrl.anim.SetTrigger("LevelDown");  // #36 게임 일시정지 하기 전에 실행해야 제대로 작동하는 것처럼 보임

                lobbyManager.StopGame(true, false, 0.7f);  // #76 게임을 1초 동안 멈추기
                Invoke("ChangeLevel", 0.3f);  // #36 LobbyManager의 stopForAMoment가 false가 되고 나서 레벨업 진행되도록 - 약간의 차이를 주기
                AudioSource.PlayClipAtPoint(hurtClip, transform.position);

                break;
            case MODE_TYPE.LEVEL3 : 
                playerLevel = MODE_TYPE.LEVEL2;
                playerCtrl.anim.SetTrigger("LevelDown");  // #36 게임 일시정지 하기 전에 실행해야 제대로 작동하는 것처럼 보임

                lobbyManager.StopGame(true, false, 0.5f);  // #76 게임을 0.5초 동안 멈추기
                Invoke("ChangeLevel", 0.3f);  // #36 LobbyManager의 stopForAMoment가 false가 되고 나서 레벨업 진행되도록 - 약간의 차이를 주기
                AudioSource.PlayClipAtPoint(leafObtained, transform.position);  // #17 레벨 다운 효과음 - (레벨3에서 레벨다운 되는 건,잎 획득 효과음과 같은 효과음)

                break;
        }

        // ChangeLevel();      // #36 레벨 변경될 때 고려되는 요인들 변경
    }

    void ReturnToNormal()   // Invoke로 호출 - IDLE 상태로 돌아오기
    {   
        playerState = MODE_STATE.IDLE;
    }

    public void LevelUp()       // #36 레벨업
    {
        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL1:
                playerLevel = MODE_TYPE.LEVEL2;
                AudioSource.PlayClipAtPoint(mushroomObtained, transform.position); // #36 레벨업
                playerCtrl.anim.SetTrigger("LevelUp");  // #36 게임 일시정지 하기 전에 실행해야 제대로 작동하는 것처럼 보임

                lobbyManager.StopGame(true, false, 0.7f);  // #76 게임을 1초 동안 멈추기
                Invoke("ChangeLevel", 0.3f);  // #36 LobbyManager의 stopForAMoment가 false가 되고 나서 레벨업 진행되도록 - 약간의 차이를 주기
                break;
            
            case MODE_TYPE.LEVEL2:
                playerLevel = MODE_TYPE.LEVEL3;
                AudioSource.PlayClipAtPoint(leafObtained, transform.position); // #36 레벨업
                playerCtrl.anim.SetTrigger("LevelUp");  // #36 게임 일시정지 하기 전에 실행해야 제대로 작동하는 것처럼 보임

                lobbyManager.StopGame(true, false, 0.5f);  // #76 게임을 0.5초 동안 멈추기

                Invoke("ChangeLevel", 0.3f);  // #36 LobbyManager의 stopForAMoment가 false가 되고 나서 레벨업 진행되도록 - 약간의 차이를 주기
                break;
            
            case MODE_TYPE.LEVEL3:
                break;
        }
    }

    public void ChangeLevel()   // #36 레벨 변경 - 콜라이더 y길이(1<->1.6), 해당 오브젝트만 켜기, 그라운드체크 재설정 
                                // 헤드체크 위치는 상관 없음. 플레이어에서 조정하는 경우가 없기 때문에, 태그만 잘 붙어있으면 됨.
    {
        // Debug.Log("//#36 레벨 변경");
        
        Vector2 size = boxCollider2D.size;

        Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식
        Transform secondChild = transform.GetChild(1);
        Transform thirdChild = transform.GetChild(2);

        switch(playerLevel)
        {
            case MODE_TYPE.LEVEL1:                          // 레벨1로 변경됐다면
                size.y = 0.9f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                secondChild.gameObject.SetActive(false);    // 오브젝트 끄고 켜기
                thirdChild.gameObject.SetActive(false);
                firstChild.gameObject.SetActive(true);  

                playerCtrl.groundCheck = firstChild.Find("groundCheck");
                playerCtrl.headCheck = firstChild.Find("headCheck").gameObject;    // #75
                playerCtrl.anim = firstChild.GetComponent<Animator>();  // #36
                playerCtrl.sprite = firstChild.GetComponent<SpriteRenderer>();  // #77

                break;

            case MODE_TYPE.LEVEL2:                          // 레벨2로 변경됐다면
                size.y = 1.6f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                firstChild.gameObject.SetActive(false);     // 오브젝트 끄고 켜기
                thirdChild.gameObject.SetActive(false);
                secondChild.gameObject.SetActive(true);

                playerCtrl.groundCheck = secondChild.Find("groundCheck");
                playerCtrl.headCheck = firstChild.Find("headCheck").gameObject;    // #75
                playerCtrl.anim = secondChild.GetComponent<Animator>(); // #36
                playerCtrl.sprite = secondChild.GetComponent<SpriteRenderer>();  // #77

                break;
            
            case MODE_TYPE.LEVEL3:                          // 레벨3로 변경됐다면
                size.y = 1.6f;                              // 콜라이더 길이 맞추기
                boxCollider2D.size = size;

                firstChild.gameObject.SetActive(false);     // 오브젝트 끄고 켜기
                secondChild.gameObject.SetActive(false);
                thirdChild.gameObject.SetActive(true);  

                playerCtrl.groundCheck = thirdChild.Find("groundCheck");
                playerCtrl.headCheck = firstChild.Find("headCheck").gameObject;    // #75
                playerCtrl.anim = thirdChild.GetComponent<Animator>();  // #36
                playerCtrl.sprite = thirdChild.GetComponent<SpriteRenderer>();  // #77

                break;
        }

    }
    
    public void PlayerDie(bool enterDieZone = false, bool _timeUp = false)     // #78 Zone.cs 에서 접근   
    {
    // #75
        this.gameObject.layer = 19;         // 죽은 플레이어 - 그 어떤 것과도 부딪히지 않도록 
        playerCtrl.headCheck.GetComponent<BoxCollider2D>().enabled = false;    // #75 플레이어의 headCheck의 콜라이더도 비활성화 - 블록과 충돌처리 일어나지 않도록
        lobbyManager.gameOver = true;       // 게임 오버 true 설정
        lobbyManager.gameStart = false;     // #50 게임 시작했을 때만 남은 시간 줄어들도록

        music.PlayerDie();              // #76

        if (GameMgr.Mgr.life > 0)
            GameMgr.Mgr.life -= 1;          // #78 플레이어 목숨 줄어들도록 - lobbyManager.CheckLife();는 나중에 시작하도록       // #78 남은 생명 수 체크
        else if (GameMgr.Mgr.life <= 0)     // #78 추가: 죽었는데 이미 목숨이 0이하인 상태라면
        {
            haveNoLife = true;
            Debug.Log("//#78 줄어들 목숨 없음 = 완전히 게임 오버 상태");
        }
            

        if(enterDieZone)    // #78 enterDieZone = true이면 플레이어 위치 고정되도록
        {
            Debug.Log("//#78 enterDieZone = true");
            Rbody.gravityScale = 0;
            // Rbody.velocity = new Vector2(0f, 0f);    // 떨어질 때 가속도 적용되지 않도록 - 그냥 DieZone 위치 안에 머물게 하기    // #78: FollowCamera에서 if조건문으로 설정하도록
            
            // Rigidbody2D의 constraints를 사용하여 움직임을 제한
            Rbody.constraints = RigidbodyConstraints2D.FreezeAll;
            Invoke("GameRestart", downTimer);        // Die Zone에 들어간 후, n초 뒤에 게임 재시작하도록 - 재시작 시간은 downTimer와 동일하게 
        }
        else    // #78 enterDieZone = false이면 PlayerUpDown 실행되도록
        {
            // #74 플레이어 위로 올라갔다가 아래로 떨어지도록
            playerCtrl.anim.enabled = false;    // 애니메이션 멈춰서 플레이어 이미지 바뀌도록

            // #50 어느 레벨에서든 플레이어 죽는 이미지가 동일하게 나타나도록
            Transform firstChild = transform.GetChild(0);   // 자식 오브젝트 위치 중 0번째 자식
            Transform secondChild = transform.GetChild(1);
            Transform thirdChild = transform.GetChild(2);

            switch(playerLevel)
            {
                case MODE_TYPE.LEVEL1: 
                    firstChild.GetComponent<SpriteRenderer>().sprite = deadPlayer;   // 이미지 변경
                    break;
                case MODE_TYPE.LEVEL2:
                    secondChild.GetComponent<SpriteRenderer>().sprite = deadPlayer;   // 이미지 변경
                    break;
                case MODE_TYPE.LEVEL3:
                    thirdChild.GetComponent<SpriteRenderer>().sprite = deadPlayer;   // 이미지 변경
                    break;
            }

            startPos = transform.position;

            upPos = startPos;   // 죽었을 때의 해당 위치
            upPos.y += 2f;

            downPos = startPos; // 죽었을 때의 해당 위치
            downPos.y = -8f;    // #74 fix: -20까지 갈 필요 없겠다 -> -8로 변경

            StartCoroutine(PlayerUpDown(_timeUp, haveNoLife)); // #74 // #78
        }

        StopFireball(); // #75
    }


    void StopFireball() // #75 플레이어 주위의 Fireball 찾아서 멈추게 하기 위한 목적 ========================================
    {
        Collider2D[] colls = Physics2D.OverlapCircleAll(transform.position, stopRadius);    
        foreach(Collider2D coll in colls)
        {   
            Rigidbody2D rbody = coll.GetComponent<Rigidbody2D>();
            if(rbody != null)
            {   
                if(rbody.gameObject.tag == "EnemyWeapon")
                {
                    // rbody.enabled = false;
                    // rbody.gameObject.SendMessage("StopFireball", SendMessageOptions.DontRequireReceiver);   //잘못된 방식 - Fireball 오브젝트에는 따로 스크립트가 없기 때문에 

                    rbody.constraints = RigidbodyConstraints2D.FreezeAll;
                }
            }
        }
    }

    IEnumerator PlayerUpDown(bool _timeUp, bool _gameCompletelyOVer = false)  // #74 플레이어 위로 올라갔다가 아래로 떨어지도록 // #78 완전히 게임 오버 상태라면, Restart 되지 않도록
    {
        Debug.Log("// #74 플레이어 업다운 시작");
        while(true)
        {
            if(moveTimer <= upTimer)
                transform.localPosition = Vector3.Lerp(startPos, upPos, curve.Evaluate(moveTimer/upTimer));
            else if(moveTimer <= downTimer) //upTimer 초과이면서 downTimer 이하일 때
                transform.localPosition = Vector3.Lerp(upPos, downPos, curve.Evaluate(moveTimer/downTimer));
            else    // 시간이 모두 지났으면
            {
                Rbody.constraints = RigidbodyConstraints2D.FreezeAll;   // #74 플레이어 이동하지 않도록 고정

                moveTimer = 0f; // 다시 원상복구
                if(_timeUp)
                {
                    lobbyManager.ShowTimeUpWindow();  // #50 플레이어 바닥으로 꺼지면 TIME-UP 화면 나타나도록
                }

                if (_gameCompletelyOVer) // #78 완전히 게임 오버 상태라면(목숨 0 상태에서 죽은 거라면), scOpen 화면으로 넘어가도록
                {
                    lobbyManager.GameCompletelyOver();
                    haveNoLife = false; // #78 추가: _gameCompletelyOVer 설정했으니까, bool형 변수 변경하기
                }    
                else
                    Invoke("GameRestart", 2f);  // #73 #50

                yield break;    // 코루틴 탈출
            }
            moveTimer += Time.deltaTime;

            yield return null;
        }
    }

    void GameRestart()
    {
        lobbyManager.RestartGame(); // #73
    }
}
