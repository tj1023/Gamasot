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

            return new MultiplyScoreCommand(source, source, multiplier);
        }
    }
}
