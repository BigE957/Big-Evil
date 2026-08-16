using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types
{
    public class BloodSplatter : Particle
    {
        public Color InitialColor;
        private static Asset<Texture2D> Texture;
        public override bool Additive => true;

        public override void Load()
        {
            Texture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/Particles/Types/BloodSplatter");
        }

        public BloodSplatter(Vector2 relativePosition, Vector2 velocity, int lifetime, float scale, Color color)
        {
            Position = relativePosition;
            Velocity = velocity;
            Scale = scale * Vector2.One;
            Lifetime = lifetime;
            Color = InitialColor = color;
        }

        public override void Update()
        {
            Velocity *= 0.98f;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifeRatio, 4D));
            Rotation = Velocity.ToRotation();
            // Cycle through disco colors if blood and gore is off
            if (!ChildSafety.Disabled)
            {
                Color = Main.DiscoColor;
            }

            base.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float brightness = (float)Math.Pow(Lighting.Brightness((int)(Position.X / 16f), (int)(Position.Y / 16f)), 0.15);
            Texture2D texture = Texture.Value;
            Rectangle frame = texture.Frame(1, 3, 0, (int)(LifeRatio * 3f));
            Vector2 origin = frame.Size() * 0.5f;

            spriteBatch.Draw(texture, Position - Main.screenPosition, frame, Color * brightness, Rotation, origin, Scale, 0, 0f);
        }
    }
}
