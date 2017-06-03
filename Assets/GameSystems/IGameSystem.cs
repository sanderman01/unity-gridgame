using AmarokGames.GridGame;
using AmarokGames.Grids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameSystem {

    void Update(World world, IEnumerable<Grid2D> grids);
}
