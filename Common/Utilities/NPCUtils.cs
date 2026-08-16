using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Common.Utilities
{
    public static class NPCUtils
    {
        /// <summary>
        /// Clones the given NPC's loot except anything input and adds it to the given loot pool.
        /// <br/>
        /// Thank you to Boatsoon for making this awesomesauce method
        /// </summary>
        /// <param name="npcToClone">The ID of the npc whose loot is to be cloned.</param>
        /// <param name="itemIdsToExclude">The items present in the former NPC's lootpool you do not wish to clone.</param>
        /// <param name="leadingCondition">The loading condition rule to apply to all cloned loot.</param>
        /// <param name="loot">The loot pool you wish to add the loot to.</param>
        public static void CloneDropsWithoutInput(int npcToClone, int[] itemIdsToExclude, LeadingConditionRule leadingCondition, ref NPCLoot loot)
        {
            List<IItemDropRule> clonedDropRules = Main.ItemDropsDB.GetRulesForNPCID(npcToClone, false);

            foreach (IItemDropRule rule in clonedDropRules)
            {
                int itemID = 0;

                if (rule is ItemDropWithConditionRule conditionDrop)
                {
                    itemID = conditionDrop.itemId;
                }
                else if (rule is CommonDrop commonDrop)
                {
                    itemID = commonDrop.itemId;
                }

                if (itemIdsToExclude.Contains(itemID))
                {
                    continue;
                }

                leadingCondition.OnSuccess(rule);
            }

            loot.Add(leadingCondition);
        }

        /// <summary>
        /// Spawn Boss Method for Using Spawn Items
        /// <para>NOTE: This method use vanilla's spawn position behaviour!</para>
        /// </summary>
        /// <param name="player">Player who used Item</param>
        /// <param name="npcType">Boss's NPCType to spawn</param>
        /// <param name="spawnSound">Sound to play when spawn, it play on used player's position</param>
        public static void SpawnBossUsingItem(Player player, int npcType, in SoundStyle? spawnSound = null)
        {
            SoundEngine.PlaySound(spawnSound, player.Center);

            if (player.whoAmI != Main.myPlayer)
                return;

            // NOTE: MP netcode can be simplified by directly spawn npc like SpawnBossOnPosUsingItem does
            // but leaving this as vanilla's standard now
            switch (Main.netMode)
            {
                // SP: Spawn Boss Immediately
                case NetmodeID.SinglePlayer:
                    NPC.SpawnOnPlayer(player.whoAmI, npcType);
                    break;

                // MP: Ask server to spawn one
                case NetmodeID.MultiplayerClient:
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, npcType);
                    break;
            }
        }

        public struct TargetingParameters : IEquatable<TargetingParameters>
        {
            public enum NPCTargetType
            {
                Anyone = 0,
                PreferSame = 1,
                ForceSwitch = 2,
            }

            // Vanilla argument to TargetClosest. Defaults to true as it does in vanilla.
            // If true, the NPC will turn to face the target.
            public bool faceTarget = true;

            // Vanilla argument to TargetClosestUpgraded. That function is never used, but the flexibility is implemented here.
            // Allows targeting distance calculations to be measured from a different position than the NPC's center, if desired.
            public Vector2? targetingCenter = null;

            // Hard maximum range to search for targets. Players outside this physical Manhattan distance will always be ignored.
            //
            // Vanilla always uses infinity for this, leading to behavior like Queen Bee crossing the world to spawncamp you.
            // Calamity defaults to a very high but not infinite value.
            public float maxSearchRange = 9600f; // 600 tiles

            // Targeting preference enum.
            // Anyone = Target the "closest" player, no other considerations. Vanilla behavior.
            // PreferSame = Always pick the same player if they're within the search range, even if another player is closer or has more aggro.
            // ForceSwitch = Try to pick any other player but the current player, if possible. Similar to an "aggro drop" in MMOs.
            //
            // ForceSwitch intentionally does nothing in single player, because there is nobody to switch to.
            public NPCTargetType targetType = NPCTargetType.Anyone;

            // The ratio at which to consider aggro bonuses from player gear.
            // 1f is vanilla. Set to 0f to ignore aggro bonuses entirely.
            // Set to a negative value to make the NPC intentionally avoid tanks and preferentially go after other players.
            public float aggroRatio = 1f;

            // Whether or not players must have line of sight to the NPC to be considered valid targets.
            // This is always line of sight to the NPC itself, even if a different targeting center for range finding is specified.
            public bool requireLineOfSight = false;

            // If true, the targeting algorithm counts missing health as a gigantic boost to aggro.
            // This makes for a "merciless" or "bloodthirsty" NPC which is focused on killing the lowest health players.
            public bool finishThemOff = false;
            internal const float FinishThemOff_MaxAggroBoost = 4000f;

            // If true, this NPC ignores the Stardust armor set bonus "JoJo Tank Minion" (or "Algalon the Observer" according to the wiki).
            //
            // This is set to false by default, because that's vanilla behavior.
            // As Stardust armor is postgame in vanilla, no vanilla bosses ignore the Stardust Guardian.
            // It is highly recommended to set this to true for all bosses, or their aggro can be abusively manipulated.
            public bool ignoreTankMinions = false;

            // If true, this NPC ignores players who have less than zero net aggro and are not actively using items.
            //
            // This is set to true by default, because it's (undocumented) vanilla behavior.
            // Bosses will automatically attack stealthed players anyway -- you don't need to set this to false for that to occur.
            public bool ignoreStealthedPlayers = true;

            // If true, this targeting change forces a net update.
            // In vanilla, targeting updates cause net updates if direction changed or the target player changed,
            // but NEVER if the NPC has collideX or collideY set to true.
            //
            // Generally this doesn't need to be set to true, as bosses will never have collideX or collideY set to true.
            public bool forceNetUpdate = false;

            // Player indexes put into this list will not be considered for targetting.
            public HashSet<int> excludedPlayers = [];

            public TargetingParameters() { }

            // Quick defaults for recommended boss settings.
            public TargetingParameters(bool isBoss) => ignoreTankMinions = isBoss;

            // Using the default keyword on structs produces garbage. Please use the below instead, or define your own parameters.
            public static TargetingParameters Defaults => new();
            public static TargetingParameters BossDefaults => new(true);

            #region Equality Operators
            public static bool operator ==(TargetingParameters ctp1, TargetingParameters ctp2)
            {
                bool targetingCentersEqual = (ctp1.targetingCenter is null && ctp2.targetingCenter is null) || ctp1.targetingCenter == ctp2.targetingCenter;
                if (!targetingCentersEqual)
                    return false;

                return ctp1.faceTarget == ctp2.faceTarget &&
                    ctp1.maxSearchRange == ctp2.maxSearchRange &&
                    ctp1.targetType == ctp2.targetType &&
                    ctp1.aggroRatio == ctp2.aggroRatio &&
                    ctp1.requireLineOfSight == ctp2.requireLineOfSight &&
                    ctp1.finishThemOff == ctp2.finishThemOff &&
                    ctp1.ignoreTankMinions == ctp2.ignoreTankMinions &&
                    ctp1.ignoreStealthedPlayers == ctp2.ignoreStealthedPlayers &&
                    ctp1.forceNetUpdate == ctp2.forceNetUpdate;
            }

            public static bool operator !=(TargetingParameters ctp1, TargetingParameters ctp2) => !(ctp1 == ctp2);

            public readonly bool Equals(TargetingParameters other) => this == other;

            public override readonly bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj is not TargetingParameters)
                    return false;

                return this == (TargetingParameters)obj;
            }

            // Visual Studio complains if this is not here. I do not know why.
            public override readonly int GetHashCode() => base.GetHashCode();
            #endregion
        }

        /// <summary>
        /// Replacement and extension for vanilla's NPC.TargetClosest. Has very flexible behavior.<br />
        /// Like vanilla's function, this function does not return any value, but makes its changes in-place.
        /// </summary>
        /// <param name="options">Struct to specify all options. Refer to struct definition in NPCUtils for details.</param>
        /// <returns>The targeted player ID.</returns>
        public static int EnhancedTargeting(this NPC npc, TargetingParameters options)
        {
            // 05JUN2024: Ozzatron: Struct defaults are always memset to all-zero, giving garbage parameters.
            // If you actually call this function with the default keyword for the struct,
            // change the options on the spot to valid default / intended default parameters.
            if (options == default)
                options = new TargetingParameters();

            float distance = 0f;
            // float realDist = 0f; // Defined but not used by vanilla. Commented out here.
            bool anyTargetAvailable = false;
            int tankMinionProjectileID = -1;

            // The setup and initial loop is equivalent to vanilla NPC.TargetClosest, but optimized.
            foreach (Player p in Main.ActivePlayers)
            {
                bool playerDead = p.dead || p.ghost;
                if (playerDead)
                    continue;

                // ForceSwitch targeting. If the same player from last time is iterated over, just ignore them.
                // Player exclusion. If the player is to be excluded, do not consider them.
                bool sameTargetAsLastTime = p.whoAmI == npc.oldTarget;
                bool notSinglePlayer = Main.netMode != NetmodeID.SinglePlayer;
                if (notSinglePlayer && (options.excludedPlayers.Contains(p.whoAmI) || (options.targetType == TargetingParameters.NPCTargetType.ForceSwitch && sameTargetAsLastTime)))
                    continue;

                //
                // The below code is implemented in vanilla as a separate method. Here, it's inlined for efficiency.
                //

                Vector2 pCenter = p.Center;
                Vector2 targetCenter = options.targetingCenter ?? npc.Center;
                float manhattanDist = Math.Abs(targetCenter.X - pCenter.X) + Math.Abs(targetCenter.Y - pCenter.Y);

                // Hard cutoff range specified in options. If the player is further, completely ignore them.
                if (manhattanDist > options.maxSearchRange)
                    continue;

                // Line of sight requirement specified in options. Please don't use this without reducing the max search range.
                if (options.requireLineOfSight && !Collision.CanHit(npc.Center, 1, 1, pCenter, 1, 1))
                    continue;

                float aggroAdjustedDist = manhattanDist - options.aggroRatio * p.aggro;

                // Implementation of "Finish Them Off": Add enormous amounts of virtual aggro to low health players
                if (options.finishThemOff)
                {
                    float missingHPRatio = MathHelper.Clamp(1f - p.statLife / (float)p.statLifeMax2, 0f, 1f);
                    float bloodthirstAggro = MathHelper.Lerp(0f, TargetingParameters.FinishThemOff_MaxAggroBoost, missingHPRatio);
                    aggroAdjustedDist -= bloodthirstAggro;
                }

                bool aggroDisabled = p.npcTypeNoAggro[npc.type];
                if (aggroDisabled && npc.direction != 0)
                    aggroAdjustedDist += 1000f;

                bool cancelTargeting = false;

                // PreferSame targeting. If the same player from last time is a valid target, even if not the "best" target, pick it anyway.
                bool preferSameFound = options.targetType == TargetingParameters.NPCTargetType.PreferSame && sameTargetAsLastTime;

                // Standard targeting. If the adjusted distance is lower, or this is the first valid target, actually choose the new target.
                bool standardTargetingRequirementsMet = !anyTargetAvailable || aggroAdjustedDist < distance;

                // If either targeting method succeeded, then this target is being engaged.
                bool engageThisTarget = preferSameFound || standardTargetingRequirementsMet;
                if (engageThisTarget)
                {
                    anyTargetAvailable = true;
                    tankMinionProjectileID = -1; // Reset any Stardust Guardian aggro because a real player was found.
                    distance = aggroAdjustedDist;
                    npc.target = p.whoAmI;

                    // If PreferSame targeting is active, and the same player was found, cancel further iteration. They are being chosen above all others.
                    if (preferSameFound)
                        cancelTargeting = true;
                }

                // "Tank pet" accomodation, AKA the 1.4+ Stardust Guardian
                // Basically, if the player would be targeted, give a chance to instead target their tank minion
                //
                // This behavior is not documented on the vanilla wiki.
                if (p.tankPet >= 0 && !aggroDisabled && !options.ignoreTankMinions)
                {
                    Projectile tankMinion = Main.projectile[p.tankPet];
                    Vector2 tmCenter = tankMinion.Center;
                    float manhattanDistToTankMinion = Math.Abs(targetCenter.X - tmCenter.X) + Math.Abs(targetCenter.Y - tmCenter.Y);

                    // The Stardust Guardian is considered to have a 200 aggro bonus by default.
                    // In Calamity this is scaled by the aggro ratio specified in options.
                    manhattanDistToTankMinion -= options.aggroRatio * 200f;

                    // The Stardust Guardian only attracts the attention of NPCs within a very short distance
                    if (manhattanDistToTankMinion < distance && manhattanDistToTankMinion < 200f && Collision.CanHit(npc.Center, 1, 1, tmCenter, 1, 1))
                        tankMinionProjectileID = p.tankPet;
                }

                // If targeting has been short-circuited for any reason, cancel iteration over players.
                if (cancelTargeting)
                    break;
            }

            // If the NPC has been aggroed by a Stardust Guardian instead of an actual player, account for that
            if (tankMinionProjectileID >= 0)
            {
                Projectile tankMinion = Main.projectile[tankMinionProjectileID];
                npc.targetRect = tankMinion.Hitbox;

                // Always set direction to a nonzero value. This NPC has been engaged in combat.
                npc.direction = 1;
                if (tankMinion.Center.X < npc.Center.X)
                    npc.direction = -1;

                npc.directionY = 1;
                if (tankMinion.Center.Y < npc.Center.Y)
                    npc.directionY = -1;
            }

            // Standard player aggro occurs here
            else
            {
                bool shouldFaceTarget = options.faceTarget;

                // Sanitize targeted player index
                if (npc.target < 0 || npc.target >= Main.maxPlayers)
                    npc.target = 0;

                Player targetPlayer = Main.player[npc.target];
                npc.targetRect = targetPlayer.Hitbox;

                // Do not switch facing to look at dead players.
                if (targetPlayer.dead)
                    shouldFaceTarget = false;

                // If already engaged in combat, do not switch facing to look at players that ignore your aggro.
                if (targetPlayer.npcTypeNoAggro[npc.type] && npc.direction != 0)
                    shouldFaceTarget = false;

                if (shouldFaceTarget)
                {
                    bool oldTargetWasValid = npc.oldTarget >= 0 && npc.oldTarget < Main.maxPlayers;

                    bool targetIsLowAggroNotUsingItem = targetPlayer.itemAnimation == 0 && targetPlayer.aggro < 0;
                    bool willIgnoreStealthedPlayers = !npc.boss && options.ignoreStealthedPlayers;

                    // Regular NPCs (not bosses) will voluntarily ignore otherwise-valid player targets with less than zero aggro if they are not actively using an item.
                    // This ONLY WORKS if they already have another valid target, aka multiplayer.
                    // As such, having net less than zero aggro enables you to remain "stealthed" to regular enemies if you are not doing anything.
                    // This is undocumented vanilla behavior.
                    bool ignoreStealthedPlayer = willIgnoreStealthedPlayers && oldTargetWasValid && targetIsLowAggroNotUsingItem;
                    if (!ignoreStealthedPlayer)
                    {
                        // Always set direction to a nonzero value. This NPC has been engaged in combat.
                        npc.direction = 1;
                        if (targetPlayer.Center.X < npc.Center.X)
                            npc.direction = -1;

                        npc.directionY = 1;
                        if (targetPlayer.Center.Y < npc.Center.Y)
                            npc.directionY = -1;
                    }
                }
            }

            // Confused enemies always run in the exact wrong direction, horizontally at least.
            if (npc.confused)
                npc.direction *= -1;

            // Apply net updates.
            bool directionChange = npc.direction != npc.oldDirection || npc.directionY != npc.oldDirectionY;
            bool targetChange = npc.target != npc.oldTarget;
            bool shouldNetUpdate = (directionChange || targetChange) && !npc.collideX && !npc.collideY;
            if (shouldNetUpdate || options.forceNetUpdate)
                npc.netUpdate = true;

            return npc.target;
        }

        /// <summary>
        /// Detects nearby hostile NPCs from a given point
        /// </summary>
        /// <param name="origin">The position where we wish to check for nearby NPCs</param>
        /// <param name="maxDistanceToCheck">Maximum amount of pixels to check around the origin</param>
        /// <param name="ignoreTiles">Whether to ignore tiles when finding a target or not</param>
        /// <param name="bossPriority">Whether bosses should be prioritized in targetting or not</param>
        public static NPC ClosestNPCAt(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
        {
            NPC closestTarget = null;
            float distance = maxDistanceToCheck;
            if (bossPriority)
            {
                bool bossFound = false;
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    // If we've found a valid boss target, ignore ALL targets which aren't bosses.
                    if (bossFound && !(Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye))
                        continue;

                    if (Main.npc[index].CanBeChasedBy(null, false))
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            if (Main.npc[index].boss || Main.npc[index].type == NPCID.WallofFleshEye)
                                bossFound = true;

                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            else
            {
                for (int index = 0; index < Main.npc.Length; index++)
                {
                    if (Main.npc[index].CanBeChasedBy(null, false))
                    {
                        float extraDistance = (Main.npc[index].width / 2) + (Main.npc[index].height / 2);

                        bool canHit = true;
                        if (extraDistance < distance && !ignoreTiles)
                            canHit = Collision.CanHit(origin, 1, 1, Main.npc[index].Center, 1, 1);

                        if (Vector2.Distance(origin, Main.npc[index].Center) < distance && canHit)
                        {
                            distance = Vector2.Distance(origin, Main.npc[index].Center);
                            closestTarget = Main.npc[index];
                        }
                    }
                }
            }
            return closestTarget;
        }
    }
}
