// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.Grids.Data {

    public interface IDataBuffer {

        BufferType Type { get; }
        int Length { get; }

        int LastModified { get; }
        void MarkModified(int frameCount);

        /// <summary>
        /// Generic method for getting a cell value in the buffer.
        /// This is of course less efficient than accessing the buffer directly, but is useful for convenience.
        /// </summary>
        object GetValue(int index);

        /// <summary>
        /// Generic method for setting a cell value in the buffer.
        /// This is of course less efficient than accessing the buffer directly, but is useful for convenience.
        /// </summary>
        void SetValue(int index, object value);
    }
}
