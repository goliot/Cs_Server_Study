using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    //Accept 담당
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory; //세션을 만들어 주는 것

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory; //연결 수락시 실행할 콜백 함수

            //문지기 교육
            _listenSocket.Bind(endPoint);

            //영업 시작
            //backlog : 최대 대기 수
            _listenSocket.Listen(backlog);

            for(int i=0; i<register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs(); //한번 만들면 계속 재사용
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); //완료되면, OnAcceptCompleted 실행
                RegisterAccept(args);
            }
        }

        //비동기에선 Accept 요청과 완료가 분리되어야 함
        //Register 와 Completed가 뺑뺑이를 돌면서 계속 실행됨
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null; //재사용이므로 초기화 시키고 사용

            bool pending = _listenSocket.AcceptAsync(args); //비동기로 예약
            if(pending == false) //완료가 된 상태
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            RegisterAccept(args);
            //이번 꺼는 완료가 됐으므로, 다음 아이를 위해 재등록
        }
    }
}
