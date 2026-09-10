using System.Collections.Generic;
using Game.Core.Randomization;
using Game.Gameplay.Enemy.Worm.Balance;
using Game.Gameplay.Enemy.Worm.Combat;
using Game.Gameplay.Rewards.Data;
using UnityEngine;

namespace Game.Gameplay.Enemy.Worm
{
    public static class WormSectionBuilder
    {
        public static List<WormSection> BuildSections(
            List<WormSegment> segments,
            IRandomSource randomSource,
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles = null)
        {
            List<WormSection> sections = new();
            List<WormSegment> buffer = new();

            int sectionIndex = 0;
            int sectionsWithoutCocoon = 0;
            int totalSections = CountGameplaySections(segments);

            for (int i = 0; i < segments.Count; i++)
            {
                WormSegment seg = segments[i];

                if (seg.Type is WormSegmentType.Head or WormSegmentType.Tail)
                    continue;

                buffer.Add(seg);

                if (buffer.Count == WormCocoonRules.SectionSize)
                {
                    CreateSection(
                        buffer,
                        sections,
                        sectionIndex,
                        totalSections,
                        cocoonProfiles,
                        randomSource,
                        ref sectionsWithoutCocoon);

                    buffer.Clear();
                    sectionIndex++;
                }
            }

            if (buffer.Count > 0)
            {
                CreateSection(
                    buffer,
                    sections,
                    sectionIndex,
                    totalSections,
                    cocoonProfiles,
                    randomSource,
                    ref sectionsWithoutCocoon);
            }

            return sections;
        }

        private static void CreateSection(
            List<WormSegment> buffer,
            List<WormSection> sections,
            int sectionIndex,
            int totalSections,
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles,
            IRandomSource randomSource,
            ref int sectionsWithoutCocoon)
        {
            WormSection section = new();

            for (int i = 0; i < buffer.Count; i++)
            {
                section.AddSegment(buffer[i]);
            }

            TryPlaceCocoon(
                buffer,
                section,
                sectionIndex,
                totalSections,
                cocoonProfiles,
                randomSource,
                ref sectionsWithoutCocoon);

            sections.Add(section);
        }

        private static void TryPlaceCocoon(
            List<WormSegment> buffer,
            WormSection section,
            int sectionIndex,
            int totalSections,
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles,
            IRandomSource randomSource,
            ref int sectionsWithoutCocoon)
        {
            if (buffer.Count == 0)
                return;

            if (!WormCocoonRules.TryGetCocoonSegmentIndex(buffer.Count, out int cocoonSegmentIndex))
            {
                sectionsWithoutCocoon++;
                return;
            }

            WormSegment cocoonSegment = buffer[cocoonSegmentIndex];
            float sectionProgress = GetSectionProgress(sectionIndex, totalSections);
            bool spawnCocoon = ShouldPlaceCocoon(
                sectionIndex,
                totalSections,
                sectionProgress,
                sectionsWithoutCocoon);

            if (!spawnCocoon)
            {
                sectionsWithoutCocoon++;
                return;
            }

            sectionsWithoutCocoon = 0;

            CocoonRewardProfile profile = RollCocoonProfile(
                cocoonProfiles,
                sectionProgress,
                randomSource);
            cocoonSegment.EnableCocoon(profile);

            section.SetCocoon(profile);
        }

        private static bool ShouldPlaceCocoon(
            int sectionIndex,
            int totalSections,
            float sectionProgress,
            int sectionsWithoutCocoon)
        {
            return WormCocoonRules.ShouldPlaceCocoon(
                sectionIndex,
                totalSections,
                sectionProgress,
                sectionsWithoutCocoon);
        }

        private static CocoonRewardProfile RollCocoonProfile(
            IReadOnlyList<CocoonRewardProfile> cocoonProfiles,
            float sectionProgress,
            IRandomSource randomSource)
        {
            return WormCocoonRules.RollCocoonProfile(
                cocoonProfiles,
                sectionProgress,
                randomSource);
        }

        private static int CountGameplaySections(List<WormSegment> segments)
        {
            if (segments == null || segments.Count == 0)
                return 0;

            int gameplaySegmentCount = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                WormSegment segment = segments[i];

                if (segment != null && segment.Type is not (WormSegmentType.Head or WormSegmentType.Tail))
                    gameplaySegmentCount++;
            }

            return WormCocoonRules.CountGameplaySections(gameplaySegmentCount);
        }

        private static float GetSectionProgress(int sectionIndex, int totalSections)
        {
            return WormCocoonRules.GetSectionProgress(sectionIndex, totalSections);
        }
    }

}
