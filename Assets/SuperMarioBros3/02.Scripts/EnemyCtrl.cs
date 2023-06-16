using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCtrl : MonoBehaviour  // #9 몬스터 움직임
{
// 몬스터 종류
    public enum ENEMY_TYPE {GOOMBA = 1, TURTLE, FLOWER};
    public enum WINGS_TYPE {YES = 1, NO};    // 날개 달려 있는지

    public ENEMY_TYPE enemyType;
    public WINGS_TYPE wingsType;

// 몬스터 이동
    private int enemyDir = -1;     // 오른쪽 : 1, 왼쪽 : -1 // 처음엔 왼쪽으로 이동
    public float moveSpeed;        // 이동 속도
    private Rigidbody2D rBody;      

    void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA :
            case ENEMY_TYPE.TURTLE : 
                moveSpeed = 2f;
                break;
        }
    }

    void FixedUpdate()
    {
        switch(enemyType)
        {
            case ENEMY_TYPE.GOOMBA : 
            case ENEMY_TYPE.TURTLE : 
                rBody.velocity = new Vector2(enemyDir * moveSpeed, rBody.velocity.y);
                    // #9 Mathf.Sign : 부호를 반환하는 함수
                break;
        }
    }

    void Flip()
    {
        enemyDir *= -1;

        Vector3 enemyScale = transform.localScale;
        enemyScale.x *= -1;
        transform.localScale = enemyScale;
    }
}
