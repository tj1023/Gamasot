using System.Collections.Generic;
using Architecture;

namespace Gameplay
{
    /// <summary>
    /// ScriptableObject(FoodIngredientData)의 원본 데이터를 참조하며,
    /// 런타임에 동적으로 변하는 상태(예: 시너지로 인한 점수 변화)를 저장하는 클래스
    /// </summary>
    public class RuntimeIngredient
    {
        public FoodIngredientData OriginalData { get; private set; }
        
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

        public RuntimeIngredient(FoodIngredientData data)
        {
            OriginalData = data;
            IsAdvanced = data.isAdvanced;
            CurrentScore = data.ActiveBaseScore;
        }
        
        public bool IsAdvanced { get; private set; }

        public List<SynergyData> ActiveSynergies => IsAdvanced ? OriginalData.advancedSynergies : OriginalData.synergies;
        
        public void TransformToAdvanced()
        {
            if (IsAdvanced) return;
            
            IsAdvanced = true;
            CurrentScore = OriginalData.advancedBaseScore;
        }
    }
}
