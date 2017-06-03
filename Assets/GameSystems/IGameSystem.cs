// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using AmarokGames.Grids;
using System.Collections.Generic;

namespace AmarokGames.GridGame {

    public interface IGameSystem {

        void Update(World world, IEnumerable<Grid2D> grids);
    }
}
