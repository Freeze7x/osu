// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModStaticBPM : Mod, IUpdatableByPlayfield, IApplicableToBeatmap, IApplicableToRate
    {

        public override double ScoreMultiplier => 0.5;

        public sealed override bool ValidForFreestyleAsRequiredMod => true;
        public sealed override bool ValidForMultiplayerAsFreeMod => false;

        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModAdaptiveSpeed) };

        private IBeatmap? beatmap;

        public BindableNumber<double> SpeedChange { get; } = new BindableDouble(1);

        public override string Name => "Static BPM";

        public override string Acronym => "SB";
        public override IconUsage? Icon => FontAwesome.Solid.Clock;

        public override LocalisableString Description => "No more tricky BPM changes.";

        [SettingSource("BPM", "The BPM to maintain throughout the beatmap.", SettingControlType = typeof(SettingsSlider<float, BPMSlider>))]
        public BindableFloat BPMStay { get; } = new BindableFloat(120)
        {
            MinValue = 29,
            MaxValue = 400,
            Precision = 1,
        };

        private readonly RateAdjustModHelper rateAdjustHelper;
        private double? lastRate;
        public ModStaticBPM()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);
            rateAdjustHelper.HandleAudioAdjustments(new(false));
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            rateAdjustHelper.ApplyToTrack(track);
        }

        public void ApplyToSample(IAdjustableAudioComponent sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            this.beatmap = beatmap;
            SpeedChange.SetDefault();
        }

        public double ApplyToRate(double time, double rate = 1)
        {
            if (beatmap == null) return 1;
            return beatmap!.ControlPointInfo.TimingPointAt(time).BeatLength / (BPMStay.Value == 29 ? beatmap.ControlPointInfo.TimingPoints[0].BeatLength : (60000 / BPMStay.Value));
        }

        public void Update(Playfield playfield)
        {
            double rate = ApplyToRate(playfield.Clock.CurrentTime);
            if (lastRate == rate) return;
            lastRate = rate;
            SpeedChange.Value = rate;
        }
    }
    public partial class BPMSlider : RoundedSliderBar<float>
    {
        public override LocalisableString TooltipText => Current.Value == 29 ? "First BPM Change" : base.TooltipText;
    }
}
