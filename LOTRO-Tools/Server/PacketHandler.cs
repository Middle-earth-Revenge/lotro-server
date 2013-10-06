using Helper;
using System;
using System.IO;
using System.Text;

namespace Server
{
    class PacketHandler
    {
        public void handleIncommingPacket(SocketObject socketObject)
        {
            // First of all: create the reader for the incoming packet
            MemoryStream memoryStreamInput = new MemoryStream(socketObject.Buffer, 0, socketObject.Length);
            BEBinaryReader beBinaryReader = new BEBinaryReader(memoryStreamInput, System.Text.Encoding.UTF8);

            UInt16 sessionID = beBinaryReader.ReadUInt16BE();
            beBinaryReader.BaseStream.Position = 0;

            if (sessionID == 0x0000) // Session setup payload
            {
                // Skip two bytes
                beBinaryReader.ReadUInt16BE();

                //Helper.HelperMethods.Instance.writeLog(Settings.Config.Instance.LogFolder + "\\" + Settings.Config.Instance.ServerLogFolder, "received-" + Server.UdpServer.Instance.packetNumberClient, socketObject.Buffer, socketObject.Length, true);
                SetupHandler.process(socketObject, beBinaryReader);
            }
            else // Handle everything else
            {
                SessionHandler.process(sessionID, socketObject, beBinaryReader);
            }          

            
        }


    }
}
