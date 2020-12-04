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
    public AudioSource musicCreditsScreen;
    public AudioSource musicTrackA;
    public AudioSource musicTrackB;
    public AudioSource stinger;
    public AudioSource hexToHexSfx;
    public AudioSource actionConfirmSfx;
    public AudioSource unitTurnSfx;
    public AudioSource uiInteractableSfx;
    public AudioSource dialogueSfx;

    private enum MusicState { NONE, TITLE_SCREEN, INTRO, GAMEPLAY, OUTRO, CREDITS }
    private MusicState currentMusicState = MusicState.NONE;

    private int currentGameplayClipIndex = -1;

    private float CROSSFADE_TIME = 2.0f;

    private void Start()
    {
        TitleScreenGUI.Instance.OnStartGame += HandleStartGameFromTitleScreen;
        TitleScreenGUI.Instance.OnOpenCredits += HandleOpenCreditsFromTitleScreen;
        CreditsScreenGUI.Instance.OnCloseCredits += HandleCloseCreditsScreen;
        PauseScreenGUI.Instance.OnRestartGame += HandleRestartGameFromPauseScreen;
        WinConditionGUI.Instance.OnRestartGame += HandleRestartGameFromWinLoseScreen;
        GameManager.Instance.OnWinLoseConditionMet += HandleWinLoseConditionMet;

        musicTitleScreen.loop = true;
        musicTitleScreen.Play();
        currentMusicState = MusicState.TITLE_SCREEN;
    }

    public void PlayHexHoverOver()
    {
        hexToHexSfx.Stop();
        hexToHexSfx.Play();
    }

    public void PlayActionConfirm()
    {
        actionConfirmSfx.Stop();
        actionConfirmSfx.Play();
    }

    public void PlayUnitTurnChange()
    {
        unitTurnSfx.Stop();
        unitTurnSfx.Play();
    }

    public void PlayHoverOver()
    {
        uiInteractableSfx.Stop();
        uiInteractableSfx.clip = audioData.InteractableUiHoverOver.AudioClip;
        uiInteractableSfx.time = audioData.InteractableUiHoverOver.StartTime;
        uiInteractableSfx.volume = audioData.InteractableUiHoverOver.MaxVolume;
        uiInteractableSfx.pitch = audioData.InteractableUiHoverOver.Pitch;
        uiInteractableSfx.Play();
    }

    public void PlayClick()
    {
        uiInteractableSfx.Stop();
        uiInteractableSfx.clip = audioData.InteractableUiClick.AudioClip;
        uiInteractableSfx.time = audioData.InteractableUiClick.StartTime;
        uiInteractableSfx.volume = audioData.InteractableUiClick.MaxVolume;
        uiInteractableSfx.pitch = audioData.InteractableUiClick.Pitch;
        uiInteractableSfx.Play();
    }

    public void PlayAIDialogue()
    {
        if (dialogueSfx.isPlaying == false)
        {
            dialogueSfx.Stop();
            dialogueSfx.time = Random.Range(0.0f, Mathf.Clamp(dialogueSfx.clip.length - 8.0f, 0.0f, dialogueSfx.clip.length));
            dialogueSfx.Play();
        }
    }

    public void StopAIDialogue()
    {
        if (dialogueSfx.isPlaying == true)
        {
            dialogueSfx.Stop();
        }
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

    private void HandleOpenCreditsFromTitleScreen()
    {
        musicTitleScreen.Stop();

        if (currentMusicState != MusicState.CREDITS)
        {
            musicCreditsScreen.Stop();
            musicCreditsScreen.Play();

            currentMusicState = MusicState.CREDITS;
        }
    }

    private void HandleCloseCreditsScreen()
    {
        musicCreditsScreen.Stop();

        if (currentMusicState != MusicState.TITLE_SCREEN)
        {
            musicTitleScreen.Stop();
            musicTitleScreen.Play();

            currentMusicState = MusicState.TITLE_SCREEN;
        }
    }

    private void HandleRestartGameFromPauseScreen()
    {
        SkipToNextGameplayTrack();
    }

    private void HandleRestartGameFromWinLoseScreen()
    {
        stinger.Stop();

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

    private void HandleWinLoseConditionMet(bool humanPlayerWon)
    {
        Debug.Log($"AudioManager::HandleWinLoseConditionmet( humanPlayerWon: {humanPlayerWon} )");

        musicTrackB.Stop();
        musicTrackA.Stop();

        stinger.Stop();
        stinger.clip = humanPlayerWon ? audioData.MissionSuccessStinger : audioData.MissionFailedStinger;
        stinger.time = humanPlayerWon ? 0.0f : 1.2f; // after drum stick count off, includes cymbal
        //stinger.time = humanPlayerWon ? 0.0f : 1.5f; // After cymbal, start of first note
        stinger.Play();

        currentMusicState = MusicState.OUTRO;
    }
}