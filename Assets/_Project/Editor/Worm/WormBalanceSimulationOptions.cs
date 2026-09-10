using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Worm
{
    public enum WormBalanceRewardPickStrategy
    {
        RandomChoice = 0,
        HighestRarityThenRandom = 1,
        HighestEstimatedDpsGain = 2
    }

    public enum WormBalanceAdSimulationMode
    {
        NoAdsOnly = 0,
        AdsAssistOnly = 1,
        CompareNoAdsAndAdsAssist = 2,
        BalanceMatrix = 3
    }

    internal enum WormBalanceScenario
    {
        NoAds = 0,
        ReviveOnly = 1,
        AdsAssistNoRevive = 2,
        AdsAssist = 3
    }

}
