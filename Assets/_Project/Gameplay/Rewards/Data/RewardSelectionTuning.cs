using System;
using UnityEngine;

namespace Game.Gameplay.Rewards.Data
{
    [Serializable]
    public struct RewardSelectionTuning
    {
        private const float MinimumMultiplier = 0.01f;

        public static readonly RewardSelectionTuning Default = new(
            postRevivePrimaryDpsWeightMultiplier: 4f,
            postReviveSecondaryDpsWeightMultiplier: 1.25f,
            paidAssistPrimaryDpsWeightMultiplier: 2.75f,
            paidAssistSecondaryDpsWeightMultiplier: 1.15f);

        [SerializeField, Min(MinimumMultiplier)]
        private float _postRevivePrimaryDpsWeightMultiplier;

        [SerializeField, Min(MinimumMultiplier)]
        private float _postReviveSecondaryDpsWeightMultiplier;

        [SerializeField, Min(MinimumMultiplier)]
        private float _paidAssistPrimaryDpsWeightMultiplier;

        [SerializeField, Min(MinimumMultiplier)]
        private float _paidAssistSecondaryDpsWeightMultiplier;

        public RewardSelectionTuning(
            float postRevivePrimaryDpsWeightMultiplier,
            float postReviveSecondaryDpsWeightMultiplier,
            float paidAssistPrimaryDpsWeightMultiplier,
            float paidAssistSecondaryDpsWeightMultiplier)
        {
            _postRevivePrimaryDpsWeightMultiplier =
                Mathf.Max(MinimumMultiplier, postRevivePrimaryDpsWeightMultiplier);
            _postReviveSecondaryDpsWeightMultiplier =
                Mathf.Max(MinimumMultiplier, postReviveSecondaryDpsWeightMultiplier);
            _paidAssistPrimaryDpsWeightMultiplier =
                Mathf.Max(MinimumMultiplier, paidAssistPrimaryDpsWeightMultiplier);
            _paidAssistSecondaryDpsWeightMultiplier =
                Mathf.Max(MinimumMultiplier, paidAssistSecondaryDpsWeightMultiplier);
        }

        public float PostRevivePrimaryDpsWeightMultiplier =>
            Mathf.Max(MinimumMultiplier, _postRevivePrimaryDpsWeightMultiplier);

        public float PostReviveSecondaryDpsWeightMultiplier =>
            Mathf.Max(MinimumMultiplier, _postReviveSecondaryDpsWeightMultiplier);

        public float PaidAssistPrimaryDpsWeightMultiplier =>
            Mathf.Max(MinimumMultiplier, _paidAssistPrimaryDpsWeightMultiplier);

        public float PaidAssistSecondaryDpsWeightMultiplier =>
            Mathf.Max(MinimumMultiplier, _paidAssistSecondaryDpsWeightMultiplier);
    }
}
