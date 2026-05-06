using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Core;
using Data;

namespace UI
{
    public class SettingUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button titleButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(CloseSetting);
            
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                volumeSlider.value = AudioListener.volume;
            }
            
            if (titleButton != null) titleButton.onClick.AddListener(GoToTitle);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

            CloseSetting();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (settingPanel != null && settingPanel.activeSelf)
                    CloseSetting();
                else
                    OpenSetting();
            }
        }

        private void OpenSetting()
        {
            settingPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void CloseSetting()
        {
            settingPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private static void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }

        private void GoToTitle()
        {
            CloseSetting();
            EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent { TargetPhase = GamePhase.Ready });
        }

        private static void ExitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
