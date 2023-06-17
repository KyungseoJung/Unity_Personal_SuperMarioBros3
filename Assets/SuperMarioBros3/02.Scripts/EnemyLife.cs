using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour  // #11 적 머리 밟았을 때, 적을 죽이는 기능
{
    [SerializeField]
    private PlayerCtrl playerCtrl;

    private void Awake() 
    {
        playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.tag == "Player")    // #11
        {
            if(playerCtrl.steppingOnEnemy)  // 만약 플레이어가 Enemy의 머리를 밟은 거라면
            {
                Debug.Log("#11 플레이어 머리 밟힘");
            }
            else
                Debug.Log("#11 플레이어랑 그냥 부딪힘");
        }        
    }

}
