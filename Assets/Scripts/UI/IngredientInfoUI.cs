using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;

namespace UI
{
    /// <summary>
    /// 재료의 정보를 나타내는 UI입니다.
    /// FoodIngredientData를 받아 각 UI 요소에 데이터를 표시하고,
    /// 희귀도에 따라 패널의 배경색을 변경합니다.
    /// </summary>
    public class IngredientInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image panelBackground;
        [SerializeField] private Image ingredientIcon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private TextMeshProUGUI descText;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor;
        [SerializeField] private Color rareColor;
        [SerializeField] private Color legendaryColor;

        private FoodIngredientData _currentData;

        /// <summary>
        /// 외부에서 등록하는 선택 콜백. 이 UI가 클릭되면 호출됩니다.
        /// </summary>
        public Action<FoodIngredientData> OnSelected { get; set; }

        /// <summary>
        /// FoodIngredientData를 통해 UI를 갱신합니다.
        /// </summary>
        public void Setup(FoodIngredientData data)
        {
            if (data == null) return;

            _currentData = data;

            // 이미지 세팅
            if (ingredientIcon != null)
            {
                ingredientIcon.sprite = data.sprite;
                ingredientIcon.enabled = data.sprite != null;
            }

            // 텍스트 정보 세팅
            if (nameText != null) nameText.text = data.ingredientName;
            if (scoreText != null) scoreText.text = $"{data.baseScore}";
            if (typeText != null) typeText.text = $"{data.type}";
            if (descText != null) descText.text = data.desc;

            // 희귀도에 따른 패널 배경색 변경
            if (panelBackground != null)
            {
                panelBackground.color = GetRarityColor(data.rarity);
            }
        }

        private Color GetRarityColor(Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => commonColor,
                Rarity.Rare => rareColor,
                Rarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        /// <summary>
        /// Button.onClick 등에서 호출합니다. 현재 표시 중인 재료 데이터를 콜백으로 전달합니다.
        /// </summary>
        public void SelectThis()
        {
            if (_currentData != null)
            {
                OnSelected?.Invoke(_currentData);
            }
        }
    }
}
