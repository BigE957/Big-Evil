using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types;

public class DirectionalPulseRing : Particle
{
    private static Asset<Texture2D> Texture;
    public override bool Additive => true;

    private readonly float OriginalScale;
    private readonly float FinalScale;
    private float opacity;
    private Vector2 Squish;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/Particles/Types/HollowCircleHardEdge");
    }

    public DirectionalPulseRing(Vector2 position, Vector2 velocity, Color color, Vector2 squish, float rotation, float originalScale, float finalScale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale * Vector2.One;
        Lifetime = lifeTime;
        Squish = squish;
        Rotation = rotation;
    }

    public override void Update()
    {
        float pulseProgress = MathUtils.PolyOutEasing(MathHelper.Clamp(LifeRatio, 0f, 1f), 4);
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, pulseProgress) * Vector2.One;

        opacity = (float)Math.Sin(MathHelper.PiOver2 + LifeRatio * MathHelper.PiOver2);

        Color c = Color * opacity;
        Lighting.AddLight(Position, c.R / 255f, c.G / 255f, c.B / 255f);
        Velocity *= 0.95f;

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D tex = Texture.Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity, Rotation, tex.Size() / 2f, Scale * Squish, SpriteEffects.None, 0);
    }
}
