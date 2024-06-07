namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;

        public RecvBuffer(int bufferSize) //생성자
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }
        
        public int DataSize { get { return _writePos - _readPos; } } //유효 버퍼
        public int FreeSize { get { return _buffer.Count - _writePos; } } //남은 버퍼

        public ArraySegment<byte> ReadSegment //현재까지 받은 데이터의 유효 범위(read용)
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        public ArraySegment<byte> WriteSegment //남은 버퍼 유효 범위(write용)
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean() //버퍼 앞으로 당겨서 공간 만들기
        {
            int dataSize = DataSize;
            if(dataSize == 0) // r == w
            {
                //남은 데이터가 없으니 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                //남은게 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes) //성공적 데이터 처리(read)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes) //성공적 recv(Write)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
