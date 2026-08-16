using BigEvil.Common.Netcode;
using System.IO;
using Terraria.ModLoader;

namespace BigEvil
{
	public class BigEvilMod : Mod
	{
        internal static BigEvilMod Instance => _instance ??= ModContent.GetInstance<BigEvilMod>();
        private static BigEvilMod _instance;

        public override void HandlePacket(BinaryReader bb, int whoAmI)
        {
            BigEvilNet.HandlePacket(bb, whoAmI);
        }
    }
}
