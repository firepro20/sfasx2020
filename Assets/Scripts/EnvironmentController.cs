using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] private List<EnvironmentTile> AccessibleTiles;
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles;
    [SerializeField] public List<EnvironmentTile> BaseTiles;
    [SerializeField] private Vector2Int Size;
    [SerializeField] private float AccessiblePercentage;

    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; // 9 
    private const float TileSize = 9.0f; //  10.0f
    private const float TileHeight = -10.0f; //2.5f
    // used to offset z of mMap
    private const float tileOffset = 20.0f; // 20.0f

    

    // This has been commented out as the player will choose start position
    public EnvironmentTile Start { get; private set; }

    // Might remove later
    public EnvironmentTile playerBase { get; private set; } // player base
    
    public EnvironmentTile enemyBase { get; private set; }
    
    

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }

    private void OnDrawGizmos() // can also create method to actually draw green tile on hover, to show where items can be placed
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mMap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private void Generate()
    {
        
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        // placement of first block affects placement of whole grid
        Vector3 position = new Vector3( -(halfWidth * TileSize), -10.0f, -(halfHeight * TileSize) + tileOffset);
        bool start = true;
        // A check to test if tile to place enemy base on is actually accessible
        bool enemyBaseAccessible = false;

        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for ( int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = start || Random.value < AccessiblePercentage; // boolean OR operator true OR false / true depending on value
                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles; // Set the Accessible / Inaccessible property here
                EnvironmentTile prefab = tiles[Random.Range(0, tiles.Count)]; // depending on whether the list contains accessible or not accessible,
                //EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);
                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.AngleAxis(180.0f, transform.up), transform);
                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                tile.MapPosition.x = x;
                tile.MapPosition.y = y;
                mMap[x][y] = tile;
                mAll.Add(tile);

                // This only happens oce then start is set to false
                // Might need to change this to spawn two bases
                if(start)
                {
                    Start = tile;
                }

                position.z += TileSize;
                start = false;
            }

            position.x += TileSize;
            position.z = -(halfHeight * TileSize) + tileOffset;
        }

        while (!enemyBaseAccessible)
        {
            
            int enemyBaseX = Random.Range(halfWidth, Size.x);
            int enemyBaseY = Random.Range(0, Size.y);
            if (mMap[enemyBaseX][enemyBaseY].IsAccessible)
            {
                
                Transform[] children = mMap[enemyBaseX][enemyBaseY].gameObject.GetComponentsInChildren<Transform>();

                int i;
                for (i = 0; i < children.Length; i++)
                {
                    //Debug.LogWarning("This is " + i + " - " + children[i].gameObject.name);
                    
                    if (i > 0)
                    {
                        //Debug.LogWarning(children[i].gameObject.name);
                        DestroyImmediate(children[i].gameObject);
                    }

                }

                EnvironmentTile prefab = BaseTiles[0];
                EnvironmentTile tile = Instantiate(prefab, children[0].gameObject.transform.position, Quaternion.AngleAxis(180.0f, transform.up), transform);
                tile.transform.SetParent(children[0].gameObject.transform);
                tile.Position = mMap[enemyBaseX][enemyBaseY].Position;
                tile.MapPosition.x = enemyBaseX;
                tile.MapPosition.y = enemyBaseY;
                
                enemyBase = tile;
                if (enemyBase.GetComponent<EnvironmentTile>())
                {
                    Debug.Log("Managed to get Environment Tile!");
                }
                else
                {
                    Debug.Log("Unsuccessful!");
                }
                
                Transform[] childrenAfterDeletion = mMap[enemyBaseX][enemyBaseY].gameObject.GetComponentsInChildren<Transform>();
                //Destroy(childrenAfterDeletion[1].gameObject.GetComponent<EnvironmentTile>()); // problem identified destroying gameobject enemy tile
                
                enemyBaseAccessible = true;
            }
            else
            {
                Debug.LogError("Encountered inAccessbile tile!");
            }
        }
    }

    public Vector2Int SizeOfMap()
    {
        Vector2Int sizeOfMap = Size;
        return sizeOfMap;
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.Connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public bool isEmpty()
    {
        if (mMap != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();
                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }
}
