// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System;
using System.Collections;

namespace AmarokGames.Grids.Data {

    public class BooleanBuffer : IDataBuffer {
        private readonly BitArray buffer;
        private int lastModified;

        public BooleanBuffer(int length) {
            this.buffer = new BitArray(length);
        }

        public BufferType Type {
            get { return BufferType.Boolean; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int LastModified {
            get { return lastModified; }
        }

        public BitArray GetRawBuffer() {
            return buffer;
        }

        public bool GetValue(int index) {
            return buffer[index];
        }

        public void SetValue(bool value, int index) {
            buffer[index] = value;
        }

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }

        public void SetValue(int index, object value) {
            if(value is bool) {
                buffer[index] = (bool)value;
            } else {
                throw new System.ArgumentException(string.Format("Tried to pass a value that is of type {0} instead of type bool.", value.GetType()), "value");
            }
        }

        object IDataBuffer.GetValue(int index) {
            return buffer[index];
        }
    }
}
