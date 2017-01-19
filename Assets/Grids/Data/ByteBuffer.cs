// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;

namespace AmarokGames.Grids.Data {

    public class ByteBuffer : IDataBuffer {

        private readonly byte[] buffer;

        public ByteBuffer(int length) {
            this.buffer = new byte[length];
        }

        public BufferType Type {
            get { return BufferType.Byte; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public byte[] GetRawBuffer() {
            return buffer;
        }

        public byte GetValue(int index) {
            return buffer[index];
        }

        public void SetValue(byte value, int index) {
            buffer[index] = value;
        }
    }
}
