using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;

namespace UI
{
    public class OwnedTrinketCellUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;

        public void Setup(TrinketData data, int count)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.sprite;
                iconImage.enabled = data.sprite != null;
            }

            if (countText != null)
            {
                countText.text = $"x{count}";
            }
        }
    }
}
