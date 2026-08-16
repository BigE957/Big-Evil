using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BigEvil.Common.Netcode
{
    public class BigEvilNet : ModSystem
    {
        private static readonly List<Packet> instances = [];
        private static readonly Dictionary<Type, byte> typeToId = [];

        public override void PostSetupContent()
        {
            var packets = Mod.GetContent<Packet>();

            foreach (var p in packets)
            {
                p.MessageType = instances.Count;
                typeToId[p.GetType()] = (byte)instances.Count;
                instances.Add(p);
            }
        }

        public override void Unload()
        {
            instances.Clear();
            typeToId.Clear();
        }

        public static void HandlePacket(BinaryReader bb, int sender)
        {
            byte msg = bb.ReadByte();

            BigEvilMod.Instance.Logger.Info($"[AANet] Received msg id {msg} from {sender}");

            if (msg >= instances.Count)
            {
                BigEvilMod.Instance.Logger.Warn("Recieved packet with an invalid msg id of " + msg);
                return;
            }

            try
            {
                instances[msg].HandlePacket(bb, sender);
            }
            catch (Exception e)
            {
                string mode = Main.netMode == NetmodeID.Server ? "server" : "client";
                BigEvilMod.Instance.Logger.Error($"{mode} Error handling packet ({msg}) on {mode}: {e}");
                BigEvilMod.Instance.Logger.Info(e.StackTrace);
            }
        }

        public static void SendNetMessage<T>(params object[] param) where T : Packet
        {
            SendNetMessageClient<T>(-1, param);
        }

        public static void SendNetMessageClient<T>(int client, params object[] param) where T : Packet
        {
            if (!typeToId.TryGetValue(typeof(T), out byte p))
            {
                BigEvilMod.Instance.Logger.Warn($"No packet ID registered for {typeof(T).Name}");
                return;
            }

            try
            {
                instances[p].Send(client, param);
            }
            catch (Exception e)
            {
                string mode = Main.netMode == NetmodeID.Server ? "server" : "client";
                BigEvilMod.Instance.Logger.Error($"{mode} Error sending packet on {mode}: {e.Message}");
                BigEvilMod.Instance.Logger.Info(e.StackTrace);
            }
        }

    }

    public abstract class Packet : ILoadable
    {
        public virtual void Load(Mod mod) { }

        public virtual void Unload() { }

        public int MessageType = -1;

        public abstract void HandlePacket(BinaryReader reader, int sender);

        // The "Internal" write logic
        protected abstract void Write(BinaryWriter writer, object[] args);

        // The clean helper for the caller
        public void Send(int toClient = -1, params object[] args)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                return;

            ModPacket packet = BigEvilMod.Instance.GetPacket();
            packet.Write((byte)MessageType);
            Write(packet, args);
            packet.Send(toClient);
        }
    }

}
