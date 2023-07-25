using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour   // #47 제일 최상위 부모 오브젝트에만 넣기
{
    public enum TELEPORT_TYPE {PIPE1_IN =1, PIPE1_OUT, PIPE2_IN, PIPE2_OUT }; // 순간이동 타입
    public enum WORKING_KEYDIR {UP = 1, DOWN, LEFT, RIGHT};                   // 작동하는 키 버튼
    public TELEPORT_TYPE teleportType;   // 인스펙터 창에서 설정
    public WORKING_KEYDIR workingKeyDir; // 인스펙터 창에서 설정

    [SerializeField]
    private Transform destTransform;     // 인스펙터 창에서 설정

    private FollowCamera followCam;      // #48 순간이동함에 따라 카메라 위치 조정
    private PlayerCtrl playerCtrl;       // #48 플레이어 상태(지하에 있나, 아닌가) bool형 접근

    // void Awake()
    // {
    //     // childTransforms = GetComponentsInChildren<Transform>();   
    //     // 또는
    //     GameObject[] objs = GameObject.FindGameObjectsWithTag("Teleport");

    //     int i = 0;
    //     foreach(GameObject obj in objs)
    //     {
    //         if(obj.GetComponent<Teleport>() == null)
    //         {
    //             Debug.Log("//#47 Teleport 클래스 없음. null임");
    //             continue;
    //         }
    //         obj.GetComponent<Teleport>().teleportType = TELEPORT_TYPE.PIPE1_IN + i; 
    //         childTransforms[i] = obj.transform;
    //         i++;
    //     }
    // }
    void Awake()
    {
        followCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();    // #44
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();         // #48  
    }

    public Vector3 StartTeleporting()    // 빠져 나가는 파이프의 위치를 반환해주는 함수 - PlayerCtrl에서 실행
    {
        Vector3 destPos = new Vector3(0, 0, 0);
        switch(teleportType)
        {
            case TELEPORT_TYPE.PIPE1_IN : 
                playerCtrl.isInUnderground = true;
                destPos = destTransform.position;
                // Debug.Log("//#47 파이프1에서 순간이동 시작");

                followCam.InUnderground();  // #48 지하로 들어가기 - 카메라 범위도 같이 이동
                break;
            case TELEPORT_TYPE.PIPE2_IN : 
                playerCtrl.isInUnderground = false;
                destPos = destTransform.position;
                // Debug.Log("//#47 파이프2에서 순간이동 시작");

                followCam.OutUnderground(); // #48 지하에서 빠져나오기 - 카메라 범위도 같이 이동
                break;
        }
        return destPos;
    }



}
