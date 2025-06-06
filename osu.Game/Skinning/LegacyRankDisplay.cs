// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        private readonly Sprite rankDisplay;

        private IBindable<ScoreRank> rank = null!;

        public LegacyRankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(rankDisplay = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            rank = scoreProcessor.Rank.GetBoundCopy();
            rank.BindValueChanged(r =>
            {
                var texture = source.GetTexture($"ranking-{r.NewValue}-small");

                rankDisplay.Texture = texture;

                if (texture != null)
                {
                    var transientRank = new Sprite
                    {
                        Texture = texture,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BypassAutoSizeAxes = Axes.Both,
                    };
                    AddInternal(transientRank);
                    transientRank.FadeOutFromOne(500, Easing.Out)
                                 .ScaleTo(new Vector2(1.625f), 500, Easing.Out)
                                 .Expire();
                }
            }, true);

            FinishTransforms(true);
        }
    }
}
