using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour  // #11 적 머리 밟았을 때, 적을 죽이는 기능
{
    private PlayerCtrl playerCtrl;
    private bool isDie;             // 죽었나 확인

    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(isDie)       // 이미 죽었으면 아래 코드 실행 X
            return;

        if(other.gameObject.tag == "Player")    // #11
        {
            if(playerCtrl.steppingOnEnemy)  // 만약 플레이어가 Enemy의 머리를 밟은 거라면
            {
                Debug.Log("#11 플레이어가 Enemy 머리 밟음");
                isDie = true;
            }
            // else
            //     Debug.Log("#11 플레이어랑 그냥 부딪힘");

        }        
    }

    
}
