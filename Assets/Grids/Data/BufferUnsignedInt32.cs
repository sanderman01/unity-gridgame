using AmarokGames.Grids.Data;

public class BufferUnsignedInt32 : IDataBuffer {

    uint[] buffer;
    private int lastModified;

    public BufferUnsignedInt32(int length) {
        buffer = new uint[length];
    }

    public int LastModified {
        get {
            return lastModified;
        }
    }

    public int Length {
        get {
            return buffer.Length;
        }
    }

    public BufferType Type {
        get {
            return BufferType.UnsignedInt32;
        }
    }

    public object GetValue(int index) {
        return buffer[index];
    }

    public void MarkModified(int frameCount) {
        lastModified = frameCount;
    }

    public void SetValue(int index, object value) {
        if (value is uint) {
            buffer[index] = (uint)value;
        } else {
            throw new System.ArgumentException(string.Format("Tried to pass a value that is of type {0} instead of type uint.", value.GetType()), "value");
        }
    }
}
