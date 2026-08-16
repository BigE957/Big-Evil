using System;
using System.Linq;
using BigEvil.Common.Graphics.Particles;
using BigEvil.Common.Graphics.Particles.Types;
using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BigEvil.Content.Reworks.Brain.Projectiles;

public class TelekineticBlast : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "BigEvil/Common/Graphics/Nothing";

    public override void SetDefaults()
    {
        Projectile.width = 1;
        Projectile.height = 1;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 10;
        Projectile.damage = 0;
        Projectile.scale = 1;
        Projectile.hostile = true;
        Projectile.netImportant = true;
    }

    Player target => Main.player[(int)Projectile.ai[0]];
    float debuffMultiplier => Main.npc[NPCSource].type == NPCID.BrainofCthulhu ? 2f : 1f;
    int delay { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
    int NPCSource => (int)Projectile.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        if (Main.npc[NPC.crimsonBoss].AIOverride<BrainOfCthulhuAI>().AttackFlag || Main.npc[NPC.crimsonBoss].AIOverride<BrainOfCthulhuAI>().AIState == BrainOfCthulhuAI.BrainAIState.DeathAnimation)
        {
            Projectile.active = false;
            return;
        }
        
        if (--delay > 0)
            return;

        if (Main.npc[NPCSource].ModNPC is FalseBrain illusion)
        {
            illusion.BeenHit = true;
            Main.npc[NPCSource].netUpdate = true;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2 dir = target.Center - Projectile.Center;
            int lifeTime = 24;
            dir /= lifeTime / 2f * 5f;
            dir *= i;
            DirectionalPulseRing pulse = new(Projectile.Center, dir, i % 2 == 0 ? Color.Red : Color.Orange, new Vector2(0.5f, 1), dir.ToRotation(), 0f, i / 5f, lifeTime + 8);
            ParticleSystem.SpawnParticle(pulse);
        }

        SoundEngine.PlaySound(BrainOfCthulhuAI.Laugh, Projectile.Center);
        target.AddBuff(BuffID.Darkness, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Bleeding, (int)Math.Round(900 * debuffMultiplier));
        target.AddBuff(BuffID.Confused, (int)Math.Round(60 * debuffMultiplier));
        int timeToAdd = (int)Math.Round(300 * debuffMultiplier);
        int bbIndex = target.buffType.ToList().IndexOf(BuffID.OnFire3/*ModContent.BuffType<BurningBlood>()*/);
        if (bbIndex != -1)
            timeToAdd += target.buffTime[bbIndex];
        if (timeToAdd > 3600)
            timeToAdd = 3600;

        target.AddBuff(BuffID.OnFire3/*ModContent.BuffType<BurningBlood>()*/, timeToAdd);

        target.Hurt(PlayerDeathReason.ByCustomReason(Language.GetText("Mods.BigEvils.DeathReasons.BrainIllusion" + Main.rand.Next(1, 3 + 1)).ToNetworkText(target.name)), 100, Main.npc[NPC.crimsonBoss].Center.X > target.Center.X ? -1 : 1, cooldownCounter: 0, dodgeable: false, scalingArmorPenetration: 1f);

        Projectile.active = false;
    }
}
