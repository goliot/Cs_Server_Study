using Server;
using ServerCore;

class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null)
            return;

        //잡큐에 broadcast 작업 넣기
        GameRoom room = clientSession.Room;
        room.Push(() => room.Broadcast(clientSession, chatPacket.chat));
    }
}