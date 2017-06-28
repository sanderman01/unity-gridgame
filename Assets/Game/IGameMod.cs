// Copyright(C) 2017 Amarok Games, Alexander Verbeek

namespace AmarokGames.GridGame {

    /// <summary>
    /// Interface to be implemented by game mods.
    /// A mod is a package of 'stuff' like additional game subsystems and content to be added to the game.
    /// </summary>
    public interface IGameMod {

        /// <summary>
        /// Called at an early stage of the mods loading process.
        /// Use this to register new systems, tiles, items and other content in the registry.
        /// </summary>
        void PreInit(Main game, GameRegistry gameRegistry);

        /// <summary>
        /// Called during the middle stage of the loading process.
        /// Use this to gain access to other Mods' systems or content and to do various types of initialization.
        /// </summary>
        void Init(Main game, GameRegistry gameRegistry);

        /// <summary>
        /// Called an end stage of the loading process.
        /// Use this for any processes that need to happen after all systems have already been initialized.
        /// </summary>
        void PostInit(Main game, GameRegistry gameRegistry);
    }
}
