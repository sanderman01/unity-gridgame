using UnityEngine;
using UnityEngine.Assertions;

namespace AmarokGames.Grids {

    /// <summary>
    /// When added to 2D grid chunks, this component handles some minimal bookkeeping and rendering for meshes created by tile rendering system.
    /// Using this, a tile rendering system can easily attach a mesh to a chunk without bothering about the details of creating a child object and setting up components.
    /// 
    /// Note: Thruthfully, I would rather prefer to keep all meshes stored in a centralised system and render them using Graphics.DrawMesh,
    /// but there are some advantages to doing mesh rendering the more conventional way, using MeshFilter and MeshRenderer components.
    /// One of these is the ability to pauze the game within the unity editor and move the camera to inspect chunks. 
    /// That will not work properly if using Graphics.DrawMesh in an Update.
    /// </summary>
    public class ChunkMeshRenderer : MonoBehaviour {

        public Mesh Mesh {
            get { return meshFilter.sharedMesh; }
            set { meshFilter.sharedMesh = value; }
        }

        public Material Material {
            get { return meshRenderer.sharedMaterial; }
            set { meshRenderer.sharedMaterial = value; }
        }

        public int LastModified { get { return lastModified; } }

        public MeshFilter MeshFilter { get { return meshFilter; } }
        public MeshRenderer MeshRenderer { get { return meshRenderer; } }

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        [SerializeField]
        private int lastModified;

        public static ChunkMeshRenderer Create(string name, GameObject parentChunk, Material material, Mesh mesh) {

            Assert.IsNotNull(parentChunk, "parentChunk is null");
            Assert.IsNotNull(material, "material is null");
            Assert.IsNotNull(mesh, "mesh is null");

            // Create object and components
            GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(ChunkMeshRenderer));
            obj.transform.SetParent(parentChunk.transform, false);
            ChunkMeshRenderer result = obj.GetComponent<ChunkMeshRenderer>();
            result.meshFilter = result.GetComponent<MeshFilter>();
            result.meshRenderer = result.GetComponent<MeshRenderer>();

            // Set values;
            result.meshFilter.sharedMesh = mesh;
            result.meshRenderer.sharedMaterial = material;

            return result;
        }

        public void MarkModified(int frameCount) {
            lastModified = frameCount;
        }
    }
}

