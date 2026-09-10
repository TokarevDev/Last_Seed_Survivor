using System;

namespace Game.Infrastructure.Advertising
{
    public interface IRewardedAdService
    {
        bool IsReady { get; }

        void ShowRewardedAd(Action<bool> onCompleted);
    }

}
