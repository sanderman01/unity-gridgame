using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AmarokGames.Grids {

    /// <summary>
    /// Manages chunk collision components.
    /// </summary>
    public class GridCollisionSystem {

        private LayerId solidLayer;
        private Dictionary<ChunkKey, ChunkCollidersEntry> chunksColliders = new Dictionary<ChunkKey, ChunkCollidersEntry>();
        private LayerId layerId;

        private class ChunkCollidersEntry {
            public int LastModified;
            public GameObject chunkColliderObject;
            public List<BoxCollider2D> colliders = new List<BoxCollider2D>();
        }

        public GridCollisionSystem(LayerId layerId) {
            this.layerId = layerId;
        }

        public void Update(Grid2D grid) {
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
                    BooleanBuffer solidBuffer = (BooleanBuffer)data.GetBuffer(solidLayer);

                    ChunkCollidersEntry chunkColliders;
                    if(chunksColliders.TryGetValue(new ChunkKey(grid.GridId, chunkCoord), out chunkColliders)) {
                        // We found existing colliders for this chunk

                        // Check if we need to update them or if they are still up-to-date.
                        if (solidBuffer.LastModified > chunkColliders.LastModified) {
                            UpdateColliders(grid, chunkCoord, solidBuffer, chunkColliders);
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
                            Rigidbody2D rigidbody = chunkColliders.chunkColliderObject.AddComponent<Rigidbody2D>();
                            rigidbody.isKinematic = true;
                            rigidbody.simulated = true; // ???

                            chunksColliders.Add(new ChunkKey(grid.GridId, chunkCoord), chunkColliders);
                            UpdateColliders(grid, chunkCoord, solidBuffer, chunkColliders);
                        } else {
                            // No chunk gameobject exists? Wha?
                        }
                    }
                }
            }
        }

        private void UpdateColliders(Grid2D grid, Int2 chunkCoord, BooleanBuffer solidBuffer, ChunkCollidersEntry chunkCollidersEntry) {

            GameObject colliderGameObject = chunkCollidersEntry.chunkColliderObject;
            if(colliderGameObject != null) {
                // Remove any existing colliders
                foreach (BoxCollider2D collider in chunkCollidersEntry.colliders) {
                    UnityEngine.Object.Destroy(collider);
                }
                chunkCollidersEntry.colliders.Clear();

                // Do row scan to determine all the rectangles we want to create box colliders for
                List<Rect> rects = new List<Rect>();

                bool makingRect;
                Rect currentRect = new Rect();
                for(int y = 0; y < grid.ChunkHeight; ++y) {
                    makingRect = false;
                    for(int x = 0; x < grid.ChunkWidth; ++x) {
                        Int2 localCoord = new Int2(x, y);
                        int cellIndex = Grid2D.GetCellIndex(localCoord, grid.ChunkWidth);
                        bool solid = solidBuffer.GetValue(cellIndex);

                        if(solid && !makingRect) {
                            // Start a new rect
                            makingRect = true;
                            currentRect.yMin = y;
                            currentRect.yMax = y + 1;
                            currentRect.xMin = x;
                        } else if(!solid && makingRect) {
                            // finish the current rect
                            makingRect = false;
                            currentRect.xMax = x;
                            rects.Add(currentRect);
                        }
                    }
                    
                    if(makingRect) {
                        // finish the current rect
                        makingRect = false;
                        currentRect.xMax = grid.ChunkWidth;
                        rects.Add(currentRect);
                    }
                }

                for(int i = rects.Count - 1; i > 0; --i) {
                    Rect a = rects[i];
                    Rect b = rects[i - 1];
                    if(a.xMin == b.xMin && a.xMax == b.xMax)
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
                        rects.Insert(i-1, newRect);
                    }
                }
                
                // Get the game object, and create all the required box colliders.
                foreach(Rect rect in rects) {
                    BoxCollider2D col = colliderGameObject.AddComponent<BoxCollider2D>();
                    col.size = rect.size;
                    //col.offset = rect.position; // + 0.5f * Vector2.one;
                    col.offset = rect.center;
                    chunkCollidersEntry.colliders.Add(col);
                }

            } else {
                // The chunk did not exist for some reason
                // Clean lingering entries in our bookkeeping.
                chunksColliders.Remove(new ChunkKey(grid.GridId, chunkCoord));
            }
        }
    }
}