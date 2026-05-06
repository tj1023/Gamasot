using UnityEngine;

namespace Data
{
    /// <summary>
    /// 게임에서 사용되는 모든 사운드 클립과 볼륨 설정을 관리하는 ScriptableObject입니다.
    /// Inspector에서 AudioClip을 직접 할당하고 볼륨을 조절할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/SoundData", fileName = "SoundData")]
    public class SoundData : ScriptableObject
    {
        [Header("BGM")]
        public AudioClip bgm;
        [Range(0f, 1f)] public float bgmVolume = 0.5f;

        [Header("SFX")]
        public AudioClip select;
        [Range(0f, 1f)] public float clickVolume = 1f;

        public AudioClip scoop;
        [Range(0f, 1f)] public float scoopVolume = 1f;

        public AudioClip synergy;
        [Range(0f, 1f)] public float synergyVolume = 1f;

        public AudioClip trailArrive;
        [Range(0f, 1f)] public float trailArriveVolume = 1f;
    }
}
