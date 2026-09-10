using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Gameplay.Rewards.Runtime;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Worm
{
    internal readonly struct WormBalanceRewardChoiceEvaluation
    {
        public readonly RewardChoiceData Reward;
        public readonly float DpsGain;

        public WormBalanceRewardChoiceEvaluation(
            RewardChoiceData reward,
            float dpsGain)
        {
            Reward = reward;
            DpsGain = dpsGain;
        }
    }

}
