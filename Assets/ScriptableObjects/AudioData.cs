using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "ScriptableObjects/AudioData", order = 0)]
public class AudioData : ScriptableObject
{
    [SerializeField]
    private AudioClip[] gameplayTracks;
    public AudioClip[] GameplayTracks { get { return gameplayTracks; } }
}