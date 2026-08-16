using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Content.Reworks.Brain
{
    public class BrainOfCthulhuGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        /// <summary>
        /// Flag used during Rev+ Brain of Cthulhu to denote projectiles that were spawned prior to its Illusion Trick attack starting.
        /// </summary>
        public bool IgnoreBoCIllusions = false;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_Parent { Entity: Projectile parent })
            {
                if (parent.GetGlobalProjectile<BrainOfCthulhuGlobalProjectile>().IgnoreBoCIllusions)
                    IgnoreBoCIllusions = true;
            }
        }
    }
}
