// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;

namespace AmarokGames.Grids.Data {

    /// <summary>
    /// Identifier used to access chunk datalayer buffers.
    /// For every layer added using LayerConfig.AddLayer, a LayerId is returned in an out parameter, 
    /// which can be used to access the layer buffer in the future.
    /// </summary>
    public struct LayerId {
        public readonly int id;

        /// <summary>
        /// Creates a LayerId with the specified id number. Should not be used directly in most cases.
        /// </summary>
        internal LayerId(int id) { this.id = id; }
    }

    public struct LayerDefinition {
        public readonly LayerId id;
        public readonly BufferType bufferType;
        public readonly string name;

        public LayerDefinition(LayerId id, string name, BufferType bufferType) {
            this.id = id;
            this.name = name;
            this.bufferType = bufferType;
        }
    }

    /// <summary>
    /// A set of data layers to be attached to each grid chunk.
    /// </summary>
    public class LayerConfig : System.ICloneable {
        private readonly List<LayerDefinition> definedLayers = new List<LayerDefinition>();

        public int Count { get { return definedLayers.Count; } }

        public LayerConfig() {
            this.definedLayers = new List<LayerDefinition>();
        }
        private LayerConfig(List<LayerDefinition> layers) {
            this.definedLayers = layers;
        }

        /// <summary>
        /// Returns a new LayerConfig object which includes all previously added and the newly added layer.
        /// </summary>
        public LayerConfig AddLayer(string name, BufferType type, out LayerId id) {
            id = new LayerId(this.definedLayers.Count);
            LayerDefinition newLayer = new LayerDefinition(id, name, type);
            List<LayerDefinition> newList = new List<LayerDefinition>(this.definedLayers);
            newList.Add(newLayer);
            return new LayerConfig(newList);
        }

        /// <summary>
        /// Try to find and return the layer with the specified name.
        /// </summary>
        public bool GetLayer(string name, out LayerDefinition layer) {
            layer = new LayerDefinition();
            int index = definedLayers.FindIndex(x => { return name.Equals(x.name); });
            bool result = index != -1;
            if (result) {
                layer = definedLayers[index];
            }
            return result;
        }

        /// <summary>
        /// Get the layer definition at the specified index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public LayerDefinition GetLayer(int i) {
            return definedLayers[i];
        }

        public object Clone() {
            return new LayerConfig(new List<LayerDefinition>(this.definedLayers));
        }
    }
}
