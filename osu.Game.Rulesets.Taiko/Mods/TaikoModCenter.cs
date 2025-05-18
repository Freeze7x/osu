// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using OpenTabletDriver.Plugin;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModCenter : Mod, IApplicableToDrawableHitObject, IApplicableToDrawableRuleset<TaikoHitObject>
    {
        public override string Name => "Centered";
        public override string Acronym => "CT";
        public override LocalisableString Description => "Hit objects come from both sides!";
        public override double ScoreMultiplier => 0.9;
        public override IconUsage? Icon { get; } = FontAwesome.Regular.ArrowAltCircleDown;

        //public override Type[] IncompatibleMods => new[] { typeof(IHidesApproachCircles), typeof(OsuModFreezeFrame) };

        [SettingSource("Style", "Change the animation style of the approach circles.", 1)]
        public Bindable<AnimationStyle> Style { get; } = new Bindable<AnimationStyle>(AnimationStyle.Gravity);

        private DrawableTaikoRuleset drawableRuleset = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableTaikoRuleset)drawableRuleset;
            var tp = (TaikoPlayfield)this.drawableRuleset.Playfield;
            tp.HitObjectContainer.Anchor = Anchor.TopCentre;
            Logger.Log(tp.X.ToString());
        }
        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.ApplyCustomUpdateState += (drawableObject, _) =>
            {
                var j = drawableObject.HitObject;

                double preempt = drawableRuleset.TimeRange.Value / drawableRuleset.ControlPointAt(j.StartTime).Multiplier;
                double end = j.StartTime;
                double start = end - preempt;
                double duration = Math.Max(end - start, 0);
                drawableObject.ClearTransforms(targetMember: nameof(drawable.X));
                //drawableObject.ClearTransforms();//
                using (drawableObject.BeginAbsoluteSequence(start))
                {
                    if (j is not TaikoStrongableHitObject) return;
                    var taikoHitObject = (TaikoStrongableHitObject)j;
                    bool blue = taikoHitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE);
                    //drawableObject.MoveToX(600).MoveToX(0, duration, getEasing(Style.Value));
                    drawableObject.MoveToX(drawableObject.X * (blue ? -1 : 1)).MoveToX(0, duration, getEasing(Style.Value));
                }
            };
        }

        private Easing getEasing(AnimationStyle style)
        {
            switch (style)
            {
                case AnimationStyle.Linear:
                    return Easing.None;

                case AnimationStyle.Gravity:
                    return Easing.InBack;

                case AnimationStyle.InOut1:
                    return Easing.InOutCubic;

                case AnimationStyle.InOut2:
                    return Easing.InOutQuint;

                case AnimationStyle.Accelerate1:
                    return Easing.In;

                case AnimationStyle.Accelerate2:
                    return Easing.InCubic;

                case AnimationStyle.Accelerate3:
                    return Easing.InQuint;

                case AnimationStyle.Decelerate1:
                    return Easing.Out;

                case AnimationStyle.Decelerate2:
                    return Easing.OutCubic;

                case AnimationStyle.Decelerate3:
                    return Easing.OutQuint;

                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, @"Unsupported animation style");
            }
        }

        public enum AnimationStyle
        {
            Linear,
            Gravity,
            InOut1,
            InOut2,
            Accelerate1,
            Accelerate2,
            Accelerate3,
            Decelerate1,
            Decelerate2,
            Decelerate3,
        }
    }
}
