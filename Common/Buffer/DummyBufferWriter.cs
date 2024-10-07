using System.Buffers;

namespace Common.Buffer;

public class DummyBufferWriter : IBufferWriter<byte>
{
    public void Advance(int count)
    {
    }

    public Memory<byte> GetMemory(int sizeHint = 0) => throw new NotSupportedException();
    public Span<byte> GetSpan(int sizeHint = 0) => throw new NotSupportedException();
}