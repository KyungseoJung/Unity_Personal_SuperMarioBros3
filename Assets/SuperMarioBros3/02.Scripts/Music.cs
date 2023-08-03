using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour  // #51 
{

    public AudioSource gameMusicArr;
    public AudioClip[] audioClips;
    /*
    0 : mainMusic
    1 : Level Completed
    2 : 
    3 : 
    4 : 
    5 : 
    */

    void Awake()
    {
        gameMusicArr = gameObject.AddComponent<AudioSource>(); // 오디오소스 없기 때문에, 추가해서 지정해줘야 함
        gameMusicArr.loop = true;   // #51 보완
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

    public void LevelCompleted()    // #53 게임 성공 종료 BGM
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[1];
        gameMusicArr.Play();
    }


}
