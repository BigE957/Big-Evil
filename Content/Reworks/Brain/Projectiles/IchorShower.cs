using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Content.Reworks.Brain.Projectiles;

public class IchorShower : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "BigEvil/Common/Graphics/Nothing";

    public override void SetDefaults()
    {
        Projectile.width = 8;
        Projectile.height = 8;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 480;
        Projectile.damage = 10;
        Projectile.scale = 1;
        Projectile.hostile = true;
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        for (int i = 0; i < 3; i++)
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Ichor, Vector2.Zero).noGravity = true;
        if (Main.rand.NextBool(8))
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.GoldFlame, Vector2.UnitX * Projectile.velocity / 10f, Scale: 0.75f);
        Projectile.velocity.Y += 0.075f;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.Ichor, 600);
    }
}

