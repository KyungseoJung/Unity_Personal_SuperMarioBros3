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
    2 : SelectAnItem (P버튼 밟았을 때)      // #72
    3 : You.have.died (플레이어 죽었을 때)  // #76
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
    
    public void MusicPauseStart() // #77 BGM 일시 정지 시작
    {
        gameMusicArr.Pause();
    }

    public void MusicPauseEnd() // #77 BGM 일시 정지 종료
    {
        gameMusicArr.Play();
    }

    public void PushPButtonMusicOn()   // #72
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[2];
        gameMusicArr.Play();

        Invoke("MainMusicOn", 8.0f);    // 8초 뒤에는 메인 뮤직으로 돌아가도록
    }

    public void LevelCompleted()    // #53 게임 성공 종료 BGM
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[1];
        gameMusicArr.loop = false;   // #53 보완 - 게임 종료 시, 나오는 BGM은 LOOP로 반복할 필요 없음.

        gameMusicArr.Play();
    }

    public void PlayerDie()         // #76
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[3];
        gameMusicArr.loop = false;   

        gameMusicArr.Play();
    }


}
