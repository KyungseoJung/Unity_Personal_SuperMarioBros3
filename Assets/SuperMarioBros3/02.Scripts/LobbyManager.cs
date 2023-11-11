using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;          // #32

using UnityEngine.UI;                       // #35

public class LobbyManager : MonoBehaviour   // #32  각종 사운드, (점수, 목숨, 남은 시간) UI 관리하는 클래스 생성
{
    private PlayerCtrl playerCtrl;          // #77 spriteRenderer 접근 목적
    private Music music;                    // #77 일시정지 할 때, BGM 멈추도록
    public Text txtScore;                   // #35 점수 표시
    public Text txtTimeLeft;                // #50 남은 시간 표시
    public Text txtLife;                    // #61 생명 표시
    
    public Image ImgFinalGet;               // #53 UI상에 나타나는 Goal 지점 획득 아이템 이미지
    
    public Sprite[] SpriteFinalGetItem;          // #53 Goal 지점의 아이템 이미지들 종류별로
    /*
    0: Flower
    1: Mushroom
    2: Star
    */

    private Goal.GOAL_ITEM_TYPE finalGetItemType;
    

    private float timeLeftFloat;            // #50 계산(측정) 목적 float형 변수
    private float delayTime;                // #76 n초 후에 게임 재시작 할 건지 나타내는 타이머
    private float gameRestartTime;          
    private float originalTime;             // #77 기존 Time.timeScale
    private int timeLeftInt;                // #50 표시 목적 int형 변수

    public bool gameOver = false;           // #75 
    private bool stopForAMoment = false;    // #76 게임 중지 여부 확인
    private bool pauseGame = false;         // #77 게임 일시정지 여부 확인

    public GameObject[] fastIndicator;      // #41 속도 표시계 (삼각형) - 6개([0]부터 [5]까지)
    public GameObject powerIndicator;       // #41 속도 표시계 (P글자. 파워)
    public GameObject pauseWindow;          // #77 일시정지 PAUSE 문구 윈도우
    public GameObject btnGameStart;         // #53 게임 시작 버튼
    public GameObject[] gameClearUIObjs;    // #53 Goal 지점 게임 클리어 UI - 총 3개
    /*
    [0]: 1번째 줄 텍스트
    [1]: 2번째 줄 텍스트
    [2]: Goal 지점 획득 이미지
    */

    public AudioClip pausingSFX; // #77 pausingSFX (일시정지 할 때)

    IEnumerator enumerator;

    void Awake()
    {
        music = GameObject.FindGameObjectWithTag("Music").GetComponent<Music>();
    }
    void Start()
    {
        //SceneManager.LoadScene("scStage1");       // #53 버튼 눌러야 scHome씬부터 실행하도록
    
        timeLeftFloat = 300f;                    // #50 남은 시간 - 첫 시작은 300초

        if(pauseWindow.activeSelf)              // #77 PAUSE 윈도우 창 꺼진 채로 시작하도록
            pauseWindow.SetActive(false);   
        
        foreach(GameObject obj in gameClearUIObjs)  // #53 첫 시작할 땐, 비활성화
        {
            if(obj.activeSelf)
                obj.SetActive(false);
        }

        originalTime = Time.timeScale;          // #77
    }

    void Update()
    {        
        if(!gameOver)
            CheckTimeLeft();                    // #50 남은 시간 체크 // #53 gameOver인 상태에서는 시간 흘러가지 않도록

        if(stopForAMoment)
        {

            //Debug.Log("//#76-2 Time.realtimeSinceStartup : " + Time.realtimeSinceStartup);    

            if(Time.realtimeSinceStartup > gameRestartTime)
            {
                ReleaseStopState();
                stopForAMoment = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.Return))    // #77 엔터 키 누르면 Pause
        {
            if(pauseGame)   // 이미 일시정지 한 상태라면 - 멈춘 상태 풀기
            {
                ReleaseStopState();
                pauseGame = false; 
            }
            else    // 일시정지 하지 않은 상태라면 - 일시정지 적용하기
            {
                AudioSource.PlayClipAtPoint(pausingSFX, transform.position);    // 일시정지 효과음 - StopGame 이후에 실행하면, 시간이 아예 멈춰버린 후이기 때문에, 효과음이 아예 적용되지 않을 때가 있음
                StopGame(false, true);   

                pauseGame = true; 
            }
        }    

    }

    public void CheckPoint()                // #35 점수, 목숨, 코인 확인용 함수 - GameMgr에서 점수 획득할 때마다 실행
    {
        txtScore.text = GameMgr.Mgr.score.ToString("D7");   // 7자리로 표시
        
        // Debug.Log("#35 포인트 체크");
    }

    private void CheckTimeLeft()    // #50
    {
        if(timeLeftFloat - Time.deltaTime >0)   // 계산 값이 0보다 크다면, 계산 적용~
        {
            timeLeftFloat -= Time.deltaTime;
        }
        timeLeftInt = (int) timeLeftFloat;

        txtTimeLeft.text = timeLeftInt.ToString("D3");

    }

    public void CheckLife()        // #61
    {
        txtLife.text = GameMgr.Mgr.life.ToString("D1"); // 1자리로 표시
        Debug.Log("#61 남은 생명 수 체크");
    }

    public void SetSpeedUp(int num, bool _max = false)           // #41 속도 표시계 설정 - [0]부터 [num]까지의 오브젝트는 활성화 - PlayerCtrl에서 접근
    {
        StopSpeedDown();                        // 이전에 작동하던 코루틴과 중복 실행되지 않도록

        // for(int i=num+1; i<6; i++)              // [num+1]부터 [5]까지의 오브젝트는 비활성화
        // {
        //     fastIndicator[i].SetActive(false);
        // }

        for(int i=0; i<num+1; i++)              // [0]부터 [num]까지의 오브젝트는 활성화
        {
            fastIndicator[i].SetActive(true);
        }

        if(_max)    
            powerIndicator.SetActive(true);     // 최고 속도로 달리고 있다면, 활성화
        else
            powerIndicator.SetActive(false);    // 최고 속도가 아니라면, 비활성화
    }

    public void SetSpeedDown()
    {
        // Debug.Log("//#41 스피드 다운");

        enumerator = SpeedDown();

        StopSpeedDown();                        // 이전에 작동하던 코루틴과 중복 실행되지 않도록
        StartCoroutine(enumerator);
    }

    public IEnumerator SpeedDown()
    {   
        yield return new WaitForSeconds(0.5f);  // 0.5초 후 속도표시계가 천천히 꺼지기 시작하도록

        powerIndicator.SetActive(false);    
        
        for(int i=5; i>=0; i--)
        {
            // Debug.Log(i + "번째 비활성화");

            fastIndicator[i].SetActive(false);      
            yield return new WaitForSeconds(1f);  // 1초마다 속도표시계가 천천히 꺼지도록
        }
    }

    private void StopSpeedDown()
    {
        StopCoroutine(enumerator);             
    }

    public void RestartGame()   // #73 플레이어 죽었을 때
    {
        //SceneManager.LoadScene("scOpen");   // #73 fix 씬 새로 시작 - scOpen도 새로 로드해야 BGM도 다시 시작함  
        // #53 scOpen 씬부터 불러와야 할 것 같아서 순서 변경
        music.GameStart();                  // #53 fix scOpen 씬 자체를 다시 불러오는 방법 대신, 게임 시작할 때 실행되는 함수들을 직접 실행해주기
        SceneManager.LoadScene("scStage1"); // 씬 새로 시작


        timeLeftFloat = 300f;               // 남은 시간 - 첫 시작은 300초
        gameOver = false;                   // #73 fix
    }

    public void LevelCompleted()    // #53 레벨 성공
    {
        // Debug.Log("//#53 fix LobbyManager.cs: LevelCompleted 함수 실행");

        Invoke("ShowClearUIFirst", 1.0f);
        Invoke("ShowClearUISecond", 2.0f);
        Invoke("ShowClearUIThird", 3.0f);
        Invoke("MoveToLobbyScene", 5.0f);   // #53 로비씬 이동 타이밍 늦추기
    }

    private void ShowClearUIFirst()
    {
        Debug.Log("//#53 ShowClearUIFirst");
        gameClearUIObjs[0].SetActive(true);
    }
    private void ShowClearUISecond()
    {
        Debug.Log("//#53 ShowClearUISecond");
        gameClearUIObjs[1].SetActive(true);
    }
    private void ShowClearUIThird()
    {
        Debug.Log("//#53 ShowClearUIThird");
        gameClearUIObjs[2].SetActive(true); // 획득 이미지 나타나게 하기
    }

    private void MoveToLobbyScene()
    {
        Debug.Log("//#53 MoveToLobbyScene");

        SceneManager.UnloadSceneAsync("scStage1");    // 비동기 방식 - 현재의 씬만 이렇게 Unload 할 수 있음
                                                    // Unity개인프로젝트 - 공부_화면전환 내용 중
                                                    // 비동기 방식은 씬 전환이 완료되기 전에도 다른 작업을 수행할 수 있으므로 유저 경험을 향상시킬 수 있다

        foreach(GameObject obj in gameClearUIObjs)  // #53 fix : 로비 화면으로 이동하기 전, UI 오브젝트 안 보이도록 비활성화 
        {
            Debug.Log("//#53 fix: " + obj + "비활성화");
            if(obj.activeSelf)
                obj.SetActive(false);
        }

        SceneManager.LoadScene("scLobby");         // Home 씬으로 이동 -> #53 scStage1으로 이동하도록 -> #53 fix: scOpen으로 이동하도록 -> # 53 fix: scHome으로 이동하도록
        btnGameStart.SetActive(true);               // #53 fix: 버튼 다시 활성화
    }
    public void StopGame(bool _replay, bool _pause, float _timer = 0f)   // #76 게임 잠시 멈춤   // #77 게임 일시정지
    {
        Time.timeScale = 0;

        if(_replay) // 다시 시작할 거라면 _timer 초 후에 다시 시작하도록
        {
            Debug.Log("//#76 게임 " + _timer + "초 후에 재시작");

            gameRestartTime = Time.realtimeSinceStartup + _timer;  // n초 후에 재시작하도록
            Debug.Log("//#76-2 gameRestartTime : " + gameRestartTime);    

            stopForAMoment = true;   
            // StartCoroutine(ReleaseStopState(_timer)); // 게임이 아예 멈춘 후이기 때문에, Invoke로 하면 실행이 안돼
            music.MusicPauseStart();    // #76 fix BGM 일시 정지 시작

        }

        if(_pause)  // #77
        {
            pauseWindow.SetActive(true);    
            HideCharacters(true);  
            music.MusicPauseStart();    // #77 BGM 일시 정지 시작
        }
    }

    void ReleaseStopState() // #76 게임 시작 - 멈춘 것 풀기
    {
        Debug.Log("//#76 멈춘 상태 풀기");
        pauseWindow.SetActive(false);  
        HideCharacters(false);
        music.MusicPauseEnd();    // #77 BGM 일시 정지 종료
        // yield return new WaitForSeconds(_timer); 
        Time.timeScale = originalTime;
    }

    void HideCharacters(bool _hide)   // #77 캐릭터들(MainPlayer, Enemy) 숨기기 or 드러내기
    {
    // 플레이어의 spriteRenderer 접근 - 레벨마다 지정되는 SpriteRenderer가 다르기 때문에, playerCtrl로 접근하는 것================================
        if(playerCtrl == null)
            playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();

        switch(_hide)
        {
            case true :     // 숨기기
                playerCtrl.sprite.sortingLayerName = "Default";
                break;
            case false :    // 드러내기
                playerCtrl.sprite.sortingLayerName = "Character";
                break;
        }

    // Enenmy의 spriteRenderer 접근 ================================
        GameObject[] enemyObjs =  GameObject.FindGameObjectsWithTag("Enemy");

        switch(_hide)
        {
            case true :     // 숨기기
                foreach(GameObject obj in enemyObjs)
                {
                    obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                }
                break;
            case false :    // 드러내기
                foreach(GameObject obj in enemyObjs)
                {
                    obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingLayerName = "Character";
                }
                break;
        }
    // EnenmyWeapon의 spriteRenderer 접근 ================================
        GameObject[] weaponObjs =  GameObject.FindGameObjectsWithTag("EnemyWeapon");

        if(weaponObjs != null)
        {
            switch(_hide)
            {
                case true :     // 숨기기
                    foreach(GameObject obj in weaponObjs)
                    {
                        obj.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
                    }
                    break;
                case false :    // 드러내기
                    foreach(GameObject obj in weaponObjs)
                    {
                        obj.GetComponent<SpriteRenderer>().sortingLayerName = "EnemyWeapon";
                    }
                    break;
            }
        }
    }

    public void StartGame()    // #53 게임 시작하기 - btnGameStart에 연결
    {
        btnGameStart.SetActive(false);
        SceneManager.LoadScene("scStage1");

        music.GameStart();      // 게임 기본 BGM 시작
        gameOver = false;        // #53 fix false 처리 해줘야 Player, Enemy 등 정상적으로 움직임

    }

    public void GetFinalItem(Goal.GOAL_ITEM_TYPE _type)  // #53 Goal 지점에 도착 - 획득한 FinalGetItem 이미지 띄우기
    {
        switch(_type)
        {
            case Goal.GOAL_ITEM_TYPE.FLOWER:
                ImgFinalGet.sprite = SpriteFinalGetItem[0];
                break;
            case Goal.GOAL_ITEM_TYPE.STAR:
                ImgFinalGet.sprite = SpriteFinalGetItem[1];
                break;
            case Goal.GOAL_ITEM_TYPE.MUSHROOM:
                ImgFinalGet.sprite = SpriteFinalGetItem[2];
                break;
        }

    }
    // public void StopGame(bool _replay, float _timer)    // #76
    // {
    //     StartCoroutine(StopGameIEnumerator(true, _timer));
    // }

    // IEnumerator StopGameIEnumerator(bool _replay, float _timer)   // #76 게임 멈춤
    // {
    //     Time.timeScale = 0;

    //     if(_replay) // 다시 시작할 거라면 _timer 초 후에 다시 시작하도록
    //     {
    //         float stopTimer = 0f;
    //         Debug.Log("//#76 게임 " + _timer + "초 후에 재시작");

    //         while(true) // 게임이 아예 멈춘 후이기 때문에, Invoke로 하면 실행이 안돼
    //         {
    //             if(stopTimer > _timer)
    //             {
    //                 ReleaseStopState();
    //                 yield break;
    //             }
    //             stopTimer += Time.deltaTime;

    //             yield return null;
    //         }             
    //     }
    // }

    // void ReleaseStopState() // #76 게임 시작 - 멈춘 것 풀기
    // {
    //     Debug.Log("//#76 게임 재시작");
    //     Time.timeScale = 1;
    // }



}
