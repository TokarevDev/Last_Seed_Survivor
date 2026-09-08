namespace Game.Gameplay.Enemy.Worm.Presentation
{
    using UnityEngine;

    public abstract class WormFaceBurstView : MonoBehaviour, IWormFaceBurstView
    {
        public abstract void SetBoostActive(bool isActive);
    }

}
