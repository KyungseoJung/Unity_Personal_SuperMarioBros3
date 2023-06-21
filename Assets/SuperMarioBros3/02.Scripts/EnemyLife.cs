using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour  // #11 적 머리 밟았을 때, 적을 죽이는 기능
{
    private PlayerCtrl playerCtrl;
    private bool isDie;             // 죽었나 확인
    
// #15 플레이어에게 머리 밟혔는지 확인용 & 등껍질로 변신하도록
    public bool beStepped = false;          // PlayerCtrl에서 true, false 적용됨
    private EnemyCtrl enemyCtrl;    // #15
    private GameObject shell;       // #15
    private GameObject body;        // #15
    private BoxCollider2D boxCollider2D; // #15

    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
        
        enemyCtrl = GetComponent<EnemyCtrl>();      // #15
        boxCollider2D = GetComponent<BoxCollider2D>();

    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(isDie)       // 이미 죽었으면 아래 코드 실행 X
            return;

        if(other.gameObject.tag == "Player")    // #11
        {
            if(beStepped)  // #15 만약 플레이어가 Enemy의 머리를 밟은 거라면
            {
                Debug.Log("#11 플레이어가 Enemy 머리 밟음");
                switch(enemyCtrl.enemyType)
                {
                    case EnemyCtrl.ENEMY_TYPE.GOOMBA : 
                        isDie = true;
                        break;
                    
                    case EnemyCtrl.ENEMY_TYPE.TURTLE : 
                        TransformIntoShell();       // #15 등껍질로 변신
                        break;
                }
            }
            // else
            //     Debug.Log("#11 플레이어랑 그냥 부딪힘");

        }        
    }

    private void TransformIntoShell()    // #15 등껍질로 변신
    {
        shell = transform.GetChild(2).gameObject;
        body = transform.GetChild(0).gameObject;

        shell.SetActive(true);      // 등껍질 이미지 활성화
        body.SetActive(false);      // 기존 바디 비홣성화

        Vector2 size = boxCollider2D.size;  // 등껍질로 사이즈 맞추기
        size.y = 1f;
        boxCollider2D.size = size;
    }

    // private void OnTriggerEnter2D(Collider2D col)   
    // {
    //     switch(enemyCtrl.enemyType)
    //     {
    //         case EnemyCtrl.ENEMY_TYPE.GOOMBA : 
    //         case EnemyCtrl.ENEMY_TYPE.TURTLE : 
    //             if(col.gameObject.tag == "Player")  // #15 굼바, 거북 Enemy의 머리 위에 있는 headCheck 트리거 콜라이더를 (플레이어에게) 밟혔을 때 
    //             {
    //                 beStepped = true;
    //             }
    //             break;
    //     }
    // }
}
