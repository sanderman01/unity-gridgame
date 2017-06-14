// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace AmarokGames.GridGame {

    public class Main : MonoBehaviour {
        [SerializeField]
        private Int2 worldSize = new Int2(1024, 1024);
        [SerializeField]
        private Int2 worldChunkSize = new Int2(64, 64);

        private TileRegistry tileRegistry;
        private World world;
        public World World;

        private List<IGameSystem> gameSystems = new List<IGameSystem>();
        private Dictionary<Type, List<IGameSystem>> gameSystemsByType = new Dictionary<Type, List<IGameSystem>>();

        private LayerConfig layers;

        BaseGameMod baseGameMod;

        public void Start() {

            layers = new LayerConfig();
            tileRegistry = new TileRegistry();

            baseGameMod = new BaseGameMod();
            baseGameMod.Init(this, ref layers, tileRegistry);

            tileRegistry.Finalise();

            baseGameMod.PostInit(this, tileRegistry);

            CreateWorld(0);
        }

        private void CreateWorld(int seed) {
            WorldGenerator worldGen = baseGameMod.GetWorldGenerator(tileRegistry);
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, layers, worldGen);
            world.WorldGenerator.Init(world);

            foreach (IGameSystem system in gameSystems) system.OnWorldCreated(world, tileRegistry);
        }

        void Update() {
            foreach (IGameSystem system in gameSystems) {
                if (system.Enabled) {
                    UnityEngine.Profiling.Profiler.BeginSample(system.GetType().Name + ".UpdateWorld");
                    system.UpdateWorld(world, Time.deltaTime);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        public IEnumerable<IGameSystem> GetSystems() {
            return gameSystems;
        }

        public IGameSystem GetSystem(Type type) {
            return gameSystemsByType[type].First();
        }

        public void AddSystem(IGameSystem system) {
            gameSystems.Add(system);
            Type type = system.GetType();
            List<IGameSystem> list;
            if(!gameSystemsByType.TryGetValue(type, out list)) {
                list = new List<IGameSystem>();
                gameSystemsByType.Add(type, list);
            }
            list.Add(system);
        }
    }
}

