using BigEvil.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static BigEvil.Content.Reworks.Brain.BrainOfCthulhuAI;

namespace BigEvil.Content.Reworks.Brain.Projectiles
{
    public class BloodshotOverrideGlobalProjectile : GlobalProjectile
    {
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.type == ProjectileID.BloodNautilusShot && projectile.ai[0] != 0 && NPC.crimsonBoss != -1 && Main.npc[NPC.crimsonBoss].TryGetAIOverride<BrainOfCthulhuAI>(out var brainAI))
            {
                if (projectile.localAI[0] == 0f)
                {
                    SoundEngine.PlaySound(SoundID.Item171, projectile.Center);
                    projectile.localAI[0] = 1f;
                    for (int i = 0; i < 8; i++)
                    {
                        Dust blood1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y, 100);
                        blood1.velocity = (Main.rand.NextFloatDirection() * MathHelper.Pi).ToRotationVector2() * 2f + projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
                        blood1.scale = 0.9f;
                        blood1.fadeIn = 1.1f;
                        blood1.position = projectile.Center;
                    }
                }

                projectile.alpha = 0;

                Dust blood2 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y, 100);
                blood2.velocity = blood2.velocity / 4f + projectile.velocity / 2f;
                blood2.scale = 1.2f;
                blood2.position = projectile.Center + Main.rand.NextFloat() * projectile.velocity * 2f;

                int trailLength = projectile.oldPos.Length / 2;
                for (int j = 1; j < trailLength && !(projectile.oldPos[j] == Vector2.Zero); j++)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Dust blood3 = Dust.NewDustDirect(projectile.oldPos[j], projectile.width, projectile.height, DustID.Blood, projectile.velocity.X, projectile.velocity.Y, 100);
                        blood3.velocity = blood3.velocity / 4f + projectile.velocity / 2f;
                        blood3.scale = 1.2f;
                        blood3.position = projectile.oldPos[j] + projectile.Size / 2f + Main.rand.NextFloat() * projectile.velocity * 2f;
                    }
                }

                int startUpTime = 20;
                float speedUpTime = 30;
                float slowDownMult = 0.96f;
                float speedUpMult = 1.025f;
                if (brainAI.AIState == BrainAIState.IllusionDash)
                {
                    startUpTime = 20;
                    speedUpTime = 30;
                    slowDownMult = 0.96f;
                    speedUpMult = 1.025f;
                }

                if (projectile.ai[2] <= startUpTime)
                    projectile.velocity *= slowDownMult;
                else
                {
                    projectile.velocity *= speedUpMult;
                    if (projectile.ai[2] <= startUpTime + speedUpTime)
                    {
                        float newAngle = projectile.ai[1].AngleLerp(projectile.ai[0] - MathHelper.TwoPi, (projectile.ai[2] - startUpTime) / speedUpTime);

                        projectile.velocity = newAngle.ToRotationVector2() * projectile.velocity.Length();
                    }
                }
                projectile.ai[2]++;

                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + MathHelper.PiOver2;

                return false;
            }
            return base.PreAI(projectile);
        }
    }
}
