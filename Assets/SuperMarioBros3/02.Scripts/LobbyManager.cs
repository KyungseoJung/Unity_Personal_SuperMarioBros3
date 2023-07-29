using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;          // #32

using UnityEngine.UI;                       // #35

public class LobbyManager : MonoBehaviour   // #32  각종 사운드, (점수, 목숨, 남은 시간) UI 관리하는 클래스 생성
{
    public Text txtScore;                   // #35 점수 표시
    public Text txtTimeLeft;                // #50 남은 시간 표시
    private float timeLeftFloat;            // #50 계산(측정) 목적 float형 변수
    private int timeLeftInt;                // #50 표시 목적 int형 변수

    public GameObject[] fastIndicator;      // #41 속도 표시계 (삼각형) - 6개([0]부터 [5]까지)
    public GameObject powerIndicator;       // #41 속도 표시계 (P글자. 파워)

    IEnumerator enumerator;

    void Start()
    {
        SceneManager.LoadScene("scStage1");        
    
        timeLeftFloat = 300f;                    // #50 남은 시간 - 첫 시작은 300초
    }

    void Update()
    {
        CheckTimeLeft();                    // #50 남은 시간 체크
    }

    public void CheckPoint()                // #35 점수, 목숨, 코인 확인용 함수 - GameMgr에서 점수 획득할 때마다 실행
    {
        txtScore.text = GameMgr.Mgr.score.ToString("D7");   // 7자리로 표시
        
        Debug.Log("#35 포인트 체크");
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
        yield return new WaitForSeconds(0.5f);  // 0.5초마다 속도표시계가 천천히 꺼지도록

        powerIndicator.SetActive(false);    
        
        for(int i=5; i>=0; i--)
        {
            // Debug.Log(i + "번째 비활성화");

            fastIndicator[i].SetActive(false);      
            yield return new WaitForSeconds(1f);  // 0.5초마다 속도표시계가 천천히 꺼지도록
        }
    }

    private void StopSpeedDown()
    {
        StopCoroutine(enumerator);             
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
}
