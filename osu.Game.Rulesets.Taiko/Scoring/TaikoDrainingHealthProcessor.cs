// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Scoring;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    public partial class TaikoDrainingHealthProcessor : LegacyDrainingHealthProcessor
    {
        public TaikoDrainingHealthProcessor(double drainStartTime)
            : base(drainStartTime)
        {
        }

        protected override IEnumerable<HitObject> EnumerateTopLevelHitObjects() => EnumerateHitObjects(Beatmap).Where(h => h is Hit || h is DrumRoll || h is Swell);

        protected override IEnumerable<HitObject> EnumerateNestedHitObjects(HitObject hitObject) => Enumerable.Empty<HitObject>();

        protected override bool CheckDefaultFailCondition(JudgementResult result)
        {
            if (result.Type is HitResult.SmallTickMiss or HitResult.LargeTickMiss)
                return false;

            if (result.HitObject is Swell)
                return false;

            return base.CheckDefaultFailCondition(result);
        }

        protected override double GetHealthIncreaseFor(HitObject hitObject, HitResult result)
        {
            double increase = 0;

            switch (result)
            {
                case HitResult.Miss:
                    return IBeatmapDifficultyInfo.DifficultyRange(Beatmap.Difficulty.DrainRate, -0.03, -0.125, -0.2) * 0.35;
                case HitResult.Great:
                    increase = 0.002;
                    break;
                case HitResult.Ok:
                    increase = 0.0004;
                    break;
            }

            return HpMultiplierNormal * increase;
        }
    }
}
