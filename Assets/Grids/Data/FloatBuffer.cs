// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public class FloatBuffer : IDataBuffer {

        private readonly float[] buffer;
        private int lastModified;

        public FloatBuffer(int length) {
            this.buffer = new float[length];
        }

        public BufferType Type {
            get { return BufferType.Float; }
        }

        public int Length {
            get { return buffer.Length; }
        }

        public int LastModified {
            get { return lastModified; }
        }

        public float[] GetRawBuffer() {
            return buffer;
        }

        public float GetValue(int index) {
            return buffer[index];
        }

        public void SetValue(float value, int index) {
            buffer[index] = value;
        }

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }

        public void SetValue(int index, object value) {
            if (value is float) {
                buffer[index] = (float)value;
            } else {
                throw new System.ArgumentException(string.Format("Tried to pass a value that is of type {0} instead of type float.", value.GetType()), "value");
            }
        }

        object IDataBuffer.GetValue(int index) {
            return buffer[index];
        }

    }
}
