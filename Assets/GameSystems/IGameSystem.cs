// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;

namespace AmarokGames.GridGame {

    public interface IGameSystem {

        /// <summary>
        /// Determines wether the system is active.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets called a fixed number of times per second.
        /// Use this for things that need to happen on a regular interval, but not every frame.
        /// </summary>
        void TickWorld(World world, int tickRate);

        /// <summary>
        /// Gets called every single frame.
        /// Use this for processes that need to happen continuously, like e.g. animation.
        /// </summary>
        void UpdateWorld(World world, float deltaTime);

        void OnWorldCreated(World world, TileRegistry registry);
    }
}
