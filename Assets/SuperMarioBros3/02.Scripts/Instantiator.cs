using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiator : MonoBehaviour   // #3 특정 오브젝트를 특정 위치에 생성해주는 클래스
{
    public GameObject instObj;   // 생성할 오브젝트
    public float yValueDiff =0;      // 생성할 오브젝트의 y축 위치 차이(이 스크립트가 들어있는 오브젝트 기준)
    public Vector3 instPos;      // 최종 생성될 위치
    public bool instOnAwake;      // 인스펙터에서 체크시 Awake() 에서 instantiateObj 오브젝트를 일정 시간 딜레이후 생성
    public float awakeInstDelay;  // Awake() 에서 사용되는 생성을 위한 딜레이 타임

    void Awake()
    {
        if(instOnAwake)
        {
            Invoke("InstantiateObj", awakeInstDelay);
        }
    }
    
    void InstantiateObj()
    {
        instPos = transform.position;
        instPos.y += yValueDiff;        // 설정한 y축 위치 차이만큼 더하기(or 빼기)

        Instantiate(instObj, instPos, Quaternion.identity);     //Quaternion.identity : 회전량이 0인 상태의 쿼터니언    //단위행렬이랑 비슷한 느낌~
    }
}
