// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osu.Game.Utils;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModDrainingHealth : Mod, IApplicableHealthProcessor, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Name => @"Draining Health";
        public override string Acronym => @"DH";
        public override LocalisableString Description => @"The health goes... down?";
        public override double ScoreMultiplier => 0.5;
        public override IconUsage? Icon => FontAwesome.Solid.Heart;
        public override ModType Type => ModType.Conversion;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableRuleset.Alpha = drawableRuleset.Alpha;
        }

        public HealthProcessor? CreateHealthProcessor(double drainStartTime)
        {
            return new TaikoDrainingHealthProcessor(drainStartTime);
        }
    }
}
