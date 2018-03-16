// Copyright(C) 2018, Alexander Verbeek

using AmarokGames.Grids;
using AmarokGames.Grids.Data;

namespace AmarokGames.GridGame
{
    public class GridTileBackgroundRenderSystem : GridTileRenderSystem
    {
        new public static GridTileBackgroundRenderSystem Create(LayerId layerId, float zPos)
        {
            GridTileBackgroundRenderSystem sys = Create<GridTileBackgroundRenderSystem>();
            sys.layerId = layerId;
            sys.zOffsetGlobal = zPos;
            return sys;
        }
    }
}