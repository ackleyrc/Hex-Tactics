using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
#region SINGLETON_MGMT
    private static AudioManager _Instance;
    public static AudioManager Instance { get { return _Instance; } }

    private void Awake()
    {
        _Instance = this;
    }

    private void OnDestroy()
    {
        if (_Instance == this)
        {
            _Instance = null;
        }
    }
    #endregion SINGLETON_MGMT

    public AudioData audioData;

    public AudioSource musicTitleScreen;
    public AudioSource musicTrackA;
    public AudioSource musicTrackB;

    private enum MusicState { NONE, TITLE_SCREEN, INTRO, GAMEPLAY, OUTRO, CREDITS }
    private MusicState currentMusicState = MusicState.NONE;

    private int currentGameplayClipIndex = -1;

    private float CROSSFADE_TIME = 2.0f;

    private void Start()
    {
        TitleScreenGUI.Instance.OnStartGame += HandleStartGameFromTitleScreen;
        PauseScreenGUI.Instance.OnRestartGame += HandleRestartGameFromPauseScreen;
        WinConditionGUI.Instance.OnRestartGame += HandleRestartGameFromWinLoseScreen;

        musicTitleScreen.loop = true;
        musicTitleScreen.Play();
        currentMusicState = MusicState.TITLE_SCREEN;
    }

    private void HandleStartGameFromTitleScreen()
    {
        /* TODO: We'll add this in once we have the Objective / Exposition UI implemented
        if (currentMusicState != MusicState.INTRO)
        {
            musicTitleScreen.Stop();
            currentMusicState = MusicState.INTRO;
        }
        */

        musicTitleScreen.Stop();

        if (currentMusicState != MusicState.GAMEPLAY)
        {
            currentGameplayClipIndex = 0;
            musicTrackA.clip = audioData.GameplayTracks[currentGameplayClipIndex];

            musicTrackA.Stop();
            musicTrackA.Play();

            currentMusicState = MusicState.GAMEPLAY;
        }
    }

    private void HandleRestartGameFromPauseScreen()
    {
        SkipToNextGameplayTrack();
    }

    private void HandleRestartGameFromWinLoseScreen()
    {
        SkipToNextGameplayTrack();
    }

    private void SkipToNextGameplayTrack()
    {
        if (musicTrackB.isPlaying == false &&
            musicTrackA.isPlaying == true)
        {
            currentGameplayClipIndex = (currentGameplayClipIndex + 1) % audioData.GameplayTracks.Length;
            musicTrackB.clip = audioData.GameplayTracks[currentGameplayClipIndex];

            musicTrackA.Stop();
            musicTrackB.Stop();
            musicTrackB.Play();

            Debug.Log($"AudioManager :: Switch from Track A to Track B with index [{currentGameplayClipIndex}]");
        }
        else if (musicTrackA.isPlaying == false &&
                 musicTrackB.isPlaying == true)
        {
            currentGameplayClipIndex = (currentGameplayClipIndex + 1) % audioData.GameplayTracks.Length;
            musicTrackA.clip = audioData.GameplayTracks[currentGameplayClipIndex];

            musicTrackB.Stop();
            musicTrackA.Stop();
            musicTrackA.Play();

            Debug.Log($"AudioManager :: Switch from Track B to Track A with index [{currentGameplayClipIndex}]");
        }
        else if (musicTrackA.isPlaying == false &&
                 musicTrackB.isPlaying == false)
        {
            currentGameplayClipIndex = (currentGameplayClipIndex + 1) % audioData.GameplayTracks.Length;
            musicTrackA.clip = audioData.GameplayTracks[currentGameplayClipIndex];

            musicTrackB.Stop();
            musicTrackA.Stop();
            musicTrackA.Play();

            Debug.Log($"AudioManager :: Start Track A with index [{currentGameplayClipIndex}]");
        }
        // else both are playing, in which case we let the crossfade play out

        currentMusicState = MusicState.GAMEPLAY;
    }

    private void Update()
    {
        if (currentMusicState == MusicState.GAMEPLAY)
        {
            if (musicTrackB.isPlaying == false &&
                musicTrackA.isPlaying == true &&
                musicTrackA.clip.length - musicTrackA.time < CROSSFADE_TIME)
            {
                currentGameplayClipIndex = (currentGameplayClipIndex + 1) % audioData.GameplayTracks.Length;
                musicTrackB.clip = audioData.GameplayTracks[currentGameplayClipIndex];

                musicTrackB.Stop();
                musicTrackB.Play();

                Debug.Log($"AudioManager :: Switch from Track A to Track B with index [{currentGameplayClipIndex}]");
            }
            else if (musicTrackA.isPlaying == false &&
                     musicTrackB.isPlaying == true &&
                     musicTrackB.clip.length - musicTrackB.time < CROSSFADE_TIME)
            {
                currentGameplayClipIndex = (currentGameplayClipIndex + 1) % audioData.GameplayTracks.Length;
                musicTrackA.clip = audioData.GameplayTracks[currentGameplayClipIndex];

                musicTrackA.Stop();
                musicTrackA.Play();

                Debug.Log($"AudioManager :: Switch from Track B to Track A with index [{currentGameplayClipIndex}]");
            }
        }
    }
}