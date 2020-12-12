using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 0)]
public class AudioData : ScriptableObject
{
    [SerializeField]
    private AudioClip[] gameplayTracks;
    public AudioClip[] GameplayTracks { get { return gameplayTracks; } }

    [SerializeField]
    private AudioClip missionSuccessStinger;
    public AudioClip MissionSuccessStinger { get { return missionSuccessStinger; } }

    [SerializeField]
    private AudioClip missionFailedStinger;
    public AudioClip MissionFailedStinger { get { return missionFailedStinger; } }

    [SerializeField]
    private Clip[] deathCollapse;
    public Clip[] DeathCollapse { get { return deathCollapse; } }

    [SerializeField]
    private Clip[] deathCrashes;
    public Clip[] DeathCrashes { get { return deathCrashes; } }

    [SerializeField]
    private Clip interactableUiHoverOver;
    public Clip InteractableUiHoverOver { get { return interactableUiHoverOver; } }

    [SerializeField]
    private Clip interactableUiClick;
    public Clip InteractableUiClick { get { return interactableUiClick; } }

    [SerializeField]
    private Clip dialogueAI;
    public Clip DialogueAI { get { return dialogueAI; } }

    [SerializeField]
    private Clip dialogueHuman;
    public Clip DialogueHuman { get { return dialogueHuman; } }

    public Clip GetRandomDeathCollapse()
    {
        return DeathCollapse[Random.Range(0, DeathCollapse.Length)];
    }

    public Clip GetRandomDeathCrash()
    {
        return DeathCrashes[Random.Range(0, DeathCrashes.Length)];
    }
}

[System.Serializable]
public class Clip
{
    [SerializeField]
    private AudioClip audioClip;
    public AudioClip AudioClip { get { return audioClip; } }

    [SerializeField]
    private float startTime;
    public float StartTime { get { return startTime; } }

    [SerializeField]
    private float pitch;
    public float Pitch { get { return pitch; } }

    [SerializeField]
    private float maxVolume;
    public float MaxVolume { get { return maxVolume; } }
}