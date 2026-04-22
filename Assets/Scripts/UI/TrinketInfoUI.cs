using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;

namespace UI
{
    public class TrinketInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private GameObject selectionHighlight;

        private TrinketData _currentData;
        public Action<TrinketData> OnSelected;

        public void Setup(TrinketData data)
        {
            _currentData = data;

            if (iconImage != null)
            {
                iconImage.sprite = data.sprite;
                iconImage.enabled = data.sprite != null;
            }

            if (nameText != null)
            {
                nameText.text = data.trinketName;
            }

            if (descText != null)
            {
                descText.text = data.description;
            }

            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(false);
            }
        }

        public void SelectThis()
        {
            if (_currentData == null) return;
            
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(true);
            }

            // 중복 호출 방지: 콜백 실행 후 즉시 해제
            var callback = OnSelected;
            OnSelected = null;
            callback?.Invoke(_currentData);
        }
    }
}
