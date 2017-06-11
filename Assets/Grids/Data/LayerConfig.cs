// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;

namespace AmarokGames.Grids.Data {

    /// <summary>
    /// Identifier used to access chunk datalayer buffers.
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
        public LayerId AddLayer(string name, BufferType type) {
            LayerId id = new LayerId(definedLayers.Count);
            LayerDefinition newLayer = new LayerDefinition(id, name, type);
            definedLayers.Add(newLayer);
            return id;
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
        /// Get the layer definition for the specified id.
        /// </summary>
        public LayerDefinition this[LayerId id] {
            get { return definedLayers[id.id]; }
        }

        public object Clone() {
            return new LayerConfig(new List<LayerDefinition>(this.definedLayers));
        }
    }
}
