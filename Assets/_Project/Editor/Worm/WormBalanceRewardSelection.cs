using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Gameplay.Rewards.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Worm
{
    internal sealed class WormBalanceRewardSelection
    {
        private readonly List<WormBalanceRewardChoiceEvaluation> _evaluations;

        public WormBalanceRewardSelection(
            List<RewardChoiceData> rewards,
            List<WormBalanceRewardChoiceEvaluation> evaluations)
        {
            Rewards = rewards ?? new List<RewardChoiceData>();
            _evaluations = evaluations ?? new List<WormBalanceRewardChoiceEvaluation>();
        }

        public readonly List<RewardChoiceData> Rewards;

        public float GetDpsGain(RewardChoiceData reward)
        {
            if (reward == null)
                return 0f;

            for (int i = 0; i < _evaluations.Count; i++)
            {
                if (ReferenceEquals(_evaluations[i].Reward, reward))
                    return _evaluations[i].DpsGain;
            }

            return 0f;
        }
    }

}
