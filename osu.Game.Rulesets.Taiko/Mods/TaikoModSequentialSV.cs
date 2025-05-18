// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public partial class TaikoModSequential : Mod, IApplicableToBeatmap, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Name => @"Sequential Scrolling";
        public override string Acronym => @"SS";
        public override LocalisableString Description => @"Change the scrolling algorithm to match osu!mania.";
        public override double ScoreMultiplier => 0.6;
        public override IconUsage? Icon => OsuIcon.RulesetMania;
        public override ModType Type => ModType.Conversion;
        [SettingSource("Interpolate scroll speed", "Should the scroll speed be interpolated where valid")]
        public BindableBool Interpolate { get; } = new BindableBool(true);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!Interpolate.Value) return;

            List<(double, EffectControlPoint)> toAdd = [];
            foreach (var tp in beatmap.ControlPointInfo.EffectPoints)
            {
                var nextTp = beatmap.ControlPointInfo.EffectPoints.FirstOrDefault(i => i.Time > tp.Time);

                if (nextTp == null) continue;

                if (nextTp.Time - tp.Time < 500)
                {
                    for (double i = tp.Time; i < nextTp.Time; i += 50)
                    {
                        double progress = (i - tp.Time) / (nextTp.Time - tp.Time);
                        if (progress > 1) break;

                        EffectControlPoint newPoint = new()
                        {
                            KiaiMode = tp.KiaiMode,
                            ScrollSpeed = Interpolation.Lerp(tp.ScrollSpeed, nextTp.ScrollSpeed, progress)
                        };
                        toAdd.Add((i, newPoint));
                    }
                }
            }

            toAdd.ForEach(i => beatmap.ControlPointInfo.Add(i.Item1, i.Item2));

        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            var drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.VisualisationMethod = ScrollVisualisationMethod.Sequential;
        }
    }
}
