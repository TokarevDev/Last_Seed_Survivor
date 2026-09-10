using System.Collections.Generic;

namespace Game.Gameplay.Enemy.Worm.Combat
{
    public interface IWormSectionHealthPresentation
    {
        void BindSections(IReadOnlyList<WormSection> sections);

        void Clear();
    }

}
