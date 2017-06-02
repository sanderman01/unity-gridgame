// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;

namespace AmarokGames.Grids.Data {

    public class ByteBuffer : IDataBuffer {

        private readonly byte[] buffer;
        private int lastModified;

        public ByteBuffer(int length) {
            this.buffer = new byte[length];
        }

        public BufferType Type {
            get { return BufferType.Byte; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int LastModified {
            get { return lastModified; }
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

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }

        public void SetValue(int index, object value) {
            if (value is byte) {
                buffer[index] = (byte)value;
            } else {
                throw new System.ArgumentException(string.Format("Tried to pass a value that is of type {0} instead of type byte.", value.GetType()), "value");
            }
        }

        object IDataBuffer.GetValue(int index) {
            return buffer[index];
        }
    }
}
