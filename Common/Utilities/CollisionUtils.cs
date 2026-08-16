using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Common.Utilities;

public static class CollisionUtils
{
    public static Vector2? RayCast(Vector2 startPosition, Vector2 rayDirection, float maxDist, out float distanceMoved, float step = 1f)
    {
        distanceMoved = 0f;

        Vector2 unitVect = rayDirection;
        if (unitVect != Vector2.Zero)
            unitVect.Normalize();
        else
        {
            distanceMoved = maxDist;
            return null;
        }

        float dirX = unitVect.X;
        float dirY = unitVect.Y;

        float tDeltaX = (Math.Abs(dirX) > 1e-12f) ? 16f / Math.Abs(dirX) : float.MaxValue;
        float tDeltaY = (Math.Abs(dirY) > 1e-12f) ? 16f / Math.Abs(dirY) : float.MaxValue;

        int cellX = (int)Math.Floor(startPosition.X / 16f);
        int cellY = (int)Math.Floor(startPosition.Y / 16f);

        int stepX = (dirX > 0) ? 1 : (dirX < 0 ? -1 : 0);
        int stepY = (dirY > 0) ? 1 : (dirY < 0 ? -1 : 0);

        float boundaryX = (dirX > 0) ? (cellX + 1) * 16f : cellX * 16f;
        float boundaryY = (dirY > 0) ? (cellY + 1) * 16f : cellY * 16f;

        float tMaxX = (dirX != 0) ? (boundaryX - startPosition.X) / dirX : float.MaxValue;
        float tMaxY = (dirY != 0) ? (boundaryY - startPosition.Y) / dirY : float.MaxValue;

        float currentT = 0f;

        float globalT = step;

        while (currentT < maxDist)
        {
            float nextT = Math.Min(tMaxX, tMaxY);

            if (WorldGen.InWorld(cellX, cellY))
            {
                Tile tile = Main.tile[cellX, cellY];

                if (tile.IsTileSolid())
                {
                    while (globalT <= currentT)
                        globalT += step;

                    float maxLocalT = Math.Min(nextT, maxDist);

                    while (globalT <= maxLocalT)
                    {
                        Vector2 currentPos = startPosition + unitVect * globalT;

                        if (tile.Slope == SlopeType.Solid && !tile.IsHalfBlock)
                        {
                            distanceMoved = globalT;
                            return currentPos;
                        }

                        Vector2 currentPosInTile = currentPos - new Vector2(cellX * 16f, cellY * 16f);
                        bool hit = false;

                        if (tile.IsHalfBlock && currentPosInTile.Y >= 8f) hit = true;
                        else if (tile.Slope == SlopeType.SlopeDownLeft && currentPosInTile.X <= currentPosInTile.Y) hit = true;
                        else if (tile.Slope == SlopeType.SlopeDownRight && (16f - currentPosInTile.X) <= currentPosInTile.Y) hit = true;
                        else if (tile.Slope == SlopeType.SlopeUpLeft && currentPosInTile.X <= (16f - currentPosInTile.Y)) hit = true;
                        else if (tile.Slope == SlopeType.SlopeUpRight && currentPosInTile.X >= currentPosInTile.Y) hit = true;

                        if (hit)
                        {
                            distanceMoved = globalT;
                            return currentPos;
                        }

                        globalT += step;
                    }
                }
            }

            if (tMaxX < tMaxY)
            {
                cellX += stepX;
                currentT = tMaxX;
                tMaxX += tDeltaX;
            }
            else
            {
                cellY += stepY;
                currentT = tMaxY;
                tMaxY += tDeltaY;
            }
        }

        distanceMoved = maxDist;
        return null;
    }

    public static Point FindSurfaceBelow(Point p, bool ignorePlatforms = false)
    {

        if (SurfaceTile(p))
            while (SurfaceTile(p.X, p.Y - 1) && p.Y >= 1)
                p.Y--;
        else
        {

            while (!SurfaceTile(p.X, p.Y + 1) && (ignorePlatforms || !TileID.Sets.Platforms[Framing.GetTileSafely(p.X, p.Y).TileType]) && p.Y < Main.maxTilesY)
                p.Y++;
        }

        return p;
    }

    public static Point FindSurfaceAround(Point p, bool ignorePlatforms = false)
    {
        Point newPoint = p;

        if (SurfaceTile(newPoint))
        {
            while (newPoint.Y >= 1)
            {
                if (!SurfaceTile(newPoint.X, newPoint.Y - 1) && (ignorePlatforms || !TileID.Sets.Platforms[Framing.GetTileSafely(newPoint.X, newPoint.Y).TileType]))
                    return newPoint;

                newPoint.Y--;
            }
        }
        else
        {
            Point altPoint = p;
            while (newPoint.Y < Main.maxTilesY && altPoint.Y >= 0)
            {
                if (SurfaceTile(newPoint.X, newPoint.Y + 1) || (!ignorePlatforms && TileID.Sets.Platforms[Framing.GetTileSafely(newPoint.X, newPoint.Y + 1).TileType]))
                    return newPoint + new Point(0, 1);

                if (SurfaceTile(altPoint.X, altPoint.Y + 1) || (!ignorePlatforms && TileID.Sets.Platforms[Framing.GetTileSafely(altPoint.X, altPoint.Y + 1).TileType]))
                    return altPoint + new Point(0, 1);

                newPoint.Y++;
                altPoint.Y--;
            }
        }

        return p;
    }

    public static bool SurfaceTile(Point p) => SurfaceTile(p.X, p.Y);
    public static bool SurfaceTile(int x, int y)
    {
        Tile t = Framing.GetTileSafely(x, y);

        if (t == null)
            return false;

        if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType] && !t.IsActuated && !TileLoader.IsClosedDoor(t))
            return true;

        return false;
    }

    public static bool SurfaceCollision(Vector2 Position, int Width, int Height, bool ignorePlatforms = false)
    {
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Vector2 vector = default(Vector2);
        for (int i = num; i < value2; i++)
        {
            for (int j = value3; j < value4; j++)
            {
                if (Main.tile[i, j] != null && Main.tile[i, j].HasUnactuatedTile)
                {
                    int type = Main.tile[i, j].TileType;
                    bool isStandardSolid = Main.tileSolid[type] && !Main.tileSolidTop[type];
                    bool isPlatform = !ignorePlatforms && TileID.Sets.Platforms[type];

                    if (isStandardSolid || isPlatform)
                    {
                        vector.X = i * 16;
                        vector.Y = j * 16;
                        int num2 = 16;

                        if (Main.tile[i, j].IsHalfBlock)
                        {
                            vector.Y += 8f;
                            num2 -= 8;
                        }

                        if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}
