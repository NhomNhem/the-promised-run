using UnityEngine;

namespace ThePromisedRun.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "The Promised Run/Audio/Audio Settings")]
    public class AudioSettingsSO : ScriptableObject
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume  = 1f;
        [Range(0f, 1f)] public float sfxVolume    = 1f;
    }
}
