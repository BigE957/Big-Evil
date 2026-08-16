using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types;

public class DetailedExplosion : Particle
{
    private static Asset<Texture2D> Texture;
    public override bool Additive => false;

    private readonly float OriginalScale;
    private readonly float FinalScale;
    private float opacity;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/Particles/Types/DetailedExplosion");
    }

    public DetailedExplosion(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime, bool UseAdditiveBlend = true)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Lifetime = lifeTime;
        Scale = squish;
        Rotation = rotation;
    }

    public override void Update()
    {
        opacity = (float)Math.Sin(MathHelper.PiOver2 + LifeRatio * MathHelper.PiOver2);

        Velocity *= 0.95f;

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        float pulseProgress = MathUtils.PolyOutEasing(MathHelper.Clamp(LifeRatio, 0f, 1f), 4);
        Texture2D tex = Texture.Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity, Rotation, tex.Size() / 2f, MathHelper.Lerp(OriginalScale, FinalScale, pulseProgress) * Vector2.One * Scale, SpriteEffects.None, 0);
    }
}
