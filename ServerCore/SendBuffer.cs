namespace ServerCore
{
    public class SendBufferHelper
    {
        //각 쓰레드별로 SendBuffer를 따로 두어 경합 방지
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null) //새삥
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize) //여유 공간 부족
                CurrentBuffer.Value = new SendBuffer(ChunkSize); //새로 하나 판다

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer //여기에서는 읽기만 하므로 락이 필요 없음
    {
        byte[] _buffer; //큰 덩어리
        int _usedSize = 0; //잘려나간 크기(writePos의 역할)

        public int FreeSize { get { return _buffer.Length - _usedSize; } } //남은 공간

        public SendBuffer(int chunkSize) //덩어리 생성자
        {
            _buffer = new byte[chunkSize];
        }
        
        public ArraySegment<byte> Open(int reserveSize/*요청크기*/)
        { //작업 공간 할당
            if (reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize/*실제사용한크기*/)
        { // 작업 공간 반환
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }
    }
}