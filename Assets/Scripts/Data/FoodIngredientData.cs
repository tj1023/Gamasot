using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public enum IngredientType
    {
        None = 0,
        Meat = 1,
        Vegetable = 2,
        Mushroom = 3,
        Ball = 4,
        Carb = 5,
        Seafood = 6,
        // Spice = 7,
        // Sauce = 8
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
