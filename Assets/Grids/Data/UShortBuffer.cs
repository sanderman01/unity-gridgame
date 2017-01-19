// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public class UShortBuffer : IDataBuffer {

        private readonly ushort[] buffer;

        public UShortBuffer(int length) {
            this.buffer = new ushort[length];
        }

        public BufferType Type {
            get { return BufferType.UShort; }
        }

        public int Length {
            get { return buffer.Length; }
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
    }
}
