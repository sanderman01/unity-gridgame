// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace AmarokGames.Grids {

    [System.Serializable]
    public struct TileRenderData {
        public bool draw;
        public ushort zLayer;
        public TileVariant[] variants;
    }

    public class GridTileRenderer : MonoBehaviour {

        private Material mat;

        [SerializeField]
        private TileRenderData[] tileData;

        [SerializeField]
        private int vertexCount;
        public int VertexCount { get { return vertexCount; } }

        private Grid2D grid;

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> triangles = new List<int>();

        private Dictionary<Int2, ChunkMeshRenderer> chunkMeshes = new Dictionary<Int2, ChunkMeshRenderer>();

        const int layerId = 1;

        public static GridTileRenderer Create(string objName, TileRenderData[] tileData, Material mat, Grid2D grid) {

            Assert.IsNotNull(mat);

            GameObject obj = new GameObject(objName);
            GridTileRenderer result = obj.AddComponent<GridTileRenderer>();
            result.grid = grid;
            result.tileData = tileData;
            result.mat = mat;
            return result;
        }

        void Update() {
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;

            var chunksToRender = grid.GetChunksWithinCameraBounds(Camera.main);
            foreach (Int2 chunkCoord in chunksToRender) {
                // skip chunk if it doesn't exist.
                ChunkData chunk;
                if (grid.TryGetChunkData(chunkCoord, out chunk)) {

                    Mesh mesh = null;
                    ChunkMeshRenderer chunkMeshRenderer;

                    // if no chunk mesh renderer exists yet, create one now
                    if (!chunkMeshes.TryGetValue(chunkCoord, out chunkMeshRenderer)) {
                        
                        mesh = new Mesh();
                        Grid2DChunk chunkObject;
                        grid.TryGetChunkObject(chunkCoord, out chunkObject);
                        GameObject parentChunkObj = chunkObject.gameObject;
                        string name = string.Format("chunk {0} tilemesh", chunkCoord);
                        chunkMeshRenderer = ChunkMeshRenderer.Create(name, parentChunkObj, mat, mesh);
                        chunkMeshes.Add(chunkCoord, chunkMeshRenderer);
                    }
                    else {
                        // mesh renderer already exists
                        mesh = chunkMeshRenderer.Mesh;

                        // check if we need to update it
                        if (chunk.LastModified < chunkMeshRenderer.LastModified) {
                            // We are up to date, no need to update the mesh.
                            continue;
                        }

                    }

                    // We have a mesh. Generate new data for the mesh.
                    vertices.Clear();
                    uvs.Clear();
                    normals.Clear();
                    triangles.Clear();
                    BuildChunkGeometry(grid, chunk, chunkCoord, tileData, mesh, chunkWidth, chunkHeight, vertices, uvs, normals, triangles);

                    // finalize mesh
                    vertexCount = vertices.Count;
                    mesh.Clear();
                    mesh.SetVertices(vertices);
                    mesh.SetUVs(0, uvs);
                    mesh.SetNormals(normals);
                    mesh.SetTriangles(triangles, 0);
                    mesh.UploadMeshData(false);
                    chunkMeshRenderer.MarkModified(Time.frameCount);
                }
            }
        }

        private static void BuildChunkGeometry(Grid2D grid, ChunkData chunk, Int2 chunkCoord, TileRenderData[] tileRenderData, Mesh mesh, int chunkWidth, int chunkHeight, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles) {
            int vertexCount = vertices.Count;
            const float zOffset = -0.1f;

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

            // copy data from neighbouring chunks to the local buffer
            for (int y = -1; y < chunkHeight + 1; y++) {
                for (int x = -1; x < chunkWidth + 1; x++) {
                    if (y == -1 || y == chunkWidth || x == -1 || x == chunkWidth) {
                        Int2 coord = new Int2(chunkCoord.x * chunkWidth + x, chunkCoord.y * chunkHeight + y);
                        b[y + 1, x + 1] = grid.GetUShort(coord, layerId);
                    }
                }
            }

            // render tiles using previously filled local buffer
            // for every tile
            for (int y = 0; y < chunkHeight; y++) {
                for (int x = 0; x < chunkWidth; x++) {

                    Int2 gridCoord = new Int2(x, y);

                    // get tile values from buffer and determine if we should draw the tile
                    ushort current = b[y + 1, x + 1];
                    int variant = 0; // TODO Tile variants

                    // TODO Draw borders...
                    ushort bottomleft = b[y + 0, x + 0];
                    ushort bottom = b[y + 0, x + 1];
                    ushort bottomright = b[y + 0, x + 2];
                    ushort left = b[y + 1, x + 0];
                    ushort right = b[y + 1, x + 2];
                    ushort topleft = b[y + 2, x + 0];
                    ushort top = b[y + 2, x + 1];
                    ushort topright = b[y + 2, x + 2];


                    // standard quad sizes
                    Vector2 high = new Vector2(0.5f, 1);
                    Vector2 wide = new Vector2(1, 0.5f);
                    Vector2 square = new Vector2(0.5f, 0.5f);

                    if (tileRenderData[current].draw) {

                        // Draw the middle of the tile. The middle will always be drawn.
                        int zLayer = tileRenderData[current].zLayer;
                        float zDepth = zOffset * zLayer;
                        {
                            Vector3 v = new Vector3(gridCoord.x, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvMiddle.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvMiddle.uv11;
                            AddQuad(v, Vector2.one, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw outside borders
                        // Draw right border
                        bool renderRight = right != current && topright != current && bottomright != current && zLayer > tileRenderData[right].zLayer;
                        if (renderRight) {
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvRight.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvRight.uv11;
                            AddQuad(v, high, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw left border
                        bool renderLeft = left != current && topleft != current && bottomleft != current && zLayer > tileRenderData[left].zLayer;
                        if (renderLeft) {
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvLeft.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvLeft.uv11;
                            AddQuad(v, high, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw top border
                        bool renderTop = top != current && topleft != current && topright != current && zLayer > tileRenderData[top].zLayer;
                        if (renderTop) {
                            Vector3 v = new Vector3(gridCoord.x + 0, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvTop.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvTop.uv11;
                            AddQuad(v, wide, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom border
                        bool renderBottom = bottom != current && bottomleft != current && bottomright != current && zLayer > tileRenderData[bottom].zLayer;
                        if (renderBottom) {
                            Vector3 v = new Vector3(gridCoord.x + 0, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvBottom.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvBottom.uv11;
                            AddQuad(v, wide, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw outside corners
                        // Draw top left outside corner
                        bool renderTopLeft = left != current && top != current && topleft != current && zLayer > tileRenderData[topleft].zLayer;
                        if (renderTopLeft) {
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvOutsideTopLeft.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvOutsideTopLeft.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw top right outside corner
                        bool renderTopRight = right != current && top != current && topright != current && zLayer > tileRenderData[topright].zLayer;
                        if (renderTopRight) {
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvOutsideTopRight.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvOutsideTopRight.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom left outside corner
                        bool renderBottomLeft = left != current && bottom != current && bottomleft != current && zLayer > tileRenderData[bottomleft].zLayer;
                        if (renderBottomLeft) {
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvOutsideBottomLeft.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvOutsideBottomLeft.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom right outside corner
                        bool renderBottomRight = right != current && bottom != current && bottomright != current && zLayer > tileRenderData[bottomright].zLayer;
                        if (renderBottomRight) {
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[current].variants[0].uvOutsideBottomRight.uv00;
                            Vector2 uv11 = tileRenderData[current].variants[0].uvOutsideBottomRight.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }
                    }

                    // Draw inside corners
                    bool renderBottomLeftInside  = tileRenderData[left].draw && left > current && bottom == left && bottom != current;
                    bool renderBottomRightInside = tileRenderData[right].draw && right > current && bottom == right && bottom != current;
                    bool renderTopLeftInside     = tileRenderData[left].draw && left > current && top == left && top != current;
                    bool renderTopRightInside    = tileRenderData[right].draw && right > current && top == right && top != current;

                    bool renderRight1 = renderTopLeftInside && bottom != left;
                    bool renderTop1 = renderBottomRightInside && bottom != left;

                    bool renderLeft1 = renderTopRightInside && bottom != right;
                    bool renderTop2 = renderBottomLeftInside && bottom != right;

                    bool renderRight2 = renderBottomLeftInside && top != left;
                    bool renderBottom1 = renderTopRightInside && top != left;

                    bool renderLeft2 = renderBottomRightInside && top != right;
                    bool renderBottom2 = renderTopLeftInside && top != right;

                    // Draw bottom left inside corner
                    if (renderBottomLeftInside) {
                        float zDepth = zOffset * tileRenderData[left].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[left].variants[0].uvInsideBottomLeft.uv00;
                        Vector2 uv11 = tileRenderData[left].variants[0].uvInsideBottomLeft.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderRight1) {
                        float zDepth = zOffset * tileRenderData[left].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[left].variants[0].uvRight1.uv00;
                        Vector2 uv11 = tileRenderData[left].variants[0].uvRight1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderTop1) {
                        float zDepth = zOffset * tileRenderData[bottom].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[bottom].variants[0].uvTop1.uv00;
                        Vector2 uv11 = tileRenderData[bottom].variants[0].uvTop1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // Draw bottom right inside corner
                    if (renderBottomRightInside) {
                        float zDepth = zOffset * tileRenderData[bottom].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[right].variants[0].uvInsideBottomRight.uv00;
                        Vector2 uv11 = tileRenderData[right].variants[0].uvInsideBottomRight.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderTop2) {
                        float zDepth = zOffset * tileRenderData[bottom].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[bottom].variants[0].uvTop2.uv00;
                        Vector2 uv11 = tileRenderData[bottom].variants[0].uvTop2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderLeft1) {
                        float zDepth = zOffset * tileRenderData[right].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[right].variants[0].uvLeft1.uv00;
                        Vector2 uv11 = tileRenderData[right].variants[0].uvLeft1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // Draw top left inside corner
                    if (renderTopLeftInside) {
                        float zDepth = zOffset * tileRenderData[left].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[left].variants[0].uvInsideTopLeft.uv00;
                        Vector2 uv11 = tileRenderData[left].variants[0].uvInsideTopLeft.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderRight2) {
                        float zDepth = zOffset * tileRenderData[left].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[left].variants[0].uvRight2.uv00;
                        Vector2 uv11 = tileRenderData[left].variants[0].uvRight2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderBottom1) {
                        float zDepth = zOffset * tileRenderData[top].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[top].variants[0].uvBottom1.uv00;
                        Vector2 uv11 = tileRenderData[top].variants[0].uvBottom1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // draw top right inside corner
                    if (renderTopRightInside) {
                        float zDepth = zOffset * tileRenderData[right].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[right].variants[0].uvInsideTopRight.uv00;
                        Vector2 uv11 = tileRenderData[right].variants[0].uvInsideTopRight.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderLeft2) {
                        float zDepth = zOffset * tileRenderData[right].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[right].variants[0].uvLeft2.uv00;
                        Vector2 uv11 = tileRenderData[right].variants[0].uvLeft2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderBottom2) {
                        float zDepth = zOffset * tileRenderData[top].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[top].variants[0].uvBottom2.uv00;
                        Vector2 uv11 = tileRenderData[top].variants[0].uvBottom2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
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