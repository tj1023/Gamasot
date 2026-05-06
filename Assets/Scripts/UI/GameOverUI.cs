using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Core;
using Data;
using Interfaces;
using Gameplay.Systems;

namespace UI
{
    /// <summary>
    /// 게임 오버 화면 UI.
    /// GameOver 페이즈에서 표시되며, 최종 점수를 보여주고 재시작 버튼을 제공합니다.
    /// </summary>
    public class GameOverUI : MonoBehaviour, IEventListener<PhaseChangedEvent>
    {
        [Header("UI References")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI roundInfoText;
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
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
            if (eventData.NewPhase == GamePhase.GameOver)
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

            var context = GameManager.Instance.Context;
            int currentScore = context.TotalScore;
            int highScore = PlayerPrefs.GetInt("HighScore", 0);

            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"{currentScore}";
            }

            if (roundInfoText != null)
            {
                roundInfoText.text = $"라운드 {context.CurrentRound - 1} 완료";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"최고 점수: {highScore}";
            }
        }

        private void Hide()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
        }

        private void OnRestartButtonClicked()
        {
            // 씬 재로드를 통해 완전 초기화
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
