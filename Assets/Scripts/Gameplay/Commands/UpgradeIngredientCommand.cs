using System.Collections;
using Core;
using Data;
using Interfaces;

namespace Gameplay.Commands
{
    public class UpgradeIngredientCommand : ICommand
    {
        private readonly RuntimeIngredient _target;
        private readonly float _delay;

        public UpgradeIngredientCommand(RuntimeIngredient target, float delay = 0.5f)
        {
            _target = target;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_target is { IsAdvanced: false })
            {
                _target.TransformToAdvanced();
                
                // 변환 연출을 위한 대기 시간 필요 시 설정
                if (_delay > 0f)
                    yield return WaitCache.Seconds(_delay);
            }
        }
    }
}
