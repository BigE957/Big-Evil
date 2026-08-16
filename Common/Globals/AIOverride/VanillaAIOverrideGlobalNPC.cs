using BigEvil.Content.Reworks.Brain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BigEvil.Common.Globals.AIOverride;

public sealed partial class VanillaAIOverrideNPC : GlobalNPC
{
    /// <summary>
    /// Toggle Entire System. External mods can toggle this out if they want.
    /// </summary>
    public static bool Enabled { get; set; } = true;

    /// <summary>
    /// Blacklist for non difficulty specific AI changes. External mods can add NPC type to opt-out global changes.
    /// <para>Example: Destroyer Probe's Telegraph Drawing</para>
    /// </summary>
    public static HashSet<int> GlobalChangeBlacklist { get; private set; } = [];

    /// <summary>
    /// Hook to Modify AI Overrides on External mods demand.<br/>
    /// Modifying <see cref="VanillaAIOverrideContext.OverrideToApply"/> will result in NPCs to use that specific AI.
    /// </summary>
    public static event Action<VanillaAIOverrideContext> ModifyAIOverride;

    /// <summary>
    /// Specify the AI Override to work with. This handles AI, SendExtraAI and ReceiveExtraAI in instaned manner.
    /// </summary>
    public VanillaAIOverride AIOverride = null;

    public static Dictionary<Type, int> NetIDLookup = [];

    public const int InvalidNetID = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        if (entity.townNPC) return false;
        if (entity.friendly) return false;
        if (entity.CountsAsACritter) return false;
        return true;
    }

    #region Clone Logic
    public override bool InstancePerEntity => true;

    public override GlobalNPC Clone(NPC npc, NPC npcClone)
    {
        VanillaAIOverrideNPC clone = (VanillaAIOverrideNPC)base.Clone(npc, npcClone);
        if (AIOverride != null)
        {
            clone.AIOverride = AIOverride.Clone();
            clone.AIOverride.NPC = npcClone;
        }
        else
        {
            clone.AIOverride = null;
        }
        return clone;
    }

    #endregion

    #region Vanilla AI Override Rule
    public static VanillaAIOverride GetVanillaAIOverrideToApply(NPC npc)
    {
        if (npc == null)
            return null;

        if (npc.whoAmI < 0 || npc.whoAmI >= Main.maxNPCs)
            return null;

        if (!npc.active)
            return null;

        return npc.type switch
        {
            NPCID.BrainofCthulhu => new BrainOfCthulhuAI(),
            NPCID.Creeper => new CreeperAI(),
            _ => null,
        };
    }
    #endregion

    internal static bool IsGlobalChangeBlacklisted(NPC npc) => GlobalChangeBlacklist.Contains(npc.type);

    internal static void RegisterNetID(VanillaAIOverride aiOverride)
    {
        var id = NetIDLookup.Count + 1;
        NetIDLookup[aiOverride.GetType()] = id;
    }

    public override void Unload()
    {
        NetIDLookup.Clear();
        GlobalChangeBlacklist.Clear();
        ModifyAIOverride = null;
    }

    public override void SetDefaults(NPC npc)
    {
        if (!Enabled)
            return;

        // Clients will get their instance in ReceiveExtraAI
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        AIOverride = GetVanillaAIOverrideToApply(npc);
        if (ModifyAIOverride != null)
        {
            var context = new VanillaAIOverrideContext()
            {
                NPC = npc,
                NPCType = npc.type,
                OverrideToApply = AIOverride
            };
            ModifyAIOverride.Invoke(context);
            AIOverride = context.OverrideToApply;
        }

        if (AIOverride != null)
        {
            AIOverride.NPC = npc;
            AIOverride.SetDefaults(Mod);
        }
    }

    #region Hooks

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (!Enabled)
            return;

        AIOverride?.OnSpawn(Mod);
    }

    public override bool PreAI(NPC npc)
    {
        if (!Enabled)
            return base.PreAI(npc);

        bool result = true;
        if (AIOverride != null)
        {
            result &= AIOverride.AI(Mod);

            if (AIOverride.DisableMultiplayerSmoothing)
            {
                npc.netOffset = Vector2.Zero;
                if (AIOverride.EnableMultiplayerSmoothingAheadOfAI)
                    AIOverride.DisableMultiplayerSmoothing = false;
            }
        }
        return result;
    }

    public override void PostAI(NPC npc)
    {
        if (!Enabled)
            return;

        AIOverride?.PostAI(Mod);
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (!Enabled)
            return base.CanBeHitByProjectile(npc, projectile);

        return AIOverride?.CanBeHitByProjectile(Mod, projectile);
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (!Enabled)
            return;

        AIOverride?.HitEffect(Mod, hit);
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (!Enabled)
            return;

        AIOverride?.ModifyHitByItem(Mod, player, item, ref modifiers);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (!Enabled)
            return;

        AIOverride?.ModifyHitByProjectile(Mod, projectile, ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (!Enabled)
            return;

        AIOverride?.OnHitByItem(Mod, player, item, hit, damageDone);
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (!Enabled)
            return;

        AIOverride?.OnHitByProjectile(Mod, projectile, hit, damageDone);
    }

    public override bool PreKill(NPC npc)
    {
        if (!Enabled || AIOverride == null)
            return base.PreKill(npc);

        return AIOverride.PreKill(Mod);
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (!Enabled || npc.IsABestiaryIconDummy)
            return;

        AIOverride?.FindFrame(Mod, frameHeight);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!Enabled)
            base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        if (npc.IsABestiaryIconDummy)
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        bool result = true;
        result &= AIOverride?.PreDraw(Mod, spriteBatch, screenPos, drawColor) ?? true;
        return result;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!Enabled)
            return;

        AIOverride?.PostDraw(Mod, spriteBatch, screenPos, drawColor);
    }

    #endregion

    #region Networking

    public static int GetNetID(VanillaAIOverride aiOverride)
    {
        if (aiOverride == null)
            return InvalidNetID;

        if (!NetIDLookup.TryGetValue(aiOverride.GetType(), out var netID))
            return InvalidNetID;

        return netID;
    }

    public static bool TryGetNetID(VanillaAIOverride aIOverride, out int netID)
    {
        netID = GetNetID(aIOverride);
        return netID != InvalidNetID;
    }

    public static VanillaAIOverride GetNewInstanceOrNullFromNetID(int netID, NPC ownerNPC)
    {
        var type = NetIDLookup.FirstOrDefault(kv => kv.Value == netID).Key;

        if (type == null)
            return null;

        var instance = (VanillaAIOverride)Activator.CreateInstance(type);
        if (instance == null)
            return null;

        instance.NPC = ownerNPC;
        return instance;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        // OnKill or any similar hooks are not reliable for checking these.
        // As SetDefaults being called on deactivated/dead NPC before ReceiveExtraAI, Prevent Sending ExtraAI is only clean way to do.
        if (!npc.active || npc.life <= 0)
        {
            AIOverride = null;
            binaryWriter.Write7BitEncodedInt(InvalidNetID);
            return;
        }

        if (!TryGetNetID(AIOverride, out var netID))
        {
            binaryWriter.Write7BitEncodedInt(InvalidNetID);
            return;
        }

        binaryWriter.Write7BitEncodedInt(netID);
        AIOverride.SendExtraAI(bitWriter, binaryWriter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        var remoteNetID = binaryReader.Read7BitEncodedInt();
        var localNetID = GetNetID(AIOverride);
        if (localNetID != remoteNetID)
        {
            AIOverride = GetNewInstanceOrNullFromNetID(remoteNetID, npc);
            AIOverride?.SetDefaults(Mod);
        }

        AIOverride?.ReceiveExtraAI(bitReader, binaryReader);
    }

    #endregion
}