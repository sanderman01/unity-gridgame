// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public class UShortBuffer : IDataBuffer {

        private readonly ushort[] buffer;
        private int lastModified;

        public UShortBuffer(int length) {
            this.buffer = new ushort[length];
        }

        public BufferType Type {
            get { return BufferType.UShort; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int LastModified {
            get { return lastModified; }
        }

        public ushort[] GetRawBuffer() {
            return buffer;
        }

        public ushort GetValue(int index) {
            return buffer[index];
        }

        public void SetValue(ushort value, int index) {
            buffer[index] = value;
        }

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }

        public void SetValue(int index, object value) {
            if (value is ushort) {
                buffer[index] = (ushort)value;
            } else {
                throw new System.ArgumentException(string.Format("Tried to pass a value that is of type {0} instead of type ushort.", value.GetType()), "value");
            }
        }

        object IDataBuffer.GetValue(int index) {
            return buffer[index];
        }
    }
}
