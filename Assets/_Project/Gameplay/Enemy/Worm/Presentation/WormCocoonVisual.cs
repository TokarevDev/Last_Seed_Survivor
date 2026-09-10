using Game.Gameplay.Rewards.Data;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public abstract class WormCocoonVisual : MonoBehaviour
    {
        public abstract void Apply(CocoonRewardProfile profile);

        public abstract void ResetVisual();

        public abstract void SetEffectSorting(int sortingLayerId, int sortingOrder);
    }

}
