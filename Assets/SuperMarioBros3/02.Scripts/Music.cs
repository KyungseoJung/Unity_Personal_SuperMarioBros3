using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour  // #51 
{

    public AudioSource gameMusicArr;
    public AudioClip[] audioClips;
    /*
    0 : mainMusic
    1 : 
    2 : 
    3 : 
    4 : 
    5 : 
    */

    void Awake()
    {
        gameMusicArr = gameObject.AddComponent<AudioSource>(); // 오디오소스 없기 때문에, 추가해서 지정해줘야 함
    }

    void Start()
    {
        MainMusicOn();
    }

    public void MainMusicOn()
    {   
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[0];
        gameMusicArr.Play();
    }


}
