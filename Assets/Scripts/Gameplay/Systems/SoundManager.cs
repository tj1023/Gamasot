using UnityEngine;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Systems
{
    /// <summary>
    /// 기존 EventBus 이벤트를 구독하여 적절한 타이밍에 사운드를 재생하는 매니저입니다.
    /// BGM용 AudioSource 1개와 SFX용 AudioSource 1개를 사용합니다.
    /// SoundData ScriptableObject에서 클립과 볼륨을 참조합니다.
    /// </summary>
    public class SoundManager : MonoBehaviour,
        IEventListener<PhaseChangedEvent>,
        IEventListener<IngredientSelectedEvent>,
        IEventListener<TrinketSelectedEvent>,
        IEventListener<ItemsHarvestedEvent>,
        IEventListener<SynergyActivatedEvent>,
        IEventListener<TrailArrivedEvent>
    {
        [Header("Data")]
        [SerializeField] private SoundData soundData;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;

        private void Awake()
        {
            // AudioSource 동적 생성으로 Inspector 의존 제거
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(this);
            EventBus<IngredientSelectedEvent>.Subscribe(this);
            EventBus<TrinketSelectedEvent>.Subscribe(this);
            EventBus<ItemsHarvestedEvent>.Subscribe(this);
            EventBus<SynergyActivatedEvent>.Subscribe(this);
            EventBus<TrailArrivedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
            EventBus<IngredientSelectedEvent>.Unsubscribe(this);
            EventBus<TrinketSelectedEvent>.Unsubscribe(this);
            EventBus<ItemsHarvestedEvent>.Unsubscribe(this);
            EventBus<SynergyActivatedEvent>.Unsubscribe(this);
            EventBus<TrailArrivedEvent>.Unsubscribe(this);
        }

        // ─── BGM ────────────────────────────────────────────

        public void OnEvent(PhaseChangedEvent eventData)
        {
            // Ready(타이틀) 진입 시 BGM 시작 — 이후 모든 페이즈에서 계속 재생
            if (eventData.NewPhase == GamePhase.Ready)
            {
                StartBGM();
            }
        }

        private void StartBGM()
        {
            if (soundData == null || soundData.bgm == null) return;
            if (_bgmSource.isPlaying) return;

            _bgmSource.clip = soundData.bgm;
            _bgmSource.volume = soundData.bgmVolume;
            _bgmSource.Play();
        }

        // ─── SFX ────────────────────────────────────────────

        public void OnEvent(IngredientSelectedEvent eventData)
        {
            PlaySfx(soundData.select, soundData.clickVolume);
        }

        public void OnEvent(TrinketSelectedEvent eventData)
        {
            PlaySfx(soundData.select, soundData.clickVolume);
        }

        public void OnEvent(ItemsHarvestedEvent eventData)
        {
            PlaySfx(soundData.scoop, soundData.scoopVolume);
        }

        public void OnEvent(SynergyActivatedEvent eventData)
        {
            PlaySfx(soundData.synergy, soundData.synergyVolume);
        }

        public void OnEvent(TrailArrivedEvent eventData)
        {
            PlaySfx(soundData.trailArrive, soundData.trailArriveVolume);
        }

        private System.Collections.Generic.Dictionary<AudioClip, float> _lastPlayedTime = new();

        private void PlaySfx(AudioClip clip, float volume)
        {
            if (clip == null || _sfxSource == null) return;
            
            if (_lastPlayedTime.TryGetValue(clip, out float lastTime))
            {
                if (Time.unscaledTime - lastTime < 0.05f) return;
            }
            _lastPlayedTime[clip] = Time.unscaledTime;

            _sfxSource.PlayOneShot(clip, volume);
        }
    }
}
