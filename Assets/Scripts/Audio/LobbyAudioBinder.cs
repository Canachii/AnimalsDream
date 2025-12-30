using UnityEngine;
using System.Collections;


public class LobbyAudioBinder : MonoBehaviour
{
    private IEnumerator Start()
    {
        // AudioManager 싱글톤 초기화 / DontDestroyOnLoad 세팅이 끝나도록 1프레임 대기
        yield return null;

        AudioManager.Instance?.PlayBgm(SoundId.Lobby_BGM, fade: true, restart: false);
    }
}
