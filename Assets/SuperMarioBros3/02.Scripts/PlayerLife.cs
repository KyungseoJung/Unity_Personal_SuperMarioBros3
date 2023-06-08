using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    private float initLife = 100.0f;    
    public float HP = 100f;
    private float lastHitTime =0f;
    private float repeatDamagePeriod = 2.0f;
    private float hurtForce = 10f;        
    
    public AudioClip hurtClip;

    private PlayerCtrl playerCtrl;
    private Vector2 lifeScale;

    void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        HP = initLife;
    }

    public void GetHurt(Transform weapon, float damageValue = 20f)
    {
        if(Time.time > lastHitTime + repeatDamagePeriod)
        {

            if(HP - damageValue> 0) //다침
            {   
                TakeDamage(weapon.transform, damageValue);
                lastHitTime = Time.time;
            }
            else    //죽음
            {
                HP = 0;
            }
        }
    }

    void TakeDamage(Transform weapon, float damageValue = 20f)
    {
        playerCtrl.playerState = PlayerCtrl.MODE_STATE.HURT;         //다치는 순간은 점프하지 못하도록
        Invoke("ReturnToNormal", 0.3f); //0.3초 후 다시 회복
        
        HP -= damageValue;

        AudioSource.PlayClipAtPoint(hurtClip, transform.position);
    }

    void ReturnToNormal()   // Invoke로 호출
    {   
        playerCtrl.playerState = PlayerCtrl.MODE_STATE.HURT;
    }

    
}
