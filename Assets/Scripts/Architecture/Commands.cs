using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay;

namespace Architecture
{
    public class IngredientScoreDeltaCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly RuntimeIngredient _target;
        private readonly int _delta;
        private readonly float _delay;

        public IngredientScoreDeltaCommand(RuntimeIngredient source, RuntimeIngredient target, int delta, float delay = 0.5f)
        {
            _source = source;
            _target = target;
            _delta = delta;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_target == null || _delta == 0) yield break;
            bool hasTrail = _source != null && _source != _target;
            float trailDuration = _delay * 0.8f;

            // 트레일 이펙트 요청 발생 (Source -> Target Ingredient)
            if (hasTrail)
            {
                EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                {
                    SourceIngredient = _source,
                    TargetType = EffectTargetType.Ingredient,
                    TargetIngredient = _target,
                    Duration = trailDuration
                });

                // 트레일이 도착할 때까지 대기
                if (trailDuration > 0f)
                    yield return new WaitForSeconds(trailDuration);
            }

            // 점수 업데이트 (이때 UI 팝 애니메이션 발동됨)
            _target.CurrentScore += _delta;
                
            // 개별 재료 점수업 이벤트가 있다면 여기서 연출 (현재는 생략)
                
            float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
            if (remainingDelay > 0f)
                yield return new WaitForSeconds(remainingDelay);
        }
    }

    public class AddHighestScoreToSelfCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly RuntimeIngredient _source;
        private readonly int _count;
        private readonly float _delay;

        public AddHighestScoreToSelfCommand(GameContext context, RuntimeIngredient source, int count, float delay = 0.5f)
        {
            _context = context;
            _source = source;
            _count = count;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_source == null || _context.HarvestedIngredients == null || _context.HarvestedIngredients.Count == 0) yield break;

            List<RuntimeIngredient> validIngredients = new List<RuntimeIngredient>();
            foreach (var item in _context.HarvestedIngredients)
            {
                if (item != null)
                {
                    validIngredients.Add(item);
                }
            }

            // 점수 내림차순 정렬
            validIngredients.Sort((a, b) => b.CurrentScore.CompareTo(a.CurrentScore));

            int loopCount = Mathf.Min(_count, validIngredients.Count);
            int totalBonus = 0;

            for (int i = 0; i < loopCount; i++)
            {
                totalBonus += validIngredients[i].CurrentScore;
            }

            if (totalBonus > 0)
            {
                float trailDuration = _delay * 0.8f;
                bool hasTrail = false;

                for (int i = 0; i < loopCount; i++)
                {
                    if (validIngredients[i] != _source)
                    {
                        EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                        {
                            SourceIngredient = validIngredients[i],
                            TargetType = EffectTargetType.Ingredient,
                            TargetIngredient = _source,
                            Duration = trailDuration
                        });
                        hasTrail = true;
                    }
                }

                if (hasTrail && trailDuration > 0f)
                    yield return new WaitForSeconds(trailDuration);

                _source.CurrentScore = totalBonus;

                float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
                if (remainingDelay > 0f)
                    yield return new WaitForSeconds(remainingDelay);
            }
        }
    }

    public class TransferRoundScoreToTotalCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly float _delay;

        public TransferRoundScoreToTotalCommand(GameContext context, float delay = 0.2f)
        {
            _context = context;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            Debug.Log($"[Command] TransferRoundScoreToTotal: Delay: {_delay}s");

            if (_context.RoundScore > 0)
            {
                _context.TotalScore += _context.RoundScore;
                _context.RoundScore = 0;

                EventBus<TotalScoreUpdatedEvent>.Publish(new TotalScoreUpdatedEvent { TotalScore = _context.TotalScore });
                EventBus<RoundScoreUpdatedEvent>.Publish(new RoundScoreUpdatedEvent { RoundScore = _context.RoundScore });

                if (_delay > 0f)
                    yield return new WaitForSeconds(_delay);
            }
        }
    }

    public class AccumulateToRoundScoreCommand : ICommand
    {
        private readonly GameContext _context;
        private readonly float _delay;

        public AccumulateToRoundScoreCommand(GameContext context, float delay = 0.2f)
        {
            _context = context;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            float trailDuration = _delay; 

            foreach (var item in _context.HarvestedIngredients)
            {
                if (item is { CurrentScore: > 0 })
                {
                    // Trail 발사 (목표는 TotalScore)
                    EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                    {
                        SourceIngredient = item,
                        TargetType = EffectTargetType.TotalScore, 
                        Duration = trailDuration
                    });

                    // 꼬리가 목표에 도달할 때까지 대기
                    yield return new WaitForSeconds(trailDuration);

                    // 점수는 RoundScore에 누계
                    _context.RoundScore += item.CurrentScore;
                    EventBus<RoundScoreUpdatedEvent>.Publish(new RoundScoreUpdatedEvent { RoundScore = _context.RoundScore });
                }
            }

            // 모든 누계가 끝난 후 잠깐 대기
            yield return new WaitForSeconds(0.2f);
        }
    }

    public class MultiplyIngredientScoreCommand : ICommand
    {
        private readonly RuntimeIngredient _source;
        private readonly RuntimeIngredient _target;
        private readonly float _multiplier;
        private readonly float _delay;

        public MultiplyIngredientScoreCommand(RuntimeIngredient source, RuntimeIngredient target, float multiplier, float delay = 0.5f)
        {
            _source = source;
            _target = target;
            _multiplier = multiplier;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            string sourceName = _source != null && _source.OriginalData != null ? _source.OriginalData.ingredientName : "None";
            string targetName = _target != null && _target.OriginalData != null ? _target.OriginalData.ingredientName : "None";
            Debug.Log($"[Command] MultiplyIngredientScore: Source({sourceName}) -> Target({targetName}) | Multiplier: x{_multiplier} | Delay: {_delay}s");

            if (_target != null)
            {
                int oldScore = _target.CurrentScore;
                int newScore = Mathf.RoundToInt(oldScore * _multiplier);
                int delta = newScore - oldScore;

                if (delta != 0)
                {
                    bool hasTrail = _source != null && _source != _target;
                    float trailDuration = _delay * 0.8f;

                    if (hasTrail)
                    {
                        EventBus<PlayScoreTrailEvent>.Publish(new PlayScoreTrailEvent
                        {
                            SourceIngredient = _source,
                            TargetType = EffectTargetType.Ingredient,
                            TargetIngredient = _target,
                            Duration = trailDuration
                        });

                        if (trailDuration > 0f)
                            yield return new WaitForSeconds(trailDuration);
                    }

                    _target.CurrentScore = newScore;
                    
                    float remainingDelay = hasTrail ? _delay - trailDuration : _delay;
                    if (remainingDelay > 0f)
                        yield return new WaitForSeconds(remainingDelay);
                }
            }
        }
    }

    public class AggregateCommand : ICommand
    {
        private readonly List<ICommand> _commands;

        public AggregateCommand(List<ICommand> commands)
        {
            _commands = commands;
        }

        public IEnumerator ExecuteAsync()
        {
            foreach (var cmd in _commands)
            {
                yield return cmd.ExecuteAsync();
            }
        }
    }

    public class TransformIngredientCommand : ICommand
    {
        private readonly RuntimeIngredient _targetIngredient;
        private readonly float _delay;

        public TransformIngredientCommand(RuntimeIngredient target, float delay = 0.5f)
        {
            _targetIngredient = target;
            _delay = delay;
        }

        public IEnumerator ExecuteAsync()
        {
            if (_targetIngredient is { IsAdvanced: false })
            {
                _targetIngredient.TransformToAdvanced();
                
                // 변환 연출을 위한 대기 시간 필요 시 설정
                if (_delay > 0f)
                    yield return new WaitForSeconds(_delay);
            }
        }
    }
}
