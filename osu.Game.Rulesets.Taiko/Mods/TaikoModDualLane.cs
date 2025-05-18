// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModDualLane : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<TaikoHitObject>
    {

        public override string Name => "Dual Lane";

        public override string Acronym => "DL";
        public override LocalisableString Description => @"dual lane";
        public override double ScoreMultiplier => 0.75;

        [SettingSource("Fade Distance", "Adjust how hidden the hitobjects are.", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> HiddenMultiplier { get; } = new(1)
        {
            MinValue = 0.4f,
            MaxValue = 1.4f,
            Precision = 0.01f,
        };

        /// <summary>
        /// How far away from the hit target should hitobjects start to fade out.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_start_time = 1f;

        /// <summary>
        /// How long hitobjects take to fade out, in terms of the scrolling length.
        /// Range: [0, 1]
        /// </summary>
        private const float fade_out_duration = 0.375f;

        private DrawableTaikoRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            ApplyNormalVisibilityState(hitObject, state);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                case DrawableDrumRollTick:
                case DrawableHit:
                    var taikoHitObject = (TaikoStrongableHitObject)hitObject.HitObject;
                    bool blue = taikoHitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE);
                    hitObject.Y = blue ? -40 : 40;

                    break;
            }
        }
    }
}
