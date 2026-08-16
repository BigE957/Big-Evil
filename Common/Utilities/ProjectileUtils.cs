using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Utilities
{
    public static class ProjectileUtils
    {
        public static void MakeSpriteCenteredOnInaccurateHitbox(this ModProjectile proj, Vector2 spriteSize)
        {
            proj.DrawOffsetX = (int)((proj.Projectile.width - spriteSize.X) / 2);
            proj.DrawOriginOffsetY = (int)((proj.Projectile.height - spriteSize.Y) / 2);
        }

        public static int CountProjectiles(int projectileID) => Main.projectile.Count(proj => proj.active && proj.type == projectileID);
    }
}
