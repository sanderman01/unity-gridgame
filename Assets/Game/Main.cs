// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Assertions;

namespace AmarokGames.GridGame {

    public class Main : MonoBehaviour {
        [SerializeField]
        private Int2 worldSize = new Int2(1024, 1024);
        [SerializeField]
        private Int2 worldChunkSize = new Int2(64, 64);

        private GameRegistry gameRegistry;
        private World world;
        public World World;

        private List<IGameSystem> gameSystems = new List<IGameSystem>();

        public LayerConfig Layers { get; private set; }

        BaseGameMod baseGameMod;

        private float lastTick;

        public void Start() {

            Layers = new LayerConfig();
            gameRegistry = new GameRegistry();

            baseGameMod = new BaseGameMod();
            baseGameMod.PreInit(this, gameRegistry);
            gameRegistry.Finalise();
            baseGameMod.Init(this, gameRegistry);
            baseGameMod.PostInit(this, gameRegistry);

            CreateWorld(0);
        }

        private void CreateWorld(int seed) {
            WorldGenerator worldGen = baseGameMod.GetWorldGenerator(gameRegistry);
            world = World.CreateWorld("world", 0, worldSize, worldChunkSize, Layers, worldGen);
            world.WorldGenerator.Init(world);

            foreach (IGameSystem system in gameSystems) system.OnWorldCreated(world, gameRegistry);
        }

        void Update() {
            foreach (IGameSystem system in gameSystems) {
                if (system.Enabled) {
                    UnityEngine.Profiling.Profiler.BeginSample(system.GetType().Name + ".UpdateWorld");
                    system.UpdateWorld(world, Time.deltaTime);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }

            const int tickRate = 20;
            const float targetTickTime = 1f / tickRate;
            if (Time.time - lastTick > targetTickTime) {
                lastTick = Time.time;
                foreach (IGameSystem system in gameSystems) {
                    if (system.Enabled) {
                        UnityEngine.Profiling.Profiler.BeginSample(system.GetType().Name + ".TickWorld");
                        system.TickWorld(world, tickRate);
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
            }
        }

        public T GetSystem<T>() where T : class, IGameSystem {
            T result = gameSystems.First(x => x is T) as T;
            Assert.IsNotNull(result);
            return result;
        }

        public void AddSystem(IGameSystem system) {
            gameSystems.Add(system);
        }
    }
}

