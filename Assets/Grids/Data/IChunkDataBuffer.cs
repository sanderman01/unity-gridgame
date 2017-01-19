// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public interface IDataBuffer {

        BufferType Type { get; }
        int Length { get; }
    }
}
