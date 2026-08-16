using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BigEvil.Common.Globals.AIOverride
{
    public abstract class VanillaAIOverride : ILoadable
    {
        public NPC NPC { get; set; }
        public bool DisableMultiplayerSmoothing { get; set; }
        public virtual bool EnableMultiplayerSmoothingAheadOfAI => false;

        void ILoadable.Load(Mod mod)
        {
            VanillaAIOverrideNPC.RegisterNetID(this);
            Load();
        }

        void ILoadable.Unload()
        {
            Unload();
        }

        public virtual void Load() { }

        public virtual void Unload() { }

        public abstract bool AI(Mod mod);

        public virtual void SetDefaults(Mod mod) { }

        public virtual void OnSpawn(Mod mod) { }

        public virtual void PostAI(Mod mod)
        {

        }

        public virtual void SendExtraAI(BitWriter bitWriter, BinaryWriter binaryWriter)
        {

        }

        public virtual void ReceiveExtraAI(BitReader bitReader, BinaryReader binaryReader)
        {

        }

        public virtual bool? CanBeHitByProjectile(Mod mod, Projectile projectile) => null;

        public virtual void ModifyHitByItem(Mod mod, Player player, Item item, ref NPC.HitModifiers modifiers) { }

        public virtual void ModifyHitByProjectile(Mod mod, Projectile projectile, ref NPC.HitModifiers modifiers) { }

        public virtual void OnHitByItem(Mod mod, Player player, Item item, NPC.HitInfo hit, int damageDone) { }

        public virtual void OnHitByProjectile(Mod mod, Projectile projectile, NPC.HitInfo hit, int damageDone) { }

        public virtual void HitEffect(Mod mod, NPC.HitInfo hit) { }

        public virtual bool PreKill(Mod mod) => true;

        public virtual void FindFrame(Mod mod, int frameHeight) { }

        public virtual bool PreDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => true;

        public virtual void PostDraw(Mod mod, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

        /// <summary>
        /// This Method should be Implemented If we added our custom field to AI Overrides
        /// </summary>
        /// <returns></returns>
        public virtual VanillaAIOverride Clone()
        {
            return (VanillaAIOverride)this.MemberwiseClone();
        }
    }
}