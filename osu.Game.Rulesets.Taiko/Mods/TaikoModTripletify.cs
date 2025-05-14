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
    public class TaikoModTripletify : Mod, IApplicableToBeatmap
    {
        public override string Name => "Tripletify";
        public override string Acronym => "TP";
        public override double ScoreMultiplier => 0.6;
        public override LocalisableString Description => "Jazzy.";
        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;
            var controlPointInfo = taikoBeatmap.ControlPointInfo;

            Hit[] hits = taikoBeatmap.HitObjects.Where(obj => obj is Hit).Cast<Hit>().ToArray();

            if (hits.Length == 0)
                return;

            var conversions = new List<(int, int)>();
            List<(double start, TimingControlPoint tp, List<TaikoHitObject> hitObjects)> beats = new();

            foreach (var tp in taikoBeatmap.ControlPointInfo.TimingPoints)
            {
                double end = taikoBeatmap.ControlPointInfo.TimingPointAfter(tp.Time)?.Time ?? taikoBeatmap.HitObjects.Last().StartTime;
                for (double i = tp.Time; i < end; i += tp.BeatLength)
                {
                    double beatEnd = i + tp.BeatLength;
                    var hitObjectsBeat = taikoBeatmap.HitObjects.Where(j => j.StartTime >= i && j.StartTime < beatEnd - 5).ToList();
                    beats.Add((i, tp, hitObjectsBeat));
                }
            }


            List<TaikoHitObject> toDelete = [];
            foreach (var beat in beats)
            {
                foreach (var hitObject in beat.hitObjects)
                {
                    double offset = hitObject.StartTime - beat.start;
                    double newOffset = offset / 0.75;

                    if (newOffset >= beat.tp.BeatLength - 5)
                        toDelete.Add(hitObject);
                    else
                        hitObject.StartTime = beat.start + newOffset;
                }
            }

            taikoBeatmap.HitObjects.RemoveAll(j => toDelete.Contains(j));
            taikoBeatmap.HitObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        }
    }
}
