using System;
using UnityEngine;

namespace Game.Infrastructure.Advertising
{
    public abstract class RewardedAdService : MonoBehaviour, IRewardedAdService
    {
        public abstract bool IsReady { get; }

        public abstract void ShowRewardedAd(Action<bool> onCompleted);
    }

}
