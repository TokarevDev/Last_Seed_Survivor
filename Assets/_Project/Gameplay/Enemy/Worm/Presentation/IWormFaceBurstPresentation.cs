namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public interface IWormFaceBurstPresentation
    {
        void Bind(IWormFaceBurstView faceView);

        void Unbind();
    }

    public interface IWormFaceBurstView
    {
        void SetBoostActive(bool isActive);
    }

}
