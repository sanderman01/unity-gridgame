// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.GridGame;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Profiling;

namespace AmarokGames.Grids {

    /// <summary>
    /// Manages chunk collision components.
    /// </summary>
    public class GridCollisionSystem : GameSystemBase, IGameSystem {

        private class ChunkCollidersEntry {
            public int LastModified;
            public GameObject chunkColliderObject;
            public List<BoxCollider2D> colliders = new List<BoxCollider2D>();
        }

        private LayerId solidLayer;
        private Dictionary<ChunkKey, ChunkCollidersEntry> chunksColliders = new Dictionary<ChunkKey, ChunkCollidersEntry>();

        private QuadTreeColliderGenerator colliderGenerator = new QuadTreeColliderGenerator();

        protected override void Enable() {
        }

        protected override void Disable() {
            foreach(ChunkCollidersEntry entry in chunksColliders.Values) {
                UnityEngine.Object.Destroy(entry.chunkColliderObject);
                entry.colliders.Clear();
            }
            chunksColliders.Clear();
        }

        public static GridCollisionSystem Create(LayerId layerId) {
            GridCollisionSystem sys = Create<GridCollisionSystem>();
            sys.solidLayer = layerId;
            return sys;
        }

        public override void TickWorld(World world, int tickRate) {
        }

        public override void UpdateWorld(World world, float deltaTime) {
            foreach(Grid2D grid in world.Grids) {
                UpdateGrid(world, grid);
            }
        }

        private void UpdateGrid(World world, Grid2D grid) {
            Profiler.BeginSample("UpdateGrid");

            // For each chunk
            // Check that the chunk is still up to date
            // If not, then regenerate the colliders for this chunk
            // Use the Row-Scan method to find sub rectangles as described here:
            // https://gamedev.stackexchange.com/questions/129648/algorithm-for-healing-multiple-rectangles-into-a-smaller-number-of-rectangles/129651#129651

            // If the chunk did not yet exist, then we should create a component to hold the collision stuff

            // For every chunk in the grid
            foreach(Int2 chunkCoord in grid.GetAllChunks()) {

                // Get the solid buffer. This buffer tracks whether cells contain solid tiles or not.
                ChunkData data = null;
                if(grid.TryGetChunkData(chunkCoord, out data)) {
                    BitBuffer solidBuffer = (BitBuffer)data.GetBuffer(solidLayer);

                    ChunkCollidersEntry chunkColliders;
                    if(chunksColliders.TryGetValue(new ChunkKey(world.WorldId, grid.GridId, chunkCoord), out chunkColliders)) {
                        // We found existing colliders for this chunk

                        // Check if we need to update them or if they are still up-to-date.
                        if (solidBuffer.LastModified > chunkColliders.LastModified) {
                            UpdateColliders(world.WorldId, grid, chunkCoord, solidBuffer, chunkColliders);
                            chunkColliders.LastModified = Time.frameCount;
                        }
                    }
                    else {
                        // We did not find any existing colliders for this chunk.
                        // Create a gameObject to hold the colliders, and add an entry to the bookkeeping.
                        Grid2DChunk chunk = null;
                        if(grid.TryGetChunkObject(chunkCoord, out chunk)) {
                            chunkColliders = new ChunkCollidersEntry();
                            chunkColliders.chunkColliderObject = new GameObject(string.Format("chunk {0} colliders", chunkCoord));
                            chunkColliders.chunkColliderObject.transform.SetParent(chunk.gameObject.transform, false);

                            chunksColliders.Add(new ChunkKey(world.WorldId, grid.GridId, chunkCoord), chunkColliders);
                            UpdateColliders(world.WorldId, grid, chunkCoord, solidBuffer, chunkColliders);
                        } else {
                            // No chunk gameobject exists? Wha?
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        private void UpdateColliders(int worldId, Grid2D grid, Int2 chunkCoord, BitBuffer solidBuffer, ChunkCollidersEntry chunkCollidersEntry) {

            Profiler.BeginSample("UpdateColliders");
            GameObject colliderGameObject = chunkCollidersEntry.chunkColliderObject;
            if(colliderGameObject != null)
            {
                // Remove any existing colliders
                foreach (BoxCollider2D collider in chunkCollidersEntry.colliders)
                {
                    UnityEngine.Object.Destroy(collider);
                }
                chunkCollidersEntry.colliders.Clear();

                // Calculate desired collider rectangles.
                List<Rect> rects = colliderGenerator.GenerateColliderRects(solidBuffer);

                // Get the game object, and create all the required box colliders.
                foreach (Rect rect in rects)
                {
                    BoxCollider2D col = colliderGameObject.AddComponent<BoxCollider2D>();
                    col.size = rect.size;
                    //col.offset = rect.position; // + 0.5f * Vector2.one;
                    col.offset = rect.center;
                    chunkCollidersEntry.colliders.Add(col);
                }

            }
            else {
                // The chunk did not exist for some reason
                // Clean lingering entries in our bookkeeping.
                chunksColliders.Remove(new ChunkKey(worldId, grid.GridId, chunkCoord));
            }
            Profiler.EndSample();
        }


    }

    /// <summary>
    /// This generates collider rectangles by scanning along rows in an attempt to merge collider rectangles.
    /// </summary>
    public class RowScanColliderGen
    {
        public static List<Rect> GenerateColliders(BitBuffer solidBuffer)
        {
            Profiler.BeginSample("GenerateColliders");
            List<Rect> rects = new List<Rect>();
            bool makingRect;
            Rect currentRect = new Rect();
            for (int y = 0; y < Grid2D.ChunkHeight; ++y)
            {
                makingRect = false;
                for (int x = 0; x < Grid2D.ChunkWidth; ++x)
                {
                    Int2 localCoord = new Int2(x, y);
                    int cellIndex = Grid2D.GetChunkCellIndex(localCoord, Grid2D.ChunkWidth);
                    bool solid = solidBuffer.GetValue(cellIndex);

                    if (solid && !makingRect)
                    {
                        // Start a new rect
                        makingRect = true;
                        currentRect.yMin = y;
                        currentRect.yMax = y + 1;
                        currentRect.xMin = x;
                    }
                    else if (!solid && makingRect)
                    {
                        // finish the current rect
                        makingRect = false;
                        currentRect.xMax = x;
                        rects.Add(currentRect);
                    }
                }

                if (makingRect)
                {
                    // finish the current rect
                    makingRect = false;
                    currentRect.xMax = Grid2D.ChunkWidth;
                    rects.Add(currentRect);
                }
            }

            for (int i = rects.Count - 1; i > 0; --i)
            {
                Rect a = rects[i];
                Rect b = rects[i - 1];
                if (a.xMin == b.xMin && a.xMax == b.xMax)
                {
                    // Then merge them in to one larger rect
                    Rect newRect = new Rect();
                    newRect.xMin = a.xMin;
                    newRect.xMax = a.xMax;
                    newRect.yMin = b.yMin;
                    newRect.yMax = a.yMax;
                    // remove the old ones from the list
                    rects.RemoveAt(i);
                    rects.RemoveAt(i - 1);

                    // put the new one in the list
                    rects.Insert(i - 1, newRect);
                }
            }
            Profiler.EndSample();
            return rects;
        }
    }

    /// <summary>
    /// Calculates collider rectangles from a tile BitBuffer using a RegionQuadTree approach.
    /// </summary>
    public class QuadTreeColliderGenerator
    {
        private List<Rect> rects = new List<Rect>(Grid2D.ChunkWidth * Grid2D.ChunkWidth);

        public enum NodeType { Empty, Solid, Partial }

        public List<Rect> GenerateColliderRects(BitBuffer solidBuffer)
        {
            Profiler.BeginSample("ImplicitQuadTreeColliderGen.GenerateColliders");
            rects.Clear();
            NodeType node = RenderTree(rects, solidBuffer, 0, 0, Grid2D.ChunkWidth);
            if (node == NodeType.Solid) rects.Add(new Rect(0, 0, Grid2D.ChunkWidth, Grid2D.ChunkWidth));
            Profiler.EndSample();
            return rects;
        }

        /// <summary>
        /// Creates a tree of nodes inside the specified array. Nodes reference their children by index into the array.
        /// </summary>
        private static NodeType RenderTree(List<Rect> results, BitBuffer solidTileBuffer, int minX, int minY, int width)
        {
            if (width == 1)
            {
                // Base case.
                int index = Grid2D.GetChunkCellIndex(new Int2(minX, minY), Grid2D.ChunkWidth);
                bool solid = solidTileBuffer.GetValue(index);
                NodeType value = solid ? NodeType.Solid : NodeType.Empty;
                return value;
            }
            else
            {
                // Recursive case.
                int childWidth = width / 2;
                NodeType childA = RenderTree(results, solidTileBuffer, minX, minY, childWidth);
                NodeType childB = RenderTree(results, solidTileBuffer, minX + childWidth, minY, childWidth);
                NodeType childC = RenderTree(results, solidTileBuffer, minX, minY + childWidth, childWidth);
                NodeType childD = RenderTree(results, solidTileBuffer, minX + childWidth, minY + childWidth, childWidth);

                if (childA == NodeType.Solid && childB == NodeType.Solid
                    && childC == NodeType.Solid && childD == NodeType.Solid)
                {
                    return NodeType.Solid;
                }
                else if (childA == NodeType.Empty && childB == NodeType.Empty
                    && childC == NodeType.Empty && childD == NodeType.Empty)
                {
                    return NodeType.Empty;
                }
                else
                {
                    if (childA == NodeType.Solid) results.Add(new Rect(minX, minY, childWidth, childWidth));
                    if (childB == NodeType.Solid) results.Add(new Rect(minX + childWidth, minY, childWidth, childWidth));
                    if (childC == NodeType.Solid) results.Add(new Rect(minX, minY + childWidth, childWidth, childWidth));
                    if (childD == NodeType.Solid) results.Add(new Rect(minX + childWidth, minY + childWidth, childWidth, childWidth));
                    return NodeType.Partial;
                }
            }
        }
    }
}