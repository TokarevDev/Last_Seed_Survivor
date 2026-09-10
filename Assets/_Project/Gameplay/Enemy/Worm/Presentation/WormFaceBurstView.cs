using UnityEngine;

namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public abstract class WormFaceBurstView : MonoBehaviour, IWormFaceBurstView
    {
        public abstract void SetBoostActive(bool isActive);
    }

}
