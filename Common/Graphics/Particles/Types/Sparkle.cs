using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Graphics.Particles.Types;

public class Sparkle : Particle
{
    private static Asset<Texture2D> Texture;
    private static Asset<Texture2D> BloomTexture;
    public bool UseAltVisual = true;
    public override bool Additive => UseAltVisual;
    public override bool Important => important;

    public bool important;
    private readonly float Spin;
    private float opacity;
    private Color bloomColor;
    private Color LightColor => bloomColor * opacity;
    private readonly float BloomScale;

    public override void Load()
    {
        Texture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/Particles/Types/Sparkle");
        BloomTexture = ModContent.Request<Texture2D>("BigEvil/Common/Graphics/BloomCircle");
    }

    public Sparkle(Vector2 position, Vector2 velocity, Color color, Color bloom, float scale, int lifeTime, float rotationSpeed = 0f, float bloomScale = 1f, bool AddativeBlend = true, bool needed = false)
    {
        Position = position;
        Velocity = velocity;
        Color = color;
        bloomColor = bloom;
        Scale = scale * Vector2.One;
        Lifetime = lifeTime;
        Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        Spin = rotationSpeed;
        BloomScale = bloomScale;
        UseAltVisual = AddativeBlend;
        important = needed;
    }

    public override void Update()
    {
        opacity = (float)Math.Sin(LifeRatio * MathHelper.Pi);
        Lighting.AddLight(Position, LightColor.R / 255f, LightColor.G / 255f, LightColor.B / 255f);
        Velocity *= 0.95f;
        Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D starTexture = Texture.Value;
        Texture2D bloomTexture = BloomTexture.Value;
        float properBloomSize = starTexture.Height / (float)bloomTexture.Height;

        spriteBatch.Draw(bloomTexture, Position - Main.screenPosition, null, bloomColor * opacity * 0.5f, 0, bloomTexture.Size() / 2f, Scale * BloomScale * properBloomSize, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Position - Main.screenPosition, null, Color * opacity * 0.5f, Rotation + MathHelper.PiOver4, starTexture.Size() / 2f, Scale * 0.75f, SpriteEffects.None, 0);
        spriteBatch.Draw(starTexture, Position - Main.screenPosition, null, Color * opacity, Rotation, starTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
    }
}
