using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace BigEvil.Common.Utilities
{
    public static class TileUtils
    {
        /// <summary>
        /// Determines if a tile is solid based on whether it's active and not actuated or if the tile is solid. This will not count platforms and other non-solid ground tiles
        /// </summary>
        /// <param name="tile">The tile to check.</param>
        public static bool IsTileSolid(this Tile tile) => tile != null && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
    }
}
