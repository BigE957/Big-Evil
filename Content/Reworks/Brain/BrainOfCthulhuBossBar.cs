using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Content.Reworks.Brain
{
    internal class BrainOfCthulhuBossBar : ModBossBar
    {
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) => TextureAssets.NpcHeadBoss[NPCID.Sets.BossHeadTextures[NPCID.BrainofCthulhu]];

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC target = Main.npc[info.npcIndexToAimAt];
            if (!target.active)
                return false;

            // Get the boss health, obviously
            life = target.life;
            lifeMax = target.lifeMax;

            // Reset the shield
            shield = 0f;
            shieldMax = 0f;

            if (NPC.AnyNPCs(NPCID.Creeper))
            {
                foreach (NPC creeper in Main.ActiveNPCs)
                {
                    if (creeper.type != NPCID.Creeper)
                        continue;
                    shieldMax = creeper.lifeMax * BrainOfCthulhuAI.GetBrainOfCthuluCreepersCount();
                    shield += creeper.life;
                }
            }
            return true;
        }
    }
}
