﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRoll : DrawableTaikoHitObject<DrumRoll>
    {
        /// <summary>
        /// Number of rolling hits required to reach the dark/final accent colour.
        /// </summary>
        private const int rolling_hits_for_dark_accent = 5;

        private Color4 accentDarkColour;

        /// <summary>
        /// Rolling number of tick hits. This increases for hits and decreases for misses.
        /// </summary>
        private int rollingHits;

        public DrawableDrumRoll(DrumRoll drumRoll)
            : base(drumRoll)
        {
            Width = (float)HitObject.Duration;

            Container<DrawableDrumRollTick> tickContainer;
            MainPiece.Add(tickContainer = new Container<DrawableDrumRollTick>
            {
                RelativeSizeAxes = Axes.Both,
                RelativeChildOffset = new Vector2((float)HitObject.StartTime, 0),
                RelativeChildSize = new Vector2((float)HitObject.Duration, 1)
            });

            foreach (var tick in drumRoll.Ticks)
            {
                var newTick = new DrawableDrumRollTick(tick);
                newTick.OnJudgement += onTickJudgement;

                AddNested(newTick);
                tickContainer.Add(newTick);
            }
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoJudgement { SecondHit = HitObject.IsStrong };

        protected override TaikoPiece CreateMainPiece() => new ElongatedCirclePiece();

        public override bool OnPressed(TaikoAction action) => false;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            MainPiece.AccentColour = AccentColour = colours.YellowDark;
            accentDarkColour = colours.YellowDarker;
        }

        private void onTickJudgement(DrawableHitObject<TaikoHitObject, TaikoJudgement> obj)
        {
            if (obj.Judgement.Result == HitResult.Hit)
                rollingHits++;
            else
                rollingHits--;

            rollingHits = MathHelper.Clamp(rollingHits, 0, rolling_hits_for_dark_accent);

            Color4 newAccent = Interpolation.ValueAt((float)rollingHits / rolling_hits_for_dark_accent, AccentColour, accentDarkColour, 0, 1);
            MainPiece.FadeAccent(newAccent, 100);
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
                return;

            if (Judgement.TimeOffset < 0)
                return;

            int countHit = NestedHitObjects.Count(o => o.Judgement.Result == HitResult.Hit);

            if (countHit > HitObject.RequiredGoodHits)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = countHit >= HitObject.RequiredGreatHits ? TaikoHitResult.Great : TaikoHitResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
