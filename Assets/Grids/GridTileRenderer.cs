﻿// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;

namespace AmarokGames.Grids {

    [System.Serializable]
    public struct TileRenderData {
        public bool draw;
        public TileVariant[] variants; 
    }

    /// <summary>
    /// UV data for all the different segments of a tile variant.
    /// A type of tile may eventually be able to have multiple variants. Eg. Dyed windows, blue/green/yellow/short/long grass, etc.
    /// In the tilesheet, each tile variant is basically a seperately and fully drawn tile sprite, so we need uv data for all the segments, per variant.
    /// </summary>
    public struct TileVariant {
        public TilePartUV uvMiddle;
        public TilePartUV uvTop;
        public TilePartUV uvBottom;
        public TilePartUV uvLeft;
        public TilePartUV uvRight;
        public TilePartUV uvOutsideTopLeft;
        public TilePartUV uvOutsideTopRight;
        public TilePartUV uvOutsideBottomLeft;
        public TilePartUV uvOutsideBottomRight;
        public TilePartUV uvInsideTopLeft;
        public TilePartUV uvInsideTopRight;
        public TilePartUV uvInsideBottomLeft;
        public TilePartUV uvInsideBottomRight;

        public TileVariant(Vector2 uv00, Vector2 uv11) {

            // Assume the following: 
            // Each tile variant sprite is 16 pixels wide and 24 high. (Or other size with same ratio)
            // The bottom 16x16 area contains the main tile plus its borders.
            // The topleft 8x8 area contains the inside corners for adjacent tiles.

            // Calculate the uvs of all tile segments
            Vector2 size = uv11 - uv00;
            Vector3 main = new Vector2(size.x, (2f/3f)*size.y);
            Vector2 a = 0.25f * main;

            // Since our sprite is divided in segments, we'll use these common values. 
            float x0 = uv00.x + 0 * a.x;
            float x1 = uv00.x + 1 * a.x;
            float x2 = uv00.x + 2 * a.x;
            float x3 = uv00.x + 3 * a.x;
            float x4 = uv00.x + 4 * a.x;
            float y0 = uv00.y + 0 * a.y;
            float y1 = uv00.y + 1 * a.y;
            float y2 = uv00.y + 2 * a.y;
            float y3 = uv00.y + 3 * a.y;
            float y4 = uv00.y + 4 * a.y;
            float y5 = uv00.y + 5 * a.y;
            float y6 = uv00.y + 6 * a.y;

            uvMiddle = new TilePartUV(x1, y1, x3, y3);
            uvTop    = new TilePartUV(x1, y3, x3, y4);
            uvBottom = new TilePartUV(x1, y0, x3, y1);
            uvLeft   = new TilePartUV(x0, y1, x1, y3);
            uvRight  = new TilePartUV(x3, y1, x4, y3);

            uvOutsideTopLeft     = new TilePartUV(x0, y3, x1, y4);
            uvOutsideTopRight    = new TilePartUV(x3, y3, x4, y4);
            uvOutsideBottomLeft  = new TilePartUV(x0, y0, x1, y1);
            uvOutsideBottomRight = new TilePartUV(x3, y0, x4, y1);


            // TODO uv Inside corners
            uvInsideTopLeft = new TilePartUV();
            uvInsideTopRight = new TilePartUV();
            uvInsideBottomLeft = new TilePartUV();
            uvInsideBottomRight = new TilePartUV();
        }
    }

    public struct TilePartUV {
        public Vector2 uv00;
        public Vector2 uv11;

        public TilePartUV(Vector2 uv00, Vector2 uv11) {
            this.uv00 = uv00;
            this.uv11 = uv11;
        }

        public TilePartUV(float x0, float y0, float x1, float y1) {
            this.uv00 = new Vector2(x0, y0);
            this.uv11 = new Vector2(x1, y1);
        }
    }

    public class GridTileRenderer : MonoBehaviour {

        [SerializeField]
        private TileRenderData[] tileData;

        [SerializeField]
        private int vertexCount;
        public int VertexCount { get { return vertexCount; } }

        private Grid2D grid;
        private Mesh mesh;

        private Grid2DBehaviour gridBehaviour;
        private MeshFilter filter;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> triangles = new List<int>();

        public static GridTileRenderer Create(string objName, TileRenderData[] tileData, Material material, Grid2DBehaviour gridBehaviour) {
            GameObject obj = new GameObject(objName);
            GridTileRenderer result = obj.AddComponent<GridTileRenderer>();
            result.gridBehaviour = gridBehaviour;
            result.grid = gridBehaviour.ParentGrid;
            result.filter = obj.AddComponent<MeshFilter>();
            MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            result.tileData = tileData;
            return result;
        }

        void Start() {
            grid = gridBehaviour.ParentGrid;
            mesh = new Mesh();
            filter = GetComponent<MeshFilter>();
            filter.sharedMesh = mesh;
        }

        void Update() {
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;

            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            triangles.Clear();

            IEnumerable<Int2> chunksToRender = gridBehaviour.GetChunksWithinCameraBounds(Camera.main);
            foreach (Int2 chunkCoord in chunksToRender) {
                // skip chunk if it doesn't exist.
                Chunk chunk;
                if (grid.TryGetChunk(chunkCoord, out chunk)) {
                    BuildChunkGeometry(grid, chunk, chunkCoord, tileData, mesh, chunkWidth, chunkHeight, vertices, uvs, normals, triangles);
                }
            }

            // finalize mesh
            vertexCount = vertices.Count;
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);

            mesh.UploadMeshData(false);
        }

        private static void BuildChunkGeometry(Grid2D grid, Chunk chunk, Int2 chunkCoord, TileRenderData[] tileRenderData, Mesh mesh, int chunkWidth, int chunkHeight, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles) {
            int vertexCount = vertices.Count;
            const int layerId = 1;
            
            // Copy data to a local buffer for easier access.
            // Because tiles encroach on their neighbours, we need both the data for the current chunk, and the borders of any neighbouring chunks.
            // For this reason, we'll also shift all the cells in the current chunk by a (1,1) offset
            ushort[,] b = new ushort[chunkHeight + 2, chunkWidth + 2];

            // copy data from the main chunk buffer to the local buffer
            UShortBuffer mainbuf = (UShortBuffer)chunk.GetBuffer(layerId);
            for(int y = 0; y < chunkHeight; y++) {
                for(int x = 0; x < chunkWidth; x++) {
                    int cellindex = Grid2D.GetCellIndex(new Int2(x, y), chunkWidth);
                    b[y + 1, x + 1] = mainbuf.GetValue(cellindex);
                }
            }

            // TODO copy data from neighbouring chunk buffers to local buffer
            // TODO Top
            // TODO Bottom
            // TODO Left
            // TODO Right

            // render tiles using previously filled local buffer
            // for every tile
            for (int y = 0; y < chunkHeight; y++) {
                for (int x = 0; x < chunkWidth; x++) {

                    // get tile values from buffer and determine if we should draw the tile
                    ushort value = b[y + 1, x + 1];
                    int variant = 0; // TODO Tile variants
                    if (tileRenderData[value].draw) {

                        // Draw the middle of the tile. The middle will always be drawn.
                        { 
                            Vector3 v = new Int2(chunkCoord.x * chunkWidth + x, chunkCoord.y * chunkWidth + y);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvMiddle.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvMiddle.uv11;
                            // TODO add quad size parameter because we'll need it later to draw the smaller tile segments.
                            AddQuad(v, Vector2.one, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // TODO Draw borders...
                        ushort bottomleft  = b[y + 0, x + 0];
                        ushort bottom      = b[y + 0, x + 1];
                        ushort bottomright = b[y + 0, x + 2];
                        ushort left        = b[y + 1, x + 0];
                        ushort right       = b[y + 1, x + 2];
                        ushort topleft     = b[y + 2, x + 0];
                        ushort top         = b[y + 2, x + 1];
                        ushort topright    = b[y + 2, x + 2];
                        
                        

                        // Draw Left border
                        if (right == 0 && topright == 0 && bottomright == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x + 1.0f, chunkCoord.y * chunkWidth + y);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvRight.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvRight.uv11;
                            Vector2 size = new Vector2(0.5f, 1);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw right border
                        if (left == 0 && topleft == 0 && bottomleft == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x - 0.5f, chunkCoord.y * chunkWidth + y);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvLeft.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvLeft.uv11;
                            Vector2 size = new Vector2(0.5f, 1);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw Top border
                        if (top == 0 && topleft == 0 && topright == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x + 0, chunkCoord.y * chunkWidth + y + 1.0f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvTop.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvTop.uv11;
                            Vector2 size = new Vector2(1, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw Bottom border
                        if (bottom == 0 && bottomleft == 0 && bottomright == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x + 0, chunkCoord.y * chunkWidth + y - 0.5f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvBottom.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvBottom.uv11;
                            Vector2 size = new Vector2(1, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // TODO Draw outside corners

                        // Draw top left corner
                        if (left == 0 && top == 0 && topleft == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x - 0.5f, chunkCoord.y * chunkWidth + y + 1.0f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvOutsideTopLeft.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvOutsideTopLeft.uv11;
                            Vector2 size = new Vector2(0.5f, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw top right corner
                        if (right == 0 && top == 0 && topright == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x + 1.0f, chunkCoord.y * chunkWidth + y + 1.0f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvOutsideTopRight.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvOutsideTopRight.uv11;
                            Vector2 size = new Vector2(0.5f, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom left corner
                        if (left == 0 && bottom == 0 && bottomleft == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x - 0.5f, chunkCoord.y * chunkWidth + y - 0.5f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvOutsideBottomLeft.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvOutsideBottomLeft.uv11;
                            Vector2 size = new Vector2(0.5f, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom right corner
                        if (right == 0 && bottom == 0 && bottomright == 0) {
                            Vector3 v = new Vector2(chunkCoord.x * chunkWidth + x + 1.0f, chunkCoord.y * chunkWidth + y - 0.5f);
                            Vector2 uv00 = tileRenderData[value].variants[0].uvOutsideBottomRight.uv00;
                            Vector2 uv11 = tileRenderData[value].variants[0].uvOutsideBottomRight.uv11;
                            Vector2 size = new Vector2(0.5f, 0.5f);
                            AddQuad(v, size, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }
                    }
                }
            }
        }

        private static void AddQuad(Vector3 v, Vector2 size, Vector2 uv00, Vector2 uv11, ref int vertexCount, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles) {
            // v1--v2
            // | / |
            // v0--v3

            Vector3 v0 = v;
            Vector3 v1 = v + new Vector3(0, size.y);
            Vector3 v2 = v + new Vector3(size.x, size.y);
            Vector3 v3 = v + new Vector3(size.x, 0);
            Vector3 n = new Vector3(0, 0, -1);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            Vector2 uv0 = new Vector2(uv00.x, uv00.y);
            Vector2 uv1 = new Vector2(uv00.x, uv11.y);
            Vector2 uv2 = new Vector2(uv11.x, uv11.y);
            Vector2 uv3 = new Vector2(uv11.x, uv00.y);

            uvs.Add(uv0);
            uvs.Add(uv1);
            uvs.Add(uv2);
            uvs.Add(uv3);

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