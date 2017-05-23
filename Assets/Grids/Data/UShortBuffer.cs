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
    }
}
