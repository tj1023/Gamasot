using UnityEngine;

namespace Data
{
    public enum TrinketEffectType
    {
        None = 0,
        // 핫도그부: 전설카드 1장 발견
        DiscoverLegendaryCard,
        // 치즈샌드위치부: 희귀 재료 카드 2장 발견
        DiscoverTwoRareCards,
        // 맛있는부거: 보유중인 희귀이상의 카드 1장 발견
        DiscoverOwnedRareOrHigherCard,
        // 군만두부: 보유중인 랜덤 전설 카드 1장 +1
        DuplicateOwnedLegendaryCard,
        // 찐빵부: 보유중인 모든 희귀 카드의 수량 +1
        DuplicateAllOwnedRareCards,
        
        // 컵라면부: 보유할 수 있는 카드 수량 +1 (MaxSelectedIngredients + 1)
        IncreaseMaxSelectedIngredients,
        // 푸딩부: 냄비 안의 모든 재료가 서로 가까워짐 (중앙으로 뭉침)
        PuddingEffect,
        // 아이스크림부: 건지기 횟수 +1 (MaxScoopCount + 1)
        IncreaseMaxScoopCount,
        // 필링부: 건지는 범위가 더 커짐
        IncreaseScoopRadius,
        
        // 우유부: 건질때, 같은 재료 2개를 동시에 건지면, pot 안에 해당 재료 1개를 추가
        MilkEffect,
        // 소다부: 건질때, 4개 이상을 건지면, 보유재료 중 랜덤으로 1개를 pot 안에 추가
        SodaEffect,
        
        // 츠바부: 라운드 시작 시, 정산 패널에 '홍탕 츠바'(재료) 1개를 추가
        TsubaEffect,
        // 팝콘부: 라운드 시작 시, 일반 재료 2개를 고급 재료로 변환
        PopcornEffect
    }

    [CreateAssetMenu(fileName = "NewTrinketData", menuName = "Data/Trinket Data")]
    public class TrinketData : ScriptableObject
    {
        [Header("기본 정보")]
        public string trinketName;
        public Sprite sprite;
        [TextArea(2, 5)] public string description;
        public int maxAccumulationCount = 1;
        
        [Header("효과")]
        public TrinketEffectType effectType;
    }
}
