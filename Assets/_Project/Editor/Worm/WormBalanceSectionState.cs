using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Gameplay.Rewards.Data;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools.Worm
{
    internal sealed class WormBalanceSectionState
    {
        public readonly int Index;
        public readonly int SegmentCount;
        public readonly CocoonRewardProfile CocoonProfile;

        public int Hp;
        public bool HasCocoon => CocoonProfile != null;

        public WormBalanceSectionState(
            int index,
            int segmentCount,
            CocoonRewardProfile cocoonProfile)
        {
            Index = index;
            SegmentCount = Mathf.Max(1, segmentCount);
            CocoonProfile = cocoonProfile;
        }
    }

}
