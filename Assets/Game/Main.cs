// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AmarokGames.GridGame {

    public class Main : MonoBehaviour {
        [SerializeField]
        private Int2 worldSize = new Int2(1024, 1024);
        [SerializeField]
        private Int2 worldChunkSize = new Int2(64, 64);

        private TileRegistry tileRegistry;
        private World world;

        private List<IGameSystem> gameSystems = new List<IGameSystem>();

        private LayerConfig layers;

        BaseGameMod baseGameMod;

        public void Start() {

            layers = new LayerConfig();
            tileRegistry = new TileRegistry();

            baseGameMod = new BaseGameMod();
            baseGameMod.Init(ref layers, tileRegistry, gameSystems);

            tileRegistry.Finalise();

            baseGameMod.PostInit(tileRegistry, gameSystems);

            CreateWorld(0);
        }

        private void CreateWorld(int seed) {
            WorldGenerator worldGen = baseGameMod.GetWorldGenerator();
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, layers, worldGen);
            world.WorldGenerator.Init(world);
        }

        void Update() {
            foreach (IGameSystem system in gameSystems) {
                if (system.Enabled) system.UpdateWorld(world, Time.deltaTime);
            }
        }
    }
}

