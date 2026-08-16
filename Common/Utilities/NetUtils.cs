using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace BigEvil.Common.Utilities
{
    public static class NetUtils
    {
        public static void WritePackedWorldPosition(this BinaryWriter writer, Vector2 worldPositionX16) => WritePackedWorldPosition(writer, (int)worldPositionX16.X, (int)worldPositionX16.Y);
        public static void WritePackedWorldPosition(this BinaryWriter writer, int worldX, int worldY)
        {
            int tileX = Math.DivRem(worldX, 16, out int remX);
            int tileY = Math.DivRem(worldY, 16, out int remY);
            byte remByte = (byte)(remX << 4 | remY);
            writer.Write((ushort)Math.Clamp(tileX, 0, ushort.MaxValue)); // If you actually have world size above 65535 tiles in axis, Good luck on that
            writer.Write((ushort)Math.Clamp(tileY, 0, ushort.MaxValue));
            writer.Write(remByte);
        }

        public static Vector2 ReadPackedWorldPosition(this BinaryReader reader)
        {
            reader.ReadPackedWorldPosition(out var worldX, out var worldY);
            return new Vector2(worldX, worldY);
        }

        public static void ReadPackedWorldPosition(this BinaryReader reader, out int worldX, out int worldY)
        {
            var tileX = (int)reader.ReadUInt16();
            var tileY = (int)reader.ReadUInt16();
            var remByte = reader.ReadByte();
            var remX = remByte >> 4;
            var remY = remByte & 0b1111;
            worldX = (tileX * 16) + remX;
            worldY = (tileY * 16) + remY;
        }
    }
}
