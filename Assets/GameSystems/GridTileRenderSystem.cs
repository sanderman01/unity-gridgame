// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AmarokGames.Grids {

    [System.Serializable]
    public struct TileRenderData {
        public bool draw;
        public ushort zLayer;
        public TileVariant[] variants;
    }

    public class GridTileRenderSystem : GameSystemBase, IGameSystem {

        [SerializeField]
        public Material material;
        public Material Material {
            get { return this.material; }
            set { this.material = value; }
        }

        [SerializeField]
        public TileRenderData[] tileData;
        public TileRenderData[] TileData {
            get { return this.tileData; }
            set { this.tileData = value; }
        }

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> triangles = new List<int>();

        private Dictionary<ChunkKey, ChunkMeshRenderer> chunkMeshes = new Dictionary<ChunkKey, ChunkMeshRenderer>();

        private LayerId layerId;
        private float zOffsetGlobal;

        protected override void Enable() {
        }

        protected override void Disable() {
            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            triangles.Clear();
            foreach (ChunkMeshRenderer renderer in chunkMeshes.Values) {
                if (renderer != null) {
                    UnityEngine.Object.Destroy(renderer.Mesh);
                    UnityEngine.Object.Destroy(renderer.gameObject);
                }
            }
            chunkMeshes.Clear();
        }

        public static GridTileRenderSystem Create(LayerId layerId, float zPos) {
            GridTileRenderSystem sys = Create<GridTileRenderSystem>();
            sys.layerId = layerId;
            sys.zOffsetGlobal = zPos;
            return sys;
        }

        public static TileRenderData[] CreateTileRenderData(GameRegistry gameRegistry) {
            int tileCount = gameRegistry.GetTileCount();
            TileRenderData[] tileData = new TileRenderData[tileCount];
            for (int i = 0; i < tileCount; ++i) {
                Tile tile = gameRegistry.GetTileById(i);

                TileRenderData renderData = new TileRenderData();
                renderData.draw = tile.BatchedRendering;
                renderData.zLayer = (ushort)i;
                renderData.variants = GameRegistry.GetTileVariants(tile.SpriteUV);

                tileData[i] = renderData;
            }
            return tileData;
        }

        public void PostInit(TileRenderData[] renderData, Material material) {
            this.TileData = renderData;
            this.Material = material;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            foreach (Grid2D grid in world.Grids) UpdateWorld(world, deltaTime, grid);
        }

        private void UpdateWorld(World world, float deltaTime, Grid2D grid) {
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;

            var chunks = grid.GetAllChunks();
            Rect bounds = Camera.main.OrthoBounds2D();

            foreach (Int2 chunkCoord in chunks) {
                UpdateChunk(bounds, world.WorldId, grid, chunkCoord);
            }
        }

        public void Remove(Grid2D grid) {
            throw new System.NotImplementedException();
        }

        private void UpdateChunk(Rect cameraBounds, int worldId, Grid2D grid, Int2 chunkCoord) {

            UnityEngine.Profiling.Profiler.BeginSample("UpdateChunk");

            // skip chunk if it doesn't exist.
            Grid2DChunk chunk;
            if (grid.TryGetChunkObject(chunkCoord, out chunk)) {

                if (cameraBounds.Overlaps(chunk.Bounds2D)) {
                    // The chunk is currently visible. Refresh the mesh if needed.
                    RefreshChunk(worldId, grid, chunk.Data, chunkCoord);
                } else {
                    // The chunk is currently not visible. We should clean up the mesh so we don't waste memory.
                    CleanChunk(worldId, grid.GridId, chunkCoord);
                }

            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        private void CleanChunk(int worldId, int gridId, Int2 chunkCoord) {

            ChunkMeshRenderer chunkMeshRenderer;
            if (chunkMeshes.TryGetValue(new ChunkKey(worldId, gridId, chunkCoord), out chunkMeshRenderer)) {
                chunkMeshRenderer.Mesh.Clear();
                chunkMeshRenderer.MarkModified(0);
            }
        }

        private void RefreshChunk(int worldId, Grid2D grid, ChunkData chunk, Int2 chunkCoord) {
            UnityEngine.Profiling.Profiler.BeginSample("RefreshChunk");
            Mesh mesh = null;
            ChunkMeshRenderer chunkMeshRenderer;

            // if no chunk mesh renderer exists yet, create one now
            ChunkKey key = new ChunkKey(worldId, grid.GridId, chunkCoord);
            if (!chunkMeshes.TryGetValue(key, out chunkMeshRenderer)) {
                mesh = new Mesh();
                Grid2DChunk chunkObject;
                grid.TryGetChunkObject(chunkCoord, out chunkObject);

                // Check that the game object is still valid.
                if(chunkObject == null) {
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }

                GameObject parentChunkObj = chunkObject.gameObject;
                string name = string.Format("chunk {0} tilemesh", chunkCoord);
                chunkMeshRenderer = ChunkMeshRenderer.Create(name, parentChunkObj, material, mesh);
                chunkMeshes.Add(key, chunkMeshRenderer);
            } else {
                // mesh renderer already exists in dictionary
                // check that it hasn't been destroyed since
                if(chunkMeshRenderer == null) {
                    chunkMeshes.Remove(key);
                    RefreshChunk(worldId, grid, chunk, chunkCoord);
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }

                mesh = chunkMeshRenderer.Mesh;

                // check if we need to update it
                if (chunk.LastModified < chunkMeshRenderer.LastModified) {
                    // We are up to date, no need to update the mesh.
                    UnityEngine.Profiling.Profiler.EndSample();
                    return;
                }
            }

            // We have a mesh. Generate new data for the mesh.
            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            triangles.Clear();
            BuildChunkGeometry(grid, layerId, chunk, chunkCoord, tileData, mesh, vertices, uvs, normals, triangles, zOffsetGlobal);

            // finalize mesh
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.UploadMeshData(false);
            chunkMeshRenderer.MarkModified(Time.frameCount);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private static void BuildChunkGeometry(Grid2D grid, LayerId layerId, ChunkData chunk, Int2 chunkCoord, TileRenderData[] tileRenderData, Mesh mesh, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<int> triangles, float zOffsetGlobal) {
            int chunkWidth = grid.ChunkWidth;
            int chunkHeight = grid.ChunkHeight;
            int vertexCount = vertices.Count;
            const float zOffset = -0.01f;

            // Copy data to a local buffer for easier access.
            // Because tiles encroach on their neighbours, we need both the data for the current chunk, and the borders of any neighbouring chunks.
            // For this reason, we'll also shift all the cells in the current chunk by a (1,1) offset
            uint[,] b = new uint[chunkHeight + 2, chunkWidth + 2];

            // copy data from the main chunk buffer to the local buffer
            BufferUnsignedInt32 mainbuf = (BufferUnsignedInt32)chunk.GetBuffer(layerId);
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
                        b[y + 1, x + 1] = (uint)grid.GetUnsignedInt(coord, layerId);
                    }
                }
            }

            // render tiles using previously filled local buffer
            // for every tile
            for (int y = 0; y < chunkHeight; y++) {
                for (int x = 0; x < chunkWidth; x++) {

                    Int2 gridCoord = new Int2(x, y);

                    // get tile values from buffer and determine if we should draw the tile
                    TileStateId current = (TileStateId)b[y + 1, x + 1];
                    TileStateId bottomleft = (TileStateId)b[y + 0, x + 0];
                    TileStateId bottom = (TileStateId)b[y + 0, x + 1];
                    TileStateId bottomright = (TileStateId)b[y + 0, x + 2];
                    TileStateId left = (TileStateId)b[y + 1, x + 0];
                    TileStateId right = (TileStateId)b[y + 1, x + 2];
                    TileStateId topleft = (TileStateId)b[y + 2, x + 0];
                    TileStateId top = (TileStateId)b[y + 2, x + 1];
                    TileStateId topright = (TileStateId)b[y + 2, x + 2];

                    uint currentTileId = current.TileId;
                    uint bottomleftTileId = bottomleft.TileId;
                    uint bottomTileId = bottom.TileId;
                    uint bottomrightTileId = bottomright.TileId;
                    uint leftTileId = left.TileId;
                    uint rightTileId = right.TileId;
                    uint topleftTileId = topleft.TileId;
                    uint topTileId = top.TileId;
                    uint toprightTileId = topright.TileId;

                    // These will eventually be used to render a different variation based on a tile's meta value
                    //uint currentMeta = current.Meta;
                    //uint bottomleftMeta = bottomleft.Meta;
                    //uint bottomMeta = bottom.Meta;
                    //uint bottomrightMeta = bottomright.Meta;
                    //uint leftMeta = left.Meta;
                    //uint rightMeta = right.Meta;
                    //uint topleftMeta = topleft.Meta;
                    //uint topMeta = top.Meta;
                    //uint toprightMeta = topright.Meta;

                    // standard quad sizes
                    Vector2 high = new Vector2(0.5f, 1);
                    Vector2 wide = new Vector2(1, 0.5f);
                    Vector2 square = new Vector2(0.5f, 0.5f);

                    if (tileRenderData[currentTileId].draw) {

                        // Draw the middle of the tile. The middle will always be drawn.
                        int zLayer = tileRenderData[currentTileId].zLayer;
                        float zDepth = zOffsetGlobal + zOffset * zLayer;
                        {
                            int variant = GetPseudoRandomInt(x, y) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvMiddle.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvMiddle.uv11;
                            AddQuad(v, Vector2.one, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw outside borders
                        // Draw right border
                        bool renderRight = rightTileId != currentTileId && toprightTileId != currentTileId && bottomrightTileId != currentTileId && zLayer > tileRenderData[rightTileId].zLayer;
                        if (renderRight) {
                            int variant = GetPseudoRandomInt(x + 1, y) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvRight.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvRight.uv11;
                            AddQuad(v, high, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw left border
                        bool renderLeft = leftTileId != currentTileId && topleftTileId != currentTileId && bottomleftTileId != currentTileId && zLayer > tileRenderData[leftTileId].zLayer;
                        if (renderLeft) {
                            int randomValue = GetPseudoRandomInt(x - 1, y);
                            int variant = randomValue % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvLeft.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvLeft.uv11;
                            AddQuad(v, high, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw top border
                        bool renderTop = topTileId != currentTileId && topleftTileId != currentTileId && toprightTileId != currentTileId && zLayer > tileRenderData[topTileId].zLayer;
                        if (renderTop) {
                            int variant = GetPseudoRandomInt(x, y + 1) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x + 0, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvTop.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvTop.uv11;
                            AddQuad(v, wide, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom border
                        bool renderBottom = bottomTileId != currentTileId && bottomleftTileId != currentTileId && bottomrightTileId != currentTileId && zLayer > tileRenderData[bottomTileId].zLayer;
                        if (renderBottom) {
                            int variant = GetPseudoRandomInt(x, y - 1) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x + 0, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvBottom.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvBottom.uv11;
                            AddQuad(v, wide, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw outside corners
                        // Draw top left outside corner
                        bool renderTopLeft = leftTileId != currentTileId && topTileId != currentTileId && topleftTileId != currentTileId && zLayer > tileRenderData[topleftTileId].zLayer;
                        if (renderTopLeft) {
                            int variant = GetPseudoRandomInt(x - 1, y + 1) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvOutsideTopLeft.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvOutsideTopLeft.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw top right outside corner
                        bool renderTopRight = rightTileId != currentTileId && topTileId != currentTileId && toprightTileId != currentTileId && zLayer > tileRenderData[toprightTileId].zLayer;
                        if (renderTopRight) {
                            int variant = GetPseudoRandomInt(x + 1, y + 1) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y + 1.0f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvOutsideTopRight.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvOutsideTopRight.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom left outside corner
                        bool renderBottomLeft = leftTileId != currentTileId && bottomTileId != currentTileId && bottomleftTileId != currentTileId && zLayer > tileRenderData[bottomleftTileId].zLayer;
                        if (renderBottomLeft) {
                            int variant = GetPseudoRandomInt(x, y - 1) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x - 0.5f, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvOutsideBottomLeft.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvOutsideBottomLeft.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }

                        // Draw bottom right outside corner
                        bool renderBottomRight = rightTileId != currentTileId && bottomTileId != currentTileId && bottomrightTileId != currentTileId && zLayer > tileRenderData[bottomrightTileId].zLayer;
                        if (renderBottomRight) {
                            int variant = GetPseudoRandomInt(x + 1, y) % tileRenderData[currentTileId].variants.Length;
                            Vector3 v = new Vector3(gridCoord.x + 1.0f, gridCoord.y - 0.5f, zDepth);
                            Vector2 uv00 = tileRenderData[currentTileId].variants[variant].uvOutsideBottomRight.uv00;
                            Vector2 uv11 = tileRenderData[currentTileId].variants[variant].uvOutsideBottomRight.uv11;
                            AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                        }
                    }

                    // Draw inside corners
                    bool renderBottomLeftInside  = tileRenderData[leftTileId].draw && leftTileId > currentTileId && bottomTileId == leftTileId && bottomTileId != currentTileId;
                    bool renderBottomRightInside = tileRenderData[rightTileId].draw && rightTileId > currentTileId && bottomTileId == rightTileId && bottomTileId != currentTileId;
                    bool renderTopLeftInside     = tileRenderData[leftTileId].draw && leftTileId > currentTileId && topTileId == leftTileId && topTileId != currentTileId;
                    bool renderTopRightInside    = tileRenderData[rightTileId].draw && rightTileId > currentTileId && topTileId == rightTileId && topTileId != currentTileId;

                    bool renderRight1 = renderTopLeftInside && bottomTileId != leftTileId;
                    bool renderTop1 = renderBottomRightInside && bottomTileId != leftTileId;

                    bool renderLeft1 = renderTopRightInside && bottomTileId != rightTileId;
                    bool renderTop2 = renderBottomLeftInside && bottomTileId != rightTileId;

                    bool renderRight2 = renderBottomLeftInside && topTileId != leftTileId;
                    bool renderBottom1 = renderTopRightInside && topTileId != leftTileId;

                    bool renderLeft2 = renderBottomRightInside && topTileId != rightTileId;
                    bool renderBottom2 = renderTopLeftInside && topTileId != rightTileId;

                    // Draw bottom left inside corner
                    if (renderBottomLeftInside) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[leftTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[leftTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[leftTileId].variants[variant].uvInsideBottomLeft.uv00;
                        Vector2 uv11 = tileRenderData[leftTileId].variants[variant].uvInsideBottomLeft.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderRight1) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[leftTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[leftTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[leftTileId].variants[variant].uvRight1.uv00;
                        Vector2 uv11 = tileRenderData[leftTileId].variants[variant].uvRight1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderTop1) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[bottomTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[bottomTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[bottomTileId].variants[variant].uvTop1.uv00;
                        Vector2 uv11 = tileRenderData[bottomTileId].variants[variant].uvTop1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // Draw bottom right inside corner
                    if (renderBottomRightInside) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[rightTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[bottomTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[rightTileId].variants[variant].uvInsideBottomRight.uv00;
                        Vector2 uv11 = tileRenderData[rightTileId].variants[variant].uvInsideBottomRight.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderTop2) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[bottomTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[bottomTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[bottomTileId].variants[variant].uvTop2.uv00;
                        Vector2 uv11 = tileRenderData[bottomTileId].variants[variant].uvTop2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderLeft1) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[rightTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[rightTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.0f, zDepth);
                        Vector2 uv00 = tileRenderData[rightTileId].variants[variant].uvLeft1.uv00;
                        Vector2 uv11 = tileRenderData[rightTileId].variants[variant].uvLeft1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // Draw top left inside corner
                    if (renderTopLeftInside) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[leftTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[leftTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[leftTileId].variants[variant].uvInsideTopLeft.uv00;
                        Vector2 uv11 = tileRenderData[leftTileId].variants[variant].uvInsideTopLeft.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderRight2) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[leftTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[leftTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[leftTileId].variants[variant].uvRight2.uv00;
                        Vector2 uv11 = tileRenderData[leftTileId].variants[variant].uvRight2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderBottom1) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[topTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[topTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.0f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[topTileId].variants[variant].uvBottom1.uv00;
                        Vector2 uv11 = tileRenderData[topTileId].variants[variant].uvBottom1.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }

                    // draw top right inside corner
                    if (renderTopRightInside) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[rightTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[rightTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[rightTileId].variants[variant].uvInsideTopRight.uv00;
                        Vector2 uv11 = tileRenderData[rightTileId].variants[variant].uvInsideTopRight.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderLeft2) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[rightTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[rightTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[rightTileId].variants[variant].uvLeft2.uv00;
                        Vector2 uv11 = tileRenderData[rightTileId].variants[variant].uvLeft2.uv11;
                        AddQuad(v, square, uv00, uv11, ref vertexCount, vertices, uvs, normals, triangles);
                    }
                    if (renderBottom2) {
                        int variant = GetPseudoRandomInt(x, y) % tileRenderData[topTileId].variants.Length;
                        float zDepth = zOffsetGlobal + zOffset * tileRenderData[topTileId].zLayer;
                        Vector3 v = new Vector3(gridCoord.x + 0.5f, gridCoord.y + 0.5f, zDepth);
                        Vector2 uv00 = tileRenderData[topTileId].variants[variant].uvBottom2.uv00;
                        Vector2 uv11 = tileRenderData[topTileId].variants[variant].uvBottom2.uv11;
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

        /// <summary>
        /// Calculate a positive pseudo-random number from two integers. This number should be positive.
        /// </summary>
        private static int GetPseudoRandomInt(int x, int y) {
            // We XOR them together with a large prime number.
            // Then we need ensure that we have a positive number.
            // Is this a good implementation for this function, or is there a better option possible?
            int result1 = x ^ y ^ 2147483647;
            return Mathf.Abs(result1 ^ int.MinValue);
        }
    }
}