using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types
{
    public class BloodDrop : Particle
    {
        public Color InitialColor;

        private static Asset<Texture2D> Texture;
        public override bool Additive => true;

        public override void Load()
        {
            Texture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/Particles/Types/BloodDrop");
        }

        public BloodDrop(Vector2 relativePosition, Vector2 velocity, int lifetime, float scale, Color color)
        {
            Position = relativePosition;
            Velocity = velocity;
            Scale = scale * Vector2.One;
            Lifetime = lifetime;
            Color = InitialColor = color;
        }

        public override void Update()
        {
            Scale *= 0.98f;
            Velocity.X *= 0.97f;
            Velocity.Y = MathHelper.Clamp(Velocity.Y + 0.9f, -22f, 22f);
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifeRatio, 3D));
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            // Cycle through disco colors if blood and gore is off
            if (!ChildSafety.Disabled)
            {
                Color = Main.DiscoColor;
            }

            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float verticalStretch = Utils.GetLerpValue(0f, 24f, Math.Abs(Velocity.Y), true) * 0.84f;
            float brightness = (float)Math.Pow(Lighting.Brightness((int)(Position.X / 16f), (int)(Position.Y / 16f)), 0.15);
            Vector2 scale = new Vector2(1f, verticalStretch + 1f) * Scale * 0.1f;
            Texture2D texture = Texture.Value;

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * brightness, Rotation, texture.Size() * 0.5f, scale, 0, 0f);
        }
    }
}
