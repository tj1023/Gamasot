using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{
    /// <summary>
    /// 재료의 속성 및 시너지 기준 분류용 열거형입니다.
    /// </summary>
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
    
    /// <summary>
    /// 각 재료의 기본 명세(BaseScore 등)와 고유 시너지 효과를 들고 있는 데이터입니다.
    /// 기획자가 유니티 인스펙터에서 하드코딩 없이 조합(Data-Driven)할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFoodIngredient", menuName = "Data/FoodIngredient Data")]
    public class FoodIngredientData : ScriptableObject
    {
        [Header("기본 정보")]
        public string ingredientName;           // 요소 이름
        public int baseScore;                   // 덱에 추가되었을 때 기본 점수
        public IngredientType type;             // 재료 속성 (채소, 육류 등)

        [Header("시너지 시스템")]
        [Tooltip("고유 시너지 규칙 (트리거+이펙트를 리스트로 조합)")]
        public List<SynergyData> synergies = new();
    }
}
