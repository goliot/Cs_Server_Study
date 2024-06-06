using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    //Recv, Send 담당
    abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint); //연결 성공 이후
        public abstract void OnRecv(ArraySegment<byte> buffer); //수신 성공 이후
        public abstract void OnSend(int numOfBytes); //송신 성공 이후
        public abstract void OnDisConnected(EndPoint endPoint); //연결 끊긴 이후

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024); //버퍼, 시작점, 사이즈

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock) //Send가 여러 쓰레드에서 호출되므로 락 필수
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0) //1빠로 Send요청을하여 등록도 안된 경우
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisConnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterSend() //얘는 이미 락이 걸려있는 부분에서 호출한게 아니면 호출될 여지가 없음 
        {
            while(_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock) //얘는 콜백에 의해 예상치 못하게 수행 될 여지가 있음
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null; //초기화
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if(_sendQueue.Count > 0) //보내는 동안 또군가 전송 요청을 한 경우
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false) //바로 성공
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            //0개의 바이트가 오거나 Success가 아닌 경우, 모두 실패한 경우임
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    RegisterRecv();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
