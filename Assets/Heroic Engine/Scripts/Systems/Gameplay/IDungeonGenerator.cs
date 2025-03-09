using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public interface IDungeonGenerator : ISystem
    {
        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <returns>List of dungeon rooms</returns>
        List<DungeonRoom> GenerateDungeon(Vector3 pos);

        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector,
        /// but with custom width and length
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        /// <returns>List of dungeon rooms</returns>
        List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length);

        /// <summary>
        /// This method generates dungeon with custom settings
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        /// <param name="enableWalls">Enable walls?</param>
        /// <param name="minRoomSize">Min room size</param>
        /// <param name="roomMargin">Margin between rooms</param>
        /// <param name="corridorWidth">Corridors width</param>
        /// <param name="maxIterations">Max number of rooms splitting iterations</param>
        /// <returns>List of dungeon rooms</returns>
        List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length, bool enableWalls = true, int minRoomSize = 5, int roomMargin = 2, int corridorWidth = 2, int maxIterations = 5);

        /// <summary>
        /// This method clears current dungeon
        /// </summary>
        void ClearDungeon();

        /// <summary>
        /// This method generates NavMesh for current dungeon
        /// </summary>
        void GenerateNavMesh();
    }
}