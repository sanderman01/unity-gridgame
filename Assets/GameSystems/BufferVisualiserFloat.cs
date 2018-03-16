// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using UnityEngine;

namespace AmarokGames.GridGame {

    public class BufferVisualiserFloat : GameSystemBase, IGameSystem {

        private LayerId layer;
        private Material material;
        private Mesh mesh;

        public static BufferVisualiserFloat Create(Material material, LayerId layer) {
            BufferVisualiserFloat sys = Create<BufferVisualiserFloat>();
            sys.material = material;
            sys.layer = layer;
            sys.mesh = CreateQuadMesh();
            return sys;
        }

        private static Mesh CreateQuadMesh() {

            //Vector3 offset = Vector3.zero;
            Vector3 offset = -0.5f * Vector3.one;

            Vector3 n0 = new Vector3(0, 0, 0);
            Vector3 n1 = new Vector3(0, 1, 0);
            Vector3 n2 = new Vector3(1, 1, 0);
            Vector3 n3 = new Vector3(1, 0, 0);

            Vector3[] verts = {
                n0 + offset,
                n1 + offset,
                n2 + offset,
                n3 + offset
            };

            Vector3 n = new Vector3(0, 0, -1);
            Vector3[] normals = { n, n, n, n };

            Vector2[] uvs = { n0, n1, n2, n3 };

            int[] triangles = {
                0, 1, 2,
                0, 2, 3
            };

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.SetTriangles(triangles, 0);
            return mesh;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            Camera cam = Camera.main;
            foreach (Grid2D grid in world.Grids) {
                foreach (Int2 chunkCoord in grid.GetChunksWithinCameraBounds(cam)) {

                    Grid2DChunk chunk;
                    if (grid.TryGetChunkObject(chunkCoord, out chunk)) {

                        FloatBuffer buffer = (FloatBuffer)grid.TryGetBuffer(chunkCoord, layer);

                        for (int i = 0; i < Grid2D.ChunkHeight * Grid2D.ChunkWidth; i++) {

                            float value = buffer.GetValue(i);
                            value = Mathf.InverseLerp(0, 2, value);
                            Vector3 offset = 0.5f * Vector3.one;

                            Int2 localGridCoord = Grid2D.GetLocalGridCoordFromCellIndex(i);
                            Vector3 pos = new Vector3(localGridCoord.x, localGridCoord.y, 0) + offset;
                            Matrix4x4 tr = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * value);
                            Matrix4x4 m = chunk.transform.localToWorldMatrix * tr;

                            Graphics.DrawMesh(mesh, m, material, 0);
                        }
                    }
                }
            }
        }

        protected override void Disable() {
        }

        protected override void Enable() {
        }

        void OnDestroy() {
            UnityEngine.Object.Destroy(mesh);
        }
    }
}
