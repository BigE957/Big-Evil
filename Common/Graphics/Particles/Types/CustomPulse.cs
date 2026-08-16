using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types
{
    public class CustomPulse : Particle
    {
        public bool UseAltVisual = true;
        public override bool Additive => UseAltVisual;

        private readonly string NewTexture;
        private readonly float OriginalScale;
        private readonly float FinalScale;
        private readonly float BaseOpacity;
        private float opacity;
        private readonly bool FadeOut;
        private Vector2 Squish;
        private Color BaseColor;
        private readonly float MakeLight;
        readonly SpriteEffects Effects = SpriteEffects.None;

        public CustomPulse(Vector2 position, Vector2 velocity, Color color, string texture, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime, bool UseAdditiveBlend = true, float baseOpacity = 1f, bool fade = true, float makeLight = 1, SpriteEffects effects = SpriteEffects.None)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            NewTexture = texture;
            OriginalScale = originalScale;
            FinalScale = finalScale;
            Scale = originalScale * Vector2.One;
            Lifetime = lifeTime;
            BaseOpacity = baseOpacity;
            FadeOut = fade;
            Squish = squish;
            Effects = effects;
            Rotation = rotation;
            UseAltVisual = UseAdditiveBlend;
            MakeLight = makeLight;
        }

        public override void Update()
        {
            float ratio = MathUtils.PolyOutEasing(MathHelper.Clamp(LifeRatio, 0f, 1f), 4);
            Scale = Vector2.One * MathHelper.Lerp(OriginalScale, FinalScale, ratio);

            opacity = (FadeOut ? (float)Math.Sin(MathHelper.PiOver2 + LifeRatio * MathHelper.PiOver2) : 1f) * BaseOpacity;

            Color = BaseColor * opacity;
            if (MakeLight > 0)
                Lighting.AddLight(Position, (Color.R / 255f) * MakeLight, (Color.G / 255f) * MakeLight, (Color.B / 255f) * MakeLight);
            Velocity *= 0.95f;

            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(NewTexture).Value;
            float scaleMult = 1;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity, Rotation, tex.Size() / 2f, Scale * Squish * scaleMult, Effects, 0);
        }
    }
}
