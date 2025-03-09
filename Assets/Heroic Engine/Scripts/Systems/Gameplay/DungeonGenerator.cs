using HeroicEngine.Systems.DI;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public class DungeonRoom
    {
        public int X, Y, Width, Height;
        public List<DungeonRoom> ConnectedRooms;

        public DungeonRoom(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            ConnectedRooms = new List<DungeonRoom>();
        }
    }

    public class DungeonGenerator : SystemBase
    {
        [SerializeField] private List<GameObject> floorTilePrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> wallTilePrefabs = new List<GameObject>();
        [SerializeField] private bool enableWalls = true;
        [SerializeField] private bool generateAtStart = true;

        [Header("Dungeon Settings")]
        [SerializeField] private int dungeonWidth = 50;
        [SerializeField] private int dungeonLength = 50;
        [SerializeField] private int minRoomSize = 6;
        [SerializeField] private int maxIterations = 5;
        [SerializeField] private int roomMargin = 2; // Space to leave between rooms for corridors
        [SerializeField] private int corridorWidth = 2;

        private List<DungeonRoom> rooms = new List<DungeonRoom>();
        private List<GameObject> floorTiles = new List<GameObject>();
        private List<GameObject> wallTiles = new List<GameObject>();
        private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        private NavMeshSurface navMeshSurface;

        [ContextMenu("Generate Dungeon")]
        public void GenerateDungeon()
        {
            GenerateDungeon(Vector3.zero);
        }

        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector
        /// </summary>
        /// <param name="pos">Start position</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos)
        {
            return GenerateDungeon(pos, dungeonWidth, dungeonLength, enableWalls, minRoomSize, corridorWidth, maxIterations);
        }

        /// <summary>
        /// This method generates dungeon with default settings set in DungeonGenerator inspector,
        /// but with custom width and length
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length)
        {
            return GenerateDungeon(pos, width, length, enableWalls, minRoomSize, corridorWidth, maxIterations);
        }

        /// <summary>
        /// This method generates dungeon with custom settings
        /// </summary>
        /// <param name="pos">Start position</param>
        /// <param name="width">Width of dungeon</param>
        /// <param name="length">Length of dungeon</param>
        /// <param name="enableWalls">Enable walls?</param>
        /// <param name="minRoomSize">Min room size</param>
        /// <param name="roomMargin">Margin between rooms</param>
        /// <param name="maxIterations">Max number of rooms splitting iterations</param>
        public List<DungeonRoom> GenerateDungeon(Vector3 pos, int width, int length, bool enableWalls = true, int minRoomSize = 5, int roomMargin = 2, int corridorWidth = 2, int maxIterations = 5, bool generateNavMesh = true)
        {
            dungeonWidth = width;
            dungeonLength = length;
            this.minRoomSize = minRoomSize;
            this.maxIterations = maxIterations;
            this.enableWalls = enableWalls;
            this.roomMargin = roomMargin;
            this.corridorWidth = corridorWidth;

            ClearDungeon();

            DungeonRoom rootRoom = new DungeonRoom((int)pos.x, (int)pos.z, dungeonWidth, dungeonLength);
            SplitRoom(rootRoom, 0);

            foreach (DungeonRoom room in rooms)
            {
                CreateFloor(room);
            }

            for (int i = 0; i < rooms.Count - 1; i++)
            {
                CreateCorridor(rooms[i], rooms[i + 1]);
            }

            if (enableWalls)
            {
                CreateWalls();
            }

            if (generateNavMesh)
            {
                GenerateNavMesh();
            }

            return rooms;
        }

        /// <summary>
        /// This method clears current dungeon
        /// </summary>
        public void ClearDungeon()
        {
            rooms.Clear();
            foreach (var floor in floorTiles.ToArray())
            {
                Destroy(floor);
            }
            foreach (var wall in wallTiles.ToArray())
            {
                Destroy(wall);
            }
            floorPositions.Clear();
            floorTiles.Clear();
            wallTiles.Clear();
        }

        /// <summary>
        /// This method generates NavMesh for current dungeon
        /// </summary>
        public void GenerateNavMesh()
        {
            if (!gameObject.TryGetComponent(out NavMeshSurface _))
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }

            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.BuildNavMesh();
        }

        private void SplitRoom(DungeonRoom room, int iteration)
        {
            if (iteration >= maxIterations || (room.Width <= minRoomSize + roomMargin * 2 && room.Height <= minRoomSize + roomMargin * 2))
            {
                // Reduce room size to leave a margin
                int roomX = room.X + roomMargin;
                int roomY = room.Y + roomMargin;
                int roomWidth = room.Width - roomMargin * 2;
                int roomHeight = room.Height - roomMargin * 2;

                if (roomWidth > 0 && roomHeight > 0)
                {
                    rooms.Add(new DungeonRoom(roomX, roomY, roomWidth, roomHeight));
                }

                return;
            }

            bool splitHorizontal = Random.Range(0, 2) == 0;

            if (room.Width > room.Height && room.Width / 2 >= minRoomSize + roomMargin * 2) splitHorizontal = false;
            else if (room.Height > room.Width && room.Height / 2 >= minRoomSize + roomMargin * 2) splitHorizontal = true;

            if (splitHorizontal)
            {
                int split = Random.Range(minRoomSize + roomMargin, room.Height - (minRoomSize + roomMargin));
                DungeonRoom topRoom = new DungeonRoom(room.X, room.Y, room.Width, split);
                DungeonRoom bottomRoom = new DungeonRoom(room.X, room.Y + split, room.Width, room.Height - split);
                SplitRoom(topRoom, iteration + 1);
                SplitRoom(bottomRoom, iteration + 1);
            }
            else
            {
                int split = Random.Range(minRoomSize + roomMargin, room.Width - (minRoomSize + roomMargin));
                DungeonRoom leftRoom = new DungeonRoom(room.X, room.Y, split, room.Height);
                DungeonRoom rightRoom = new DungeonRoom(room.X + split, room.Y, room.Width - split, room.Height);
                SplitRoom(leftRoom, iteration + 1);
                SplitRoom(rightRoom, iteration + 1);
            }
        }

        private void CreateCorridor(DungeonRoom roomA, DungeonRoom roomB)
        {
            roomA.ConnectedRooms.Add(roomB);
            roomB.ConnectedRooms.Add(roomA);

            Vector2Int centerA = new Vector2Int(roomA.X + roomA.Width / 2, roomA.Y + roomA.Height / 2);
            Vector2Int centerB = new Vector2Int(roomB.X + roomB.Width / 2, roomB.Y + roomB.Height / 2);

            if (Random.Range(0, 2) == 0)
            {
                CreateFloor(new DungeonRoom(centerA.x, centerA.y, Mathf.Abs(centerB.x - centerA.x) + 1, corridorWidth));
                CreateFloor(new DungeonRoom(centerB.x, Mathf.Min(centerA.y, centerB.y), corridorWidth, Mathf.Abs(centerB.y - centerA.y) + 1));
            }
            else
            {
                CreateFloor(new DungeonRoom(Mathf.Min(centerA.x, centerB.x), centerA.y, Mathf.Abs(centerB.x - centerA.x) + 1, corridorWidth));
                CreateFloor(new DungeonRoom(centerB.x, centerB.y, corridorWidth, Mathf.Abs(centerB.y - centerA.y) + 1));
            }
        }

        private void CreateFloor(DungeonRoom room)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    Vector2Int tilePos = new Vector2Int(x, y);
                    if (!floorPositions.Contains(tilePos))
                    {
                        floorTiles.Add(Instantiate(floorTilePrefabs.GetRandomElement(), new Vector3(x, 0, y), Quaternion.identity, transform));
                        floorPositions.Add(tilePos);
                    }
                }
            }
        }

        private void CreateWalls()
        {
            foreach (Vector2Int position in floorPositions.ToArray())
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dz) == 0) continue;

                        Vector2Int neighborPosition = position + new Vector2Int(dx, dz);

                        if (!floorPositions.Contains(neighborPosition))
                        {
                            wallTiles.Add(Instantiate(wallTilePrefabs.GetRandomElement(), new Vector3(neighborPosition.x, 0, neighborPosition.y), Quaternion.identity, transform));
                            floorPositions.Add(neighborPosition);
                        }
                    }
                }
            }
        }

        private void Start()
        {
            if (generateAtStart)
            {
                GenerateDungeon(Vector3.zero, dungeonWidth, dungeonLength, true, minRoomSize, roomMargin, maxIterations);
            }
        }
    }
}