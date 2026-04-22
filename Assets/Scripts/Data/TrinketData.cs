using Core;
using Gameplay.TrinketEffects;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "NewTrinketData", menuName = "Data/Trinket Data")]
    public class TrinketData : ScriptableObject
    {
        [Header("기본 정보")]
        public string trinketName;
        public Sprite sprite;
        [TextArea(2, 5)] public string description;
        public int maxAccumulationCount = 1;
        
        [Header("효과")]
        [SerializeReference, SubclassSelector]
        [Tooltip("인스펙터에서 효과 구현체를 선택하세요")]
        public TrinketEffect effect;
    }
}
