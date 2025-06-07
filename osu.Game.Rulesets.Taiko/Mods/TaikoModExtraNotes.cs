// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModExtraNotes : Mod, IApplicableToBeatmap
    {
        public override string Name => "Extra Notes";
        public override string Acronym => "EN";
        public override double ScoreMultiplier => 0.9;
        public override LocalisableString Description => "Add extra notes in between certain snappings.";
        public override ModType Type => ModType.Conversion;
        public override Type[] IncompatibleMods => new[] { typeof(TaikoModSimplifiedRhythm) };

        [SettingSource("1/1 pattern addition", "Adds a note between 1/1 patterns.")]
        public Bindable<bool> OneWholeConversion { get; } = new BindableBool();

        [SettingSource("1/2 pattern addition", "Adds a note between 1/2 patterns.")]
        public Bindable<bool> OneHalfConversion { get; } = new BindableBool(true);

        [SettingSource("1/3 pattern addition", "Adds a note between 1/3 patterns.")]
        public Bindable<bool> OneThirdConversion { get; } = new BindableBool();

        [SettingSource("Invert additional note", "Set the added note to be the opposite color of the copied one.")]
        public Bindable<bool> InvertAddition { get; } = new BindableBool();

        [SettingSource("Note to copy", "Choose which note's color to copy when adding a new note.")]
        public Bindable<NoteSideToCopy> NoteSideToCopySetting { get; } = new Bindable<NoteSideToCopy>(NoteSideToCopy.Left);

        [SettingSource("BPM Multiplier", "If the beatmap's BPM is double or half of what it sounds like, apply this to BPM before adding notes.")]
        public Bindable<BPMMultiplier> BeatmapBPMMultiplier { get; } = new Bindable<BPMMultiplier>(BPMMultiplier.Whole);
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            var controlPointInfo = taikoBeatmap.ControlPointInfo;

            Hit[] hits = taikoBeatmap.HitObjects.Where(obj => obj is Hit).Cast<Hit>().ToArray();

            if (hits.Length == 0)
                return;

            var conversions = new List<int>();
            var toAdd = new List<Hit>();

            if (OneWholeConversion.Value) conversions.Add(1);
            if (OneHalfConversion.Value) conversions.Add(2);
            if (OneThirdConversion.Value) conversions.Add(3);

            foreach (int baseRhythm in conversions)
            {
                for (int i = 1; i < hits.Length; i++)
                {
                    Hit currentNote = hits[i - 1];
                    TaikoHitObject taikoHitObject = currentNote;
                    Hit nextNote = hits[i];
                    Hit toCopy = NoteSideToCopySetting.Value == NoteSideToCopy.Left ? currentNote : nextNote;
                    double snapValue = Math.Round(getSnapBetweenNotes(controlPointInfo, currentNote, nextNote) * 10) / 10; // Round to nearest 0.1 to avoid near misses.

                    if (snapValue == baseRhythm && !currentNote.IsStrong && !nextNote.IsStrong && taikoHitObject is not Swell or DrumRoll)
                    {
                        //var noteSample = currentNote.Samples;
                        toAdd.Add(new Hit
                        {
                            StartTime = currentNote.StartTime + (nextNote.StartTime - currentNote.StartTime) / 2,
                            Samples = toCopy.Samples,
                            HitWindows = toCopy.HitWindows,
                            Type = InvertAddition.Value
                                ? (toCopy.Type == HitType.Centre ? HitType.Rim : HitType.Centre)
                                : toCopy.Type
                        });
                    }
                }
            }

            taikoBeatmap.HitObjects.AddRange(toAdd);
            taikoBeatmap.HitObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        }

        private double getSnapBetweenNotes(ControlPointInfo controlPointInfo, TaikoHitObject currentNote, TaikoHitObject nextNote)
        {
            var currentTimingPoint = controlPointInfo.TimingPointAt(currentNote.StartTime);
            double beatLength = currentTimingPoint.BeatLength / (Math.Pow(2, (int)BeatmapBPMMultiplier.Value) / 2);
            double difference = nextNote.StartTime - currentNote.StartTime;
            return 1 / (difference / beatLength);
        }

        public enum NoteSideToCopy
        {
            Left,
            Right
        }

        public enum BPMMultiplier
        {
            Half,
            Whole,
            Double,
        }
    }
}
