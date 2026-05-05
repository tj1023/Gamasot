using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public enum IngredientType
    {
        None = 0,
        육류 = 1,
        채소 = 2,
        버섯 = 3,
        완자 = 4,
        주식 = 5,
        해산물 = 6,
        // 향신료 = 7,
        // 소스 = 8
    }

    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Legendary = 2
    }

    [CreateAssetMenu(fileName = "NewFoodIngredient", menuName = "Data/FoodIngredient Data")]
    public class FoodIngredientData : ScriptableObject
    {
        [Header("기본 정보")]
        public string ingredientName;
        public int baseScore;
        public IngredientType type;
        public Rarity rarity;
        public Sprite sprite;
        public int count = 4;
        [TextArea(2, 5)] public string desc;
        public List<SynergyData> synergies = new();

        [Header("고급 속성")]
        public bool isAdvanced;
        public int advancedBaseScore;
        [TextArea(2, 5)] public string advancedDesc;
        public List<SynergyData> advancedSynergies = new();
    }
}
