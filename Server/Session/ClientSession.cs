using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //Session의 사후 처리 담당
    class ClientSession : PacketSession //클라이언트의 대리인
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            //TODO
            Program.room.Enter(this);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisConnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);

            if(Room != null)
            {
                Room.Leave(this);
                Room = null;
            }

            Console.WriteLine($"OnDisConnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferrd bytes: {numOfBytes}");
        }
    }
}
