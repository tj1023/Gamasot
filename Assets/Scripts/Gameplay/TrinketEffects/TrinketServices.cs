using Data;
using Gameplay.Systems;

namespace Gameplay.TrinketEffects
{
    /// <summary>
    /// TrinketEffect 구현체에 외부 서비스를 전달하는 컨텍스트 객체입니다.
    /// TrinketManager가 Awake에서 한 번 생성하여 재사용합니다.
    /// </summary>
    public class TrinketServices
    {
        public IngredientManager IngredientManager { get; }
        public FoodIngredientData[] AllIngredients { get; }

        public TrinketServices(IngredientManager ingredientManager, FoodIngredientData[] allIngredients)
        {
            IngredientManager = ingredientManager;
            AllIngredients = allIngredients;
        }
    }
}
