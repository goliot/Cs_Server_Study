using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //DNS 사용
            string host = Dns.GetHostName(); //내 로컬 컴퓨터의 호스트 이름
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777); //주소, 포트번호

            while (true)
            {
                //손님 휴대폰 설정
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    //문지기한테 입장 문의
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    //보낸다
                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello world! {i}");
                        int sendBytes = socket.Send(sendBuff);
                    }

                    //받는다
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    //나간다
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}