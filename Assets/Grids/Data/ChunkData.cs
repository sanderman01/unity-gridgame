// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public class ChunkData {

        private readonly IDataBuffer[] buffers;
        
        public int LastModified { get; private set; }

        // Create a square chunk with the specified buffer length and buffer layers.
        public ChunkData(int bufferSize, LayerConfig layerConfig) {

            // create a buffer for each data layer
            this.buffers = new IDataBuffer[layerConfig.Count];

            for (int i = 0; i < layerConfig.Count; ++i) {
                // Create buffer based on type enum value
                BufferType bufferType = layerConfig[new LayerId(i)].bufferType;
                this.buffers[i] = CreateBuffer(bufferType, bufferSize);
            }
        }

        private IDataBuffer CreateBuffer(BufferType type, int length) {
            IDataBuffer buffer;
            switch (type) {
                case BufferType.Boolean:
                    buffer = new BitBuffer(length);
                    break;
                case BufferType.Byte:
                    buffer = new ByteBuffer(length);
                    break;
                case BufferType.UShort:
                    buffer = new UShortBuffer(length);
                    break;
                case BufferType.UnsignedInt32:
                    buffer = new BufferUnsignedInt32(length);
                    break;
                case BufferType.Float:
                    buffer = new FloatBuffer(length);
                    break;
                default:
                    throw new System.NotImplementedException(string.Format("No buffer implemented for BufferType={0}", type));
            }
            return buffer;
        }

        public IDataBuffer GetBuffer(LayerId layerIndex) {
            return this.buffers[layerIndex.id];
        }

        public void MarkModified(int layerIndex, int frameCount) {
            LastModified = frameCount;
            buffers[layerIndex].MarkModified(frameCount);
        }
    }

}
