namespace Game.Gameplay.Enemy.Worm.Presentation
{
    public readonly struct WormPresentationConfig
    {
        public WormPresentationConfig(
            float segmentSpacing,
            float tailVisualSpacingMultiplier,
            float headBridgeSpacingMultiplier,
            float activeDistancePadding,
            float waveAmplitude,
            float waveFrequency,
            float waveSpeed)
        {
            SegmentSpacing = segmentSpacing;
            TailVisualSpacingMultiplier = tailVisualSpacingMultiplier;
            HeadBridgeSpacingMultiplier = headBridgeSpacingMultiplier;
            ActiveDistancePadding = activeDistancePadding;
            WaveAmplitude = waveAmplitude;
            WaveFrequency = waveFrequency;
            WaveSpeed = waveSpeed;
        }

        public float SegmentSpacing { get; }
        public float WaveSpeed { get; }

        private float TailVisualSpacingMultiplier { get; }
        private float HeadBridgeSpacingMultiplier { get; }
        private float ActiveDistancePadding { get; }
        private float WaveAmplitude { get; }
        private float WaveFrequency { get; }

        public WormSegmentChainLayout CreateSegmentLayout(
            float headDistance,
            float waveTime,
            float verticalOffset,
            bool isSectionRollback,
            bool isReviveRollback)
        {
            return new WormSegmentChainLayout(
                headDistance,
                SegmentSpacing,
                TailVisualSpacingMultiplier,
                HeadBridgeSpacingMultiplier,
                ActiveDistancePadding,
                WaveAmplitude,
                WaveFrequency,
                waveTime,
                verticalOffset,
                isSectionRollback,
                isReviveRollback);
        }
    }
}
