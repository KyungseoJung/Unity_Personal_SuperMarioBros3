using System.Collections;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()    
    {
        // #32 이 게임오브젝트가 사라지지 않도록
        DontDestroyOnLoad(this.gameObject);
        
    }
}
