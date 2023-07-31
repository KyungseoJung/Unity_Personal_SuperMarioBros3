using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour   // #52 골 아이템 변경
{
    public enum GOAL_ITEM_TYPE {FLOWER = 1, STAR, MUSHROOM}
    public GOAL_ITEM_TYPE goalItemType = GOAL_ITEM_TYPE.FLOWER;

    void ChangeGoalItem(GOAL_ITEM_TYPE _type)   // 애니메이터에서 설정 - 골 이미지가 바뀜에 따라 변수의 값도 바뀌도록
    {
        goalItemType = _type;
    }


}
