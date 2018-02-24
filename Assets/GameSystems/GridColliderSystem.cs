// Copyright(C) 2018, Alexander Verbeek

using AmarokGames.GridGame;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace AmarokGames.Grids
{
    /// <summary>
    /// Manages chunk collider components.
    /// </summary>
    public class GridColliderSystem : GameSystemBase, IGameSystem
    {
        private class ChunkCollidersEntry
        {
            public int LastModified;
            public GameObject chunkColliderObject;
            public List<BoxCollider2D> colliders = new List<BoxCollider2D>();
        }

        private LayerId _solidLayer;
        private Dictionary<ChunkKey, ChunkCollidersEntry> _chunksColliders = new Dictionary<ChunkKey, ChunkCollidersEntry>();

        private ColliderGeneratorQT _colliderGenerator = new ColliderGeneratorQT();

        protected override void Enable()
        {
        }

        protected override void Disable()
        {
            foreach (ChunkCollidersEntry entry in _chunksColliders.Values)
            {
                UnityEngine.Object.Destroy(entry.chunkColliderObject);
                entry.colliders.Clear();
            }
            _chunksColliders.Clear();
        }

        public static GridColliderSystem Create(LayerId layerId)
        {
            GridColliderSystem sys = Create<GridColliderSystem>();
            sys._solidLayer = layerId;
            return sys;
        }

        public override void TickWorld(World world, int tickRate)
        {
        }

        public override void UpdateWorld(World world, float deltaTime)
        {
            foreach (Grid2D grid in world.Grids)
            {
                UpdateGrid(world, grid);
            }
        }

        private void UpdateGrid(World world, Grid2D grid)
        {
            Profiler.BeginSample("UpdateGrid");

            // For each chunk
            // Check that the chunk is still up to date
            // If not, then regenerate the colliders for this chunk
            // Use the Row-Scan method to find sub rectangles as described here:
            // https://gamedev.stackexchange.com/questions/129648/algorithm-for-healing-multiple-rectangles-into-a-smaller-number-of-rectangles/129651#129651

            // If the chunk did not yet exist, then we should create a component to hold the collision stuff

            // For every chunk in the grid
            foreach (Int2 chunkCoord in grid.GetAllChunks())
            {

                // Get the solid buffer. This buffer tracks whether cells contain solid tiles or not.
                ChunkData data = null;
                if (grid.TryGetChunkData(chunkCoord, out data))
                {
                    BitBuffer solidBuffer = (BitBuffer)data.GetBuffer(_solidLayer);

                    ChunkCollidersEntry chunkColliders;
                    if (_chunksColliders.TryGetValue(new ChunkKey(world.WorldId, grid.GridId, chunkCoord), out chunkColliders))
                    {
                        // We found existing colliders for this chunk

                        // Check if we need to update them or if they are still up-to-date.
                        if (solidBuffer.LastModified > chunkColliders.LastModified)
                        {
                            UpdateChunkColliders(world.WorldId, grid, chunkCoord, solidBuffer, chunkColliders);
                            chunkColliders.LastModified = Time.frameCount;
                        }
                    }
                    else
                    {
                        // We did not find any existing colliders for this chunk.
                        // Create a gameObject to hold the colliders, and add an entry to the bookkeeping.
                        Grid2DChunk chunk = null;
                        if (grid.TryGetChunkObject(chunkCoord, out chunk))
                        {
                            chunkColliders = new ChunkCollidersEntry();
                            chunkColliders.chunkColliderObject = new GameObject(string.Format("chunk {0} colliders", chunkCoord));
                            chunkColliders.chunkColliderObject.transform.SetParent(chunk.gameObject.transform, false);

                            _chunksColliders.Add(new ChunkKey(world.WorldId, grid.GridId, chunkCoord), chunkColliders);
                            UpdateChunkColliders(world.WorldId, grid, chunkCoord, solidBuffer, chunkColliders);
                        }
                        else
                        {
                            // No chunk gameobject exists? Wha?
                        }
                    }
                }
            }
            Profiler.EndSample();
        }

        private void UpdateChunkColliders(int worldId, Grid2D grid, Int2 chunkCoord, BitBuffer solidBuffer, ChunkCollidersEntry chunkCollidersEntry)
        {

            Profiler.BeginSample("UpdateChunkColliders");
            GameObject colliderGameObject = chunkCollidersEntry.chunkColliderObject;
            if (colliderGameObject != null)
            {
                // Remove any existing colliders
                foreach (BoxCollider2D collider in chunkCollidersEntry.colliders)
                {
                    UnityEngine.Object.Destroy(collider);
                }
                chunkCollidersEntry.colliders.Clear();

                // Calculate desired collider rectangles.
                List<Rect> rects = _colliderGenerator.GetRects(solidBuffer);

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
            else
            {
                // The chunk did not exist for some reason
                // Clean lingering entries in our bookkeeping.
                _chunksColliders.Remove(new ChunkKey(worldId, grid.GridId, chunkCoord));
            }
            Profiler.EndSample();
        }
    }

    /// <summary>
    /// Calculates collider rectangles from a tile BitBuffer using a RegionQuadTree approach.
    /// </summary>
    class ColliderGeneratorQT
    {
        private List<Rect> _rects = new List<Rect>(Grid2D.ChunkWidth * Grid2D.ChunkWidth);

        public enum NodeType { Empty, Solid, Partial }

        /// <summary>
        /// Returns a list of rectangles based on the provided buffer, which can be used to create the required colliders. 
        /// This object will remain the owner of that list and the list will be re-used on subsequent calls to this method.
        /// </summary>
        public List<Rect> GetRects(BitBuffer solidBuffer)
        {
            Profiler.BeginSample("QuadTreeColliderGenerator.GetRects");
            _rects.Clear();
            NodeType node = RenderTree(_rects, solidBuffer, 0, 0, Grid2D.ChunkWidth);
            if (node == NodeType.Solid) _rects.Add(new Rect(0, 0, Grid2D.ChunkWidth, Grid2D.ChunkWidth));
            Profiler.EndSample();
            return _rects;
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

                // Determine which sub-sections are solid.
                bool ab = childA == NodeType.Solid && childB == NodeType.Solid;
                bool cd = childC == NodeType.Solid && childD == NodeType.Solid;
                bool ac = childA == NodeType.Solid && childC == NodeType.Solid;
                bool bd = childB == NodeType.Solid && childD == NodeType.Solid;

                if (ab && cd)
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
                    if (ab)
                    {
                        results.Add(new Rect(minX, minY, childWidth + childWidth, childWidth));
                        childA = childB = NodeType.Empty;
                    }
                    else if (cd)
                    {
                        results.Add(new Rect(minX, minY + childWidth, childWidth + childWidth, childWidth));
                        childC = childD = NodeType.Empty;
                    }
                    else if (ac)
                    {
                        results.Add(new Rect(minX, minY, childWidth, childWidth + childWidth));
                        childA = childC = NodeType.Empty;
                    }
                    else if (bd)
                    {
                        results.Add(new Rect(minX + childWidth, minY, childWidth, childWidth + childWidth));
                        childB = childD = NodeType.Empty;
                    }

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