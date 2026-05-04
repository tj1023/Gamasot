using System.Collections.Generic;
using Core;

namespace Data
{
    /// <summary>
    /// ScriptableObject(FoodIngredientData)의 원본 데이터를 참조하며,
    /// 런타임에 동적으로 변하는 상태(예: 시너지로 인한 점수 변화)를 저장하는 클래스
    /// </summary>
    public class RuntimeIngredient
    {
        public FoodIngredientData OriginalData { get; }
        public bool IsAdvanced { get; private set; }

        /// <summary>
        /// 시너지 효과 등으로 런타임에 변동되는 실제 점수
        /// </summary>
        private int _currentScore;
        public int CurrentScore
        {
            get => _currentScore;
            set
            {
                if (_currentScore != value)
                {
                    int oldScore = _currentScore;
                    _currentScore = value;
                    EventBus<IngredientScoreChangedEvent>.Publish(new IngredientScoreChangedEvent
                    {
                        Ingredient = this,
                        OldScore = oldScore,
                        NewScore = value
                    });
                }
            }
        }

        public RuntimeIngredient(FoodIngredientData data, bool forceAdvanced = false)
        {
            OriginalData = data;
            IsAdvanced = forceAdvanced || data.isAdvanced;
            _currentScore = IsAdvanced ? data.advancedBaseScore : data.baseScore;
        }
        
        public List<SynergyData> ActiveSynergies => IsAdvanced ? OriginalData.advancedSynergies : OriginalData.synergies;
        
        public void TransformToAdvanced()
        {
            if (IsAdvanced) return;
            
            IsAdvanced = true;
            int oldScore = _currentScore;
            _currentScore = OriginalData.advancedBaseScore;

            // 점수 변동이 없더라도, 고급 변환 시각적 갱신을 위해 이벤트를 강제 발행
            EventBus<IngredientScoreChangedEvent>.Publish(new IngredientScoreChangedEvent
            {
                Ingredient = this,
                OldScore = oldScore,
                NewScore = _currentScore
            });
        }
    }
}
