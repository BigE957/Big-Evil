using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace BigEvil.Common.Graphics.Particles.Types
{
    public class BrainOfCthulhuAfterImage : Particle
    {
        public override bool Important => true;

        private float StartFade;
        private float Opacity = 1f;
        List<Vector2> Path;
        private Vector2 MyScale = Vector2.One;
        private Rectangle Frame;

        public BrainOfCthulhuAfterImage(BezierCurve path, float rotation, Vector2 scale, int lifeTime, Rectangle frame, float startFadeRatio = 0f)
        {
            Path = path.GetPoints(lifeTime);
            Position = Path[0];
            Rotation = rotation;
            StartFade = startFadeRatio;
            MyScale = scale;
            Frame = frame;
            Lifetime = lifeTime + 1;
        }

        public override void Update()
        {
            float timeRatio = Time / (float)Lifetime;
            Opacity = timeRatio;
            if (StartFade != 0f)
                Opacity = Utils.GetLerpValue(StartFade, 1f, timeRatio, true);

            Opacity = 1 - MathUtils.SineInEasing(Opacity);

            List<Vector2> pathPosition = Path;

            if (Time >= pathPosition.Count)
            {
                ParticleSystem.RemoveParticle(this);
                return;
            }

            Position = pathPosition[Time];

            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float timeRatio = Time / (float)Lifetime;
            spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, Position - Main.screenPosition, Frame, Lighting.GetColor(Position.ToTileCoordinates()) * Opacity * 0.5f, Rotation, Frame.Size() * 0.5f, MyScale, 0, 0);
        }
    }
}
