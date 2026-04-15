using System;
using UnityEngine;
using Data;
using Gameplay.Commands;
using Interfaces;

namespace Gameplay.Synergies
{
    [Serializable]
    public class MultiplyScoreEffect : IEffect
    {
        public int priority = 200;
        public int Priority => priority;
        public float multiplier = 2f;

        public ICommand GenerateCommand(GameContext context, RuntimeIngredient source)
        {
            if (source == null) return null;

            int scoreDelta = Mathf.RoundToInt(source.CurrentScore * multiplier) - source.CurrentScore;
            if (scoreDelta == 0) return null;

            return new AddScoreCommand(source, source, scoreDelta);
        }
    }
}
