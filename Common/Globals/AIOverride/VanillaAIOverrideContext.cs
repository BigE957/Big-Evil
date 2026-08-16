using Terraria;

namespace BigEvil.Common.Globals.AIOverride
{
    public class VanillaAIOverrideContext
    {
        public NPC NPC { get; init; }
        public int NPCType { get; init; }
        public VanillaAIOverride OverrideToApply { get; set; }
    }
}
