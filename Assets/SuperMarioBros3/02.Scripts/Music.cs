using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour  // #51 //#51 refactor 사운드 크기 디폴트 1로 맞추기 
{

    public AudioSource gameMusicArr;
    public AudioSource soundEffectArr;
    public AudioClip[] audioClips;
    /*
    0 : mainMusic
    1 : Level Completed
    2 : SelectAnItem (P버튼 밟았을 때)      // #72
    3 : You.have.died (플레이어 죽었을 때)  // #76
    4 : Level Timer Points SFX 
    5 : Hurry                             // #81 시간 얼마 안 남았을 때 - 메인 뮤직 빠르게 나오기 전
    */

    void Awake()
    {
        gameMusicArr = gameObject.AddComponent<AudioSource>(); // 오디오소스 없기 때문에, 추가해서 지정해줘야 함
        soundEffectArr = gameObject.AddComponent<AudioSource>(); // 오디오소스 없기 때문에, 추가해서 지정해줘야 함
        gameMusicArr.loop = true;   // #51 보완
    }
    void Update()
    {
        if(!soundEffectArr.isPlaying && !gameMusicArr.isPlaying)   // #81 효과음 끝나면, 배경음악 재생
        {
            gameMusicArr.Play();
        }
    }
    public void MusicOff()  // #79
    {
        gameMusicArr.Stop();
    }

    public void GameStart() // #53 LobbyManager.cs에서 btnGameStart 버튼 누르면 실행되도록 
                            // #53 fix: 또는 게임 재시작(플레이어 죽어서 재시작)할 때에도 실행되도록 
    {
        MainMusicOn();
    }

    public void MainMusicOn(float _volume = 1f)
    {   
        Debug.Log("//#72 fix: 문제 검토 - MainMusciOn");
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[0];
        gameMusicArr.volume = _volume;
        gameMusicArr.Play();
        gameMusicArr.loop = true;  // #51 메인 뮤직 BGM 반복되도록 설정

    }

    public void NotMuchTimeLeft(float _volume = 1f) // #81 
    {
        gameMusicArr.Stop();
        soundEffectArr.clip = audioClips[5];
        soundEffectArr.volume = _volume;
        soundEffectArr.Play();
        soundEffectArr.loop = false;  

    }

    private void PlayMainMusic()    // #72 fix: Parameter(매개변수) 때문에 Invoke로 실행되지 않는 문제 해결
    {
        MainMusicOn();
    }
    
    public void MusicPauseStart() // #77 BGM 일시 정지 시작
    {
        gameMusicArr.Pause();
        // PausingSFX();
        // AudioSource.PlayClipAtPoint(audioClips[5], transform.position);    // #77 fix 일시정지 효과음 - StopGame 이후에 실행하면, 시간이 아예 멈춰버린 후이기 때문에, 효과음이 아예 적용되지 않을 때가 있음

    }

    public void MusicPauseEnd() // #77 BGM 일시 정지 종료
    {
        // MainMusicOn();   // #77 fix
        gameMusicArr.Play();
    }

    public void PushPButtonMusicOn(float _volume = 1f)   // #72
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[2];
        gameMusicArr.volume = _volume;
        gameMusicArr.Play();

        Invoke("PlayMainMusic", 8.0f);    // 8초 뒤에는 메인 뮤직으로 돌아가도록
    }

    public void LevelCompleted(float _volume = 1f)    // #53 게임 성공 종료 BGM
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[1];
        gameMusicArr.volume = _volume;
        gameMusicArr.loop = false;   // #53 보완 - 게임 종료 시, 나오는 BGM은 LOOP로 반복할 필요 없음.

        gameMusicArr.Play();
    }

    public void LevelTimerPoints(float _volume = 1f)  // #79 남은 시간 -> 점수로 전환되는 효과음 SFX
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[4];
        gameMusicArr.volume = _volume;
        gameMusicArr.loop = true;  

        gameMusicArr.Play();
   
    }

    // #77 fix: gameMusic.Arr 자체에 접근하기 때문에 - 일시정지가 풀린 후, Main Music을 실행하려면
    // 아예 Main Music이 재시작되는 문제가 있음
   
    // public void PausingSFX(float _volume = 1f)      
    // // #77 fix: LobbyManager.cs에서 실행하면 (0,0)를 벗어난 지점에서는 소리 잘 안 들리는 문제 발생 -> Music.cs에서 실행하도록 변경
    // {
    //     gameMusicArr.Stop();
    //     gameMusicArr.clip = audioClips[5];
    //     gameMusicArr.volume = _volume;
    //     gameMusicArr.loop = false;  

    //     gameMusicArr.Play();
    // }
    public void PlayerDie(float _volume = 1f)         // #76
    {
        gameMusicArr.Stop();
        gameMusicArr.clip = audioClips[3];
        gameMusicArr.volume = _volume;
        gameMusicArr.loop = false;   

        gameMusicArr.Play();
    }


}
