using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyZone : MonoBehaviour    // #68 이 DestroyZone 안에 들어온 모든 오브젝트들은 소멸 - 데이터 낭비를 아끼기 위함
{

    private void OnTriggerEnter2D(Collider2D col) 
    {
        if((col.gameObject != null) && (col.gameObject.tag != "Player"))    // #74 에러 방지 - 플레이어는 Destroy 되지 않도록
            Destroy(col.gameObject);    
    }

}
