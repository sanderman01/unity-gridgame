// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using System.Collections.Generic;

namespace AmarokGames.Grids.Data {

    public struct LayerDefinition {
        public readonly int index;
        public readonly string name;
        public readonly BufferType bufferType;

        public LayerDefinition(int index, string name, BufferType bufferType) {
            this.index = index;
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
        public LayerConfig AddLayer(string name, BufferType type, out int index) {
            index = this.definedLayers.Count;
            LayerDefinition newLayer = new LayerDefinition(index, name, type);
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
