using BigEvil.Content.Reworks.Brain.Projectiles;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace BigEvil.Common.Netcode.Packets
{
    internal class BrainIllusionHitPacket : Packet
    {
        protected override void Write(BinaryWriter writer, object[] args)
        {
            writer.Write((byte)args[0]);
            writer.Write((byte)args[1]);
        }

        public override void HandlePacket(BinaryReader packet, int sender)
        {
            var bytes = packet.ReadBytes(2);
            NPC illusion = Main.npc[(int)bytes[0]];
            Player fool = Main.player[(int)bytes[1]];
            if (Main.dedServ)
                Projectile.NewProjectile(illusion.GetSource_FromThis(), illusion.Center, Vector2.Zero, ModContent.ProjectileType<TelekineticBlast>(), 50, 0.5f, -1, fool.whoAmI, 8, illusion.whoAmI);
            illusion.dontTakeDamage = true;
            illusion.netUpdate = true;
        }
    }
}
