
using Game.Gameplay.Rewards.Data;

namespace Game.Gameplay.Enemy.Worm.Presentation
{
    using UnityEngine;

    public abstract class WormCocoonVisual : MonoBehaviour
    {
        public abstract void Apply(CocoonRewardProfile profile);

        public abstract void ResetVisual();

        public abstract void SetEffectSorting(int sortingLayerId, int sortingOrder);
    }

}
