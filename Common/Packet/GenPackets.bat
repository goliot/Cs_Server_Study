START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../Server/Packet"
XCOPY /Y ServerPacketManager.cs "../../Server/Packet"