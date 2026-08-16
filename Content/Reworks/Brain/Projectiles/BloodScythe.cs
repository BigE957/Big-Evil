using System.IO;
using BigEvil.Common.Graphics.Particles;
using BigEvil.Common.Graphics.Particles.Types;
using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Content.Reworks.Brain.Projectiles;

public class BloodScythe : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "BigEvil/Common/Graphics/Particles/Types/VerticalSmearRagged";

    private Vector2 InitialVelocity = Vector2.Zero;
    private Vector2 AcceleratingVelocity = Vector2.Zero;
    private static float RotationSpeed => MathHelper.Pi / 8f;
    private static float Acceleration => 0.175f;
    private static int Lifetime => 240;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 16;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 300;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = Lifetime;
        Projectile.damage = 10;
        Projectile.scale = 0.1f;
        Projectile.hostile = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        InitialVelocity = Projectile.velocity;
        Projectile.rotation = Projectile.velocity.ToRotation();
        for (int i = 0; i < 3; i++)
        {
            BloodDrop p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
            ParticleSystem.SpawnParticle(p);
        }
        BloodSplatter p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
        ParticleSystem.SpawnParticle(p2);
    }

    public override void AI()
    {
        int UpTime = Lifetime - Projectile.timeLeft;
        InitialVelocity *= 0.925f;
        if (UpTime > 15)
        {
            AcceleratingVelocity += Projectile.velocity.SafeNormalize(InitialVelocity.SafeNormalize(Vector2.UnitX)) * Acceleration;
            if (Main.rand.NextBool(1 + Projectile.timeLeft / 32))
            {
                BloodDrop p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (-Projectile.velocity).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                ParticleSystem.SpawnParticle(p);
            }
        }
        Projectile.velocity = InitialVelocity + AcceleratingVelocity;
        Projectile.rotation += RotationSpeed;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.WritePackedVector2(InitialVelocity);
        writer.WritePackedVector2(AcceleratingVelocity);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        InitialVelocity = reader.ReadPackedVector2();
        AcceleratingVelocity = reader.ReadPackedVector2();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Color drawColor = (ChildSafety.Disabled ? Color.Red : Main.DiscoColor) with { A = 0 };

        for (int i = 0; i < Projectile.oldPos.Length; ++i)
        {
            float afterimageRot = Projectile.oldRot[i];
            Vector2 drawPos = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            if (i != 0)
                drawColor *= 0.9f;

            float interpolant = ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(tex, drawPos, null, drawColor, afterimageRot, tex.Size() * 0.5f, Projectile.scale * interpolant, SpriteEffects.None);
        }

        return false;
    }
}

public class CirclingBloodScythe : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "BigEvil/Common/Graphics/Particles/Types/VerticalSmearRagged";

    private static float RotationSpeed => MathHelper.Pi / 8f;
    private static int Lifetime => BrainOfCthulhuAI.CrimsonEyeAttackDuration;
    private static float MaxCircleSpeed => MathHelper.Pi / 30f; //1 rev per second

    ref float CircleAngle => ref Projectile.ai[0];
    ref float CircleRadius => ref Projectile.ai[1];
    ref float CircleRadiusVelocity => ref Projectile.ai[2];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 16;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 300;
        Projectile.height = 300;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = Lifetime;
        Projectile.damage = 10;
        Projectile.scale = 0.1f;
        Projectile.hostile = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = CircleAngle;
        for (int i = 0; i < 3; i++)
        {
            BloodDrop p = new(Projectile.Center, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.5f, 1f), 32, 1f, Color.Red);
            ParticleSystem.SpawnParticle(p);
        }
        BloodSplatter p2 = new(Projectile.Center, Projectile.velocity * 0.75f, 16, 0.5f, Color.Red);
        ParticleSystem.SpawnParticle(p2);
    }

    public override void AI()
    {
        if (NPC.crimsonBoss == -1)
        {
            Projectile.active = false;
            return;
        }
        int UpTime = Lifetime - Projectile.timeLeft;

        if (UpTime < 30f)
            CircleRadius = MathHelper.Lerp(0f, 128f, MathUtils.CircOutEasing(UpTime / 30f));
        else if (UpTime < Lifetime - 180)
            CircleRadius = 128f;
        else
        {
            if (UpTime == Lifetime - 180)
                CircleRadiusVelocity = -3f;
            CircleRadius += CircleRadiusVelocity;
            CircleRadiusVelocity += 0.1f;
        }

        if (UpTime >= 15f)
        {
            if (UpTime < 30f)
                CircleAngle += MathHelper.Lerp(0f, MaxCircleSpeed, MathUtils.SineInEasing((UpTime - 15) / 15f));
            else if (UpTime < 870)
                CircleAngle += MaxCircleSpeed;
            else if (UpTime < 900)
                CircleAngle += MathHelper.Lerp(MaxCircleSpeed, MaxCircleSpeed / 3f, MathUtils.SineOutEasing((UpTime - 870) / 30f));
            else
                CircleAngle += MaxCircleSpeed / 3f;

            if (Main.rand.NextBool(6))
            {
                BloodDrop p = new(Projectile.Center + Main.rand.NextVector2CircularEdge(32, 32), (Projectile.DirectionTo(Main.npc[NPC.crimsonBoss].Center).RotatedBy(MathHelper.PiOver2) * 16f).RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 6f, MathHelper.Pi / 6f)) * Main.rand.NextFloat(0.25f, 0.75f), Main.rand.Next(10, 17), 1f, Color.Red);
                ParticleSystem.SpawnParticle(p);
            }
        }

        NPC boss = Main.npc[NPC.crimsonBoss];

        Projectile.Center = boss.Center + (CircleAngle.ToRotationVector2() * CircleRadius);
        Projectile.rotation += RotationSpeed;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.End(out var snap);
        var additive = snap;
        additive.BlendState = BlendState.Additive;
        Main.spriteBatch.Begin(additive);

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Color drawColor = Color.Red;
        if (!ChildSafety.Disabled)
            drawColor = Main.DiscoColor;

        for (int i = 0; i < Projectile.oldPos.Length; ++i)
        {
            float afterimageRot = Projectile.oldRot[i];
            Vector2 drawPos = Projectile.oldPos[i] + (Projectile.Size / 2f) - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            if (i != 0)
                drawColor *= 0.9f;
            float interpolant = ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
            Main.spriteBatch.Draw(tex, drawPos, null, drawColor, afterimageRot, tex.Size() * 0.5f, Projectile.scale * interpolant, SpriteEffects.None, 0f);
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(snap);

        return false;
    }
}

