// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AmarokGames.Grids {

    public class GridSolidRenderer : IGameSystem {
        
        private LayerId layerId = new LayerId(0);
        private Material material;

        private Grid2D grid;
        private Mesh mesh;

        private MeshFilter filter;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> triangles = new List<int>();

        private int vertexCount;
        public int VertexCount { get { return vertexCount; } }

        private bool enabled = true;
        public bool Enabled {
            get {
                return enabled;
            }

            set {
                if (!enabled && value) Enable();
                else if (enabled && !value) Disable();
                enabled = value;
            }
        }

        private void Enable() {
            filter.gameObject.SetActive(true);
        }

        private void Disable() {
            filter.gameObject.SetActive(false);
            mesh.Clear();
        }



        public GridSolidRenderer(string objName, Material material, Grid2D grid) {
            GameObject obj = new GameObject(objName);
            this.grid = grid;
            this.filter = obj.AddComponent<MeshFilter>();
            this.filter.sharedMesh = mesh = new Mesh();
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        public void Update(World world, IEnumerable<Grid2D> grids) {
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;
            IEnumerable<Int2> chunks = grid.GetAllChunks();
            Bounds bounds = Camera.main.CalcOrthographicCameraBounds();

            vertices.Clear();
            normals.Clear();
            triangles.Clear();

            foreach (Int2 chunkCoord in chunks) {
                ChunkData chunk;
                if (grid.TryGetChunkData(chunkCoord, out chunk) && bounds.Intersects(grid.CalculateChunkAABB(chunkCoord))) {
                    BitBuffer buffer = (BitBuffer)chunk.GetBuffer(layerId);
                    BuildChunkGeometry(chunkCoord, buffer, mesh, chunkWidth, chunkHeight, vertices, normals, triangles);
                }
            }

            // finalize mesh
            vertexCount = vertices.Count;
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.UploadMeshData(false);
        }

        private static void BuildChunkGeometry(Int2 chunkCoord, BitBuffer buffer, Mesh mesh, int chunkWidth, int chunkHeight, List<Vector3> vertices, List<Vector3> normals, List<int> triangles) {
            int vertexCount = vertices.Count;

            for (int i = 0; i < buffer.Length; ++i) {
                // generate vertices
                bool value = buffer.GetValue(i);
                if (value) {
                    Int2 gridCoord = Grid2D.GetGridCoordFromCellIndex(i, chunkCoord, chunkWidth, chunkHeight);
                    Vector3 vertexPos = gridCoord;
                    // add quad
                    AddQuad(vertexPos, ref vertexCount, vertices, normals, triangles);
                }
            }



        }

        private static void AddQuad(Vector3 v, ref int vertexCount, List<Vector3> vertices, List<Vector3> normals, List<int> triangles) {
            Vector3 v0 = v;
            Vector3 v1 = v + new Vector3(0, 1);
            Vector3 v2 = v + new Vector3(1, 1);
            Vector3 v3 = v + new Vector3(1, 0);
            Vector3 n = new Vector3(0, 0, -1);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            // normals for both triangles
            for (int i = 0; i < 4; ++i) normals.Add(n);

            // 1st triangle
            triangles.Add(vertexCount + 0);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);

            // 2nd triangle
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
            triangles.Add(vertexCount + 0);

            vertexCount += 4;
        }
    }
}