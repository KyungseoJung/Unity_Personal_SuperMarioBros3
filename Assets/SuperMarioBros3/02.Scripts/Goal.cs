using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour   // #52 골 아이템 변경
{
    public enum GOAL_ITEM_TYPE {FLOWER = 1, STAR, MUSHROOM}
    public GOAL_ITEM_TYPE goalItemType = GOAL_ITEM_TYPE.FLOWER;
// #53 골 지점에 닿았을 때 상황
    // private GOAL_ITEM_TYPE finalGetItemType;
    private Animator anim;                   // #53
    private GameObject goalItemTransform;    // #53 

    private LobbyManager lobbyManager;      // #53
    void Awake()
    {
        anim = GetComponent<Animator>();    // #53
        goalItemTransform = transform.GetChild(0).gameObject;

        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();    // 오브젝트 이름도 LobbyManager이기 때문에
    }

    void ChangeGoalItem(GOAL_ITEM_TYPE _type)   // 애니메이터에서 설정 - 골 이미지가 바뀜에 따라 변수의 값도 바뀌도록
    {
        goalItemType = _type;
    }
    
    public void ReachTheGoal() // #53 플레이어가 골 지점에 닿았을 때 실행 - 애니메이션 멈추고, 한 가지 그림으로 고정되도록
    {
        // finalGetItemType = goalItemType;    // 가장 마지막 골 아이템 타입을 최종 획득 아이템 타입으로 지정

        switch(goalItemType)
        {
            case GOAL_ITEM_TYPE.FLOWER : 
                anim.SetTrigger("GetFlower");
                break;

            case GOAL_ITEM_TYPE.STAR :
                anim.SetTrigger("GetStar");
                break;

            case GOAL_ITEM_TYPE.MUSHROOM :
                anim.SetTrigger("GetMushroom");
                break;
        }

        lobbyManager.GetFinalItem(goalItemType);    // #53 Game Clear UI에 나올 아이템 이미지
    }

    void AlreadyGetItem()   // #53 이미 아이템 획득함
    {
        // anim.enabled = false;           // 애니메이션 자체가 이제 필요 없어졌으니, 아예 꺼놓기 
                                            // 에러 발생해서 주석 처리 - 어차피 게임 종료돼서 화면 전환되니까 굳이 하지 않아도 될 듯
        goalItemTransform.SetActive(false); // GoalItem 비활성화해서 안 보이도록
    }


}
