namespace LastSeed.Core.Input
{
    public interface IPlayerInputSnapshotProvider
    {
        PlayerInputSnapshot CurrentSnapshot { get; }

        void CaptureFrame();

        void ResetState();
    }
}
