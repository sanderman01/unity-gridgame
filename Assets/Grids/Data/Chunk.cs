// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public class Chunk {

        private readonly IDataBuffer[] buffers;

        // Create a square chunk with the specified buffer length and buffer layers.
        public Chunk(int bufferSize, LayerConfig layerConfig) {

            // create a buffer for each data layer
            this.buffers = new IDataBuffer[layerConfig.Count];

            for (int i = 0; i < layerConfig.Count; ++i) {
                // Create buffer based on type enum value
                BufferType bufferType = layerConfig.GetLayer(i).bufferType;
                this.buffers[i] = CreateBuffer(bufferType, bufferSize);
            }
        }

        private IDataBuffer CreateBuffer(BufferType type, int length) {
            IDataBuffer buffer;
            switch (type) {
                case BufferType.Boolean:
                    buffer = new BooleanBuffer(length);
                    break;
                case BufferType.Byte:
                    buffer = new ByteBuffer(length);
                    break;
                case BufferType.UShort:
                    buffer = new UShortBuffer(length);
                    break;
                default:
                    throw new System.NotImplementedException(string.Format("No buffer implemented for BufferType={0}", type));
            }
            return buffer;
        }

        public IDataBuffer GetBuffer(int layerIndex) {
            return this.buffers[layerIndex];
        }
    }

}
