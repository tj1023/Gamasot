using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core;
using Data;
using Interfaces;

namespace UI
{
    /// <summary>
    /// 게임 시작 화면 UI.
    /// Ready 페이즈에서 표시되며, 시작 버튼 클릭 시 OnSelection 페이즈로 전환합니다.
    /// </summary>
    public class GameStartUI : MonoBehaviour, IEventListener<PhaseChangedEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(this);
        }

        public void OnEvent(PhaseChangedEvent eventData)
        {
            if (eventData.NewPhase == GamePhase.Ready)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void Show()
        {
            if (rootPanel != null) rootPanel.SetActive(true);

            if (highScoreText != null)
            {
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                highScoreText.text = $"최고 점수: {highScore}";
            }
        }

        private void Hide()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        private void OnStartButtonClicked()
        {
            EventBus<RequestPhaseChangeEvent>.Publish(new RequestPhaseChangeEvent
            {
                TargetPhase = GamePhase.OnSelection
            });
        }
    }
}
