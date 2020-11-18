using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexGrid
{
    private Alignment alignment;
    private HashSet<Cube> hexes = new HashSet<Cube>();

    public HashSet<Cube> GenerateRectangularGrid(Alignment alignment, int width, int length)
    {
        if (alignment == Alignment.Horizontal)
        {
            for (int row = 0; row < length; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    AddHex(new Cube(col - Mathf.FloorToInt(row / 2f), row));
                }
            }
        }
        else
        {
            Debug.LogError("GenerateRectangularGrid() does not support vertical hex grids at this time.");
        }

        return hexes;
    }

    public enum Alignment
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Describes the alignment of the hex grid. A horizontally aligned hex grid 
    /// contains rows of horizontally aligned hexes. A vertically aligned hex grid
    /// contains columns of vertically aligned hexes.
    /// </summary>
    public Alignment GetAlignment()
    {
        return alignment;
    }

    /// <summary>
    /// Determines the alignment of the hex grid. A horizontally aligned hex grid 
    /// contains rows of horizontally aligned hexes. A vertically aligned hex grid
    /// contains columns of vertically aligned hexes.
    /// </summary>
    public void SetAlignment(Alignment aligned)
    {
        alignment = aligned;
    }

    /// <summary>
    /// Returns true if the provided Hex is new to the grid. Otherwise, 
    /// returns false if the grid already contains the provided coordinates.
    /// </summary>
    public bool AddHex(Cube cubeCoords)
    {
        return hexes.Add(cubeCoords);
    }

    /// <summary>
    /// Returns true if the provided Hex is new to the grid. Otherwise, 
    /// returns false if the grid already contains the provided coordinates.
    /// </summary>
    public bool AddHex(Axial axialCoords)
    {
        return hexes.Add(axialCoords.ToCube());
    }

    public HashSet<Cube> GetHexes()
    {
        return hexes;
    }

    public Vector3 CubeToPixel(Cube cube, float hexSize)
    {
        Vector3 pixel = new Vector3();

        if (alignment == Alignment.Horizontal)
        {
            float x = hexSize * (cube.q + cube.r * 0.5f);
            float y = hexSize * 0.75f * cube.r;
            pixel = new Vector3(x, y, 0.0f);
        }
        else if (alignment == Alignment.Vertical)
        {
            float x = hexSize * 0.75f * cube.q;
            float y = hexSize * (cube.r + cube.q * 0.5f);
            pixel = new Vector3(x, y, 0.0f);
        }
        else
        {
            Debug.LogError("Hex Grid alignment not set.");
        }

        return pixel;
    }

    public Cube PixelToCube(float x, float y, float hexSize)
    {
        Cube cube = new Cube(0, 0, 0);

        if (alignment == Alignment.Horizontal)
        {
            float r = (4.0f / 3.0f) * (y / hexSize);
            float q = (x / hexSize) - (r / 2.0f);
            return Cube.Round(q, r);
        }
        else if (alignment == Alignment.Vertical)
        {
            float q = (x / hexSize) * (4.0f / 3.0f);
            float r = (y / hexSize) - (q / 2.0f);
            return Cube.Round(q, r);
        }
        else
        {
            Debug.LogError("Hex Grid alignment not set.");
        }

        return cube;
    }

    public delegate bool CubeFilterCriteria(Cube cube);

    /// <summary>
    /// Returns a list of list of cube coordinates, where each sub-list contains the coordinates 
    /// reachable at that index distance from the provided start coordinate.
    /// </summary>
    public List<List<Cube>> Reachable(Cube start, int range, CubeFilterCriteria criteria)
    {
        HashSet<Cube> visited = new HashSet<Cube> { start };
        List<List<Cube>> reachable = new List<List<Cube>> { new List<Cube> { start } };

        for (int i = 1; i <= range; i++)
        {
            reachable.Add(new List<Cube>());
            foreach (Cube cube in reachable[i - 1])
            {
                List<Cube> neighbors = GetNeighbors(cube);
                foreach (Cube neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && criteria(neighbor))
                    {
                        visited.Add(neighbor);
                        reachable[i].Add(neighbor);
                    }
                }
            }
        }

        return reachable;
    }

    public List<Cube> GetShortestPath(Cube start, Cube target, CubeFilterCriteria criteria)
    {
        Dictionary<Cube, Cube> previous = new Dictionary<Cube, Cube>();

        Queue<Cube> queue = new Queue<Cube>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Cube currentCube = queue.Dequeue();
            foreach (Cube neighbor in GetNeighbors(currentCube))
            {
                if (previous.ContainsKey(neighbor) || !criteria(neighbor))
                {
                    continue;
                }

                previous[neighbor] = currentCube;
                queue.Enqueue(neighbor);
            }
        }

        List<Cube> path = new List<Cube>();
        if (previous.ContainsKey(target))
        {
            Cube currentCube = target;
            while (currentCube != start)
            {
                path.Add(currentCube);
                currentCube = previous[currentCube];
            }
            path.Add(start);
            path.Reverse();
        }
        return path;
    }

    /// <summary> This function must return a weight of 1 or greater to guarantee that the quickest path is always returned </summary>
    public delegate float CubeWeight(Cube cube);

    public List<Cube> GetQuickestPath(Cube start, Cube target, CubeFilterCriteria criteria, CubeWeight weight)
    {
        List<Cube> quickestPath = new List<Cube>();

        HashSet<Cube> openCubes = new HashSet<Cube>(); // set of cubes to be evaluated
        HashSet<Cube> closedCubes = new HashSet<Cube>(); // set of cubes already evaluated

        Dictionary<Cube, Cube> cameFrom = new Dictionary<Cube, Cube>(); // associates a key cube with the cube it came from
        Dictionary<Cube, float> gScore = new Dictionary<Cube, float>(); // cost of getting to cube from the start (inf if not included)
        Dictionary<Cube, float> fScore = new Dictionary<Cube, float>(); // total cost of getting from start to end via a given cube (partly known, partly heuristic)

        openCubes.Add(start);
        gScore.Add(start, 0.0f); // cost from start to start is zero
        fScore.Add(start, CubeDistance(start, target)); // for start node, total cost is all heuristic (i.e unweighted cube distance to end)

        while (openCubes.Count > 0)
        {
            // TODO: This can be optimized witch some sort of ordered priority queue, such as a binary heap
            // Get the cube in the open set that has the current lowest fScore
            Cube currentCube = openCubes.First();
            foreach (Cube cube in openCubes)
            {
                if (fScore.ContainsKey(cube) && fScore[cube] < fScore[currentCube])
                {
                    currentCube = cube;
                }
            }

            //Debug.Log($"HexGrid :: CURRENT Cube: {currentCube}");

            // if the current node is the target/goal endNode, then we've reached the end and can return the shortest path
            if (currentCube == target)
            {
                //Debug.Log($"HexGrid :: Current Cube is the TARGET!");
                quickestPath.Add(currentCube);
                while (cameFrom.ContainsKey(currentCube))
                {
                    currentCube = cameFrom[currentCube];
                    quickestPath.Add(currentCube); // adds cubes from finish to start
                }
                return quickestPath.AsEnumerable().Reverse().ToList(); // reverse cubes to order from start to finish
            }

            openCubes.Remove(currentCube);
            closedCubes.Add(currentCube);

            foreach (Cube neighbor in GetNeighbors(currentCube))
            {
                //Debug.Log($"HexGrid :: Neighbor Cube: {neighbor}");

                if (criteria(neighbor) == false)
                {
                    //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} NOT Traversible");
                    continue;
                }

                if (closedCubes.Contains(neighbor))
                {
                    //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} already Evaluted");
                    continue; // ignore if already evaluated
                }

                float tentativeGScore = gScore[currentCube] + (0.5f * weight(currentCube) + 0.5f * weight(neighbor));
                //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} Tentative G Score: ({gScore[currentCube]} + {weight(neighbor)})");

                if (openCubes.Contains(neighbor) == false) // newly discovered node
                {
                    //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} added to Open Cubes");
                    openCubes.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue; // This is not a better path
                }

                // Otherwise, this is the best path up til now
                cameFrom[neighbor] = currentCube;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + CubeDistance(neighbor, target);

                //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} best so far!");
                //Debug.Log($"HexGrid :: Neighbor Cube {neighbor} F Score: ({gScore[neighbor]} + {CubeDistance(neighbor, target)})");
            }
        }

        // If endNode not reached, return empty list
        return quickestPath;
    }

    public int CubeDistance(Cube cubeA, Cube cubeB)
    {
        return Mathf.Max(Mathf.Abs(cubeA.q - cubeB.q), Mathf.Abs(cubeA.r - cubeB.r), Mathf.Abs(cubeA.s - cubeB.s));
    }

    /// <summary>
    /// Returns a list of the Cube coordinates of the hexes forming 
    /// a ring around the provided center at radius distance away.
    /// </summary>
    public List<Cube> GetRing(Cube center, int radius)
    {
        if (radius == 0)
        {
            return new List<Cube> { center };
        }

        List<Cube> ring = new List<Cube>();
        Cube currentCube = center + (Mathf.Abs(radius) * cubeDirections[4]);
        
        // Iterate through each direction
        foreach (int i in alignment == Alignment.Horizontal ? hDirections.Keys : vDirections.Keys)
        {
            // The length side of the ring is equal to the radius 
            for (int j = 0; j < Mathf.Abs(radius); j++)
            {
                ring.Add(currentCube);
                currentCube = GetNeighbor(currentCube, (Direction)i);
            }
        }

        return ring;
    }

    public enum Direction
    {
        N, NE, E, SE, S, SW, W, NW
    }

    // Key: Direction as int; Value: index of corresponding Cube vector in cubeDirections
    Dictionary<int, int> hDirections = new Dictionary<int, int> { { 1, 0 }, { 2, 1 }, { 3, 2 }, { 5, 3 }, { 6, 4 }, { 7, 5 } };
    Dictionary<int, int> vDirections = new Dictionary<int, int> { { 0, 5 }, { 1, 0 }, { 3, 1 }, { 4, 2 }, { 5, 3 }, { 7, 4 } };

    Cube[] cubeDirections = new Cube[6] { new Cube(+1, -1, 0), new Cube(+1, 0, -1), new Cube(0, +1, -1),
                                          new Cube(-1, +1, 0), new Cube(-1, 0, +1), new Cube(0, -1, +1)};

    /// <summary>
    /// Returns a cube coordinate vector corresponding with the provided direction. Errors on invalid directions given alignment.
    /// </summary>
    public Cube CubeDirection(Direction dir)
    {
        if (alignment == Alignment.Horizontal && !hDirections.ContainsKey((int)dir) ||
            alignment == Alignment.Vertical && !vDirections.ContainsKey((int)dir))
        {
            Debug.LogError(dir.ToString() + " is not a valid direction for neighbors in a " + alignment + " hex grid.");
        }

        return cubeDirections[alignment == Alignment.Horizontal ? hDirections[(int)dir] : vDirections[(int)dir]];
    }

    /// <summary>
    /// Return the cube coordinates of the neighbor toward the given direction from the provided cube coordinates.
    /// </summary>
    public Cube GetNeighbor(Cube cube, Direction dir)
    {
        return cube + CubeDirection(dir);
    }

    /// <summary>
    /// Return all neighboring cube coordinates from provided cube coordinates (in clockwise order).
    /// </summary>
    public List<Cube> GetNeighbors(Cube cube)
    {
        List<Cube> neighboringCubes = new List<Cube>();
        foreach (Direction dir in alignment == Alignment.Horizontal ? hDirections.Keys : vDirections.Keys)
        {
            neighboringCubes.Add(GetNeighbor(cube, dir));
        }
        return neighboringCubes;
    }

    // Key: Direction as int; Value: index of corresponding Cube vector in cubeDiagonals
    Dictionary<int, int> hDiagonals = new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 3, 2 }, { 4, 3 }, { 5, 4 }, { 7, 5 } };
    Dictionary<int, int> vDiagonals = new Dictionary<int, int> { { 1, 0 }, { 2, 1 }, { 3, 2 }, { 5, 3 }, { 6, 4 }, { 7, 5 } };

    Cube[] cubeDiagonals = new Cube[6] { new Cube(+1, -2, +1), new Cube(+2, -1, -1), new Cube(+1, +1, -2),
                                         new Cube(-1, +2, -1), new Cube(-2, +1, +1), new Cube(-1, -1, +2)};

    /// <summary>
    /// Returns a cube coordinate vector corresponding with the provided diagonal direction. Errors on invalid directions given alignment.
    /// </summary>
    public Cube CubeDiagonal(Direction dir)
    {
        if (alignment == Alignment.Horizontal && !hDiagonals.ContainsKey((int)dir) ||
            alignment == Alignment.Vertical && !vDiagonals.ContainsKey((int)dir))
        {
            Debug.LogError(dir.ToString() + " is not a valid direction for diagonal neighbors in a " + alignment + " hex grid.");
        }

        return cubeDiagonals[alignment == Alignment.Horizontal ? hDiagonals[(int)dir] : vDiagonals[(int)dir]];
    }

    /// <summary>
    /// Return the cube coordinates of the neighbor toward the given diagonal direction from the provided cube coordinates.
    /// </summary>
    public Cube GetDiagonalNeighbor(Cube cube, Direction dir)
    {
        return cube + CubeDiagonal(dir);
    }

    /// <summary>
    /// Return all diagonally-neighboring cube coordinates from provided cube coordinates (in clockwise order).
    /// </summary>
    public List<Cube> GetDiagonalNeighbors(Cube cube)
    {
        List<Cube> neighboringDiagonalCubes = new List<Cube>();
        foreach (Direction dir in alignment == Alignment.Horizontal ? hDiagonals.Keys : vDiagonals.Keys)
        {
            neighboringDiagonalCubes.Add(GetDiagonalNeighbor(cube, dir));
        }
        return neighboringDiagonalCubes;
    }
}

/// <summary>
/// Cube coordinate for a hex grid
/// </summary>
public class Cube
{
    Vector3Int vec3;

    public Cube(int q, int r, int s)
    {
        if (q + r + s != 0)
        {
            Debug.LogError("The provided coordinates (" + q + ", " + r + ", " + s + ") "
                         + "do not form a canonical coordinate set. "
                         + "Coordinate values must satisfy the criteria q + r + s == 0.");
        }

        this.vec3 = new Vector3Int(q, s, r);
    }

    public Cube(int q, int r)
    {
        this.vec3 = new Vector3Int(q, -q - r, r);
    }

    public int q { get { return vec3.x; } }

    public int r { get { return vec3.z; } }

    public int s { get { return vec3.y; } }

    public Axial ToAxial()
    {
        return new Axial(q, r);
    }

    public OffsetCoord ToOffsetCoord()
    {
        var oddR_col = q + (r - (r & 1)) / 2;
        var oddR_row = r;
        return new OffsetCoord(oddR_col, oddR_row);
    }

    public Vector3 ToVector3()
    {
        return vec3;
    }

    public static Cube Round(float q, float r)
    {
        float s = -q - r;
        return Round(q, r, s);
    }

    public static Cube Round(float q, float r, float s)
    {
        int rq = Mathf.RoundToInt(q);
        int rr = Mathf.RoundToInt(r);
        int rs = Mathf.RoundToInt(s);

        float dq = Mathf.Abs(rq - q);
        float dr = Mathf.Abs(rr - r);
        float ds = Mathf.Abs(rs - s);

        if (dq > dr && dq > ds)
        {
            rq = -rr - rs;
        }
        else if (dr > ds)
        {
            rr = -rq - rs;
        }
        else
        {
            rs = -rq - rr;
        }

        return new Cube(rq, rr, rs);
    }

    public static List<Cube> Line(Cube a, Cube b)
    {
        if (a == b)
        {
            return new List<Cube> { a };
        }

        int N = Distance(a, b);
        List<Cube> results = new List<Cube>();

        for (int i = 0; i <= N; i++)
        {
            float t = (1f / N) * i;
            float q = Mathf.Lerp(a.q, b.q, t);
            float r = Mathf.Lerp(a.r, b.r, t);
            float s = Mathf.Lerp(a.s, b.s, t);
            results.Add(Round(q, r, s));
        }

        return results;
    }

    public static List<Cube> Range(Cube center, int range)
    {
        List<Cube> results = new List<Cube>();

        for (int dq = -range; dq <= range; dq++)
        {
            for (int dr = Mathf.Max(-range, -dq - range); dr <= Mathf.Min(range, -dq + range); dr++)
            {
                int ds = -dq - dr;
                results.Add(center + new Cube(dq, dr, ds));
            }
        }

        return results;
    }

    public static int Distance(Cube a, Cube b)
    {
        return Mathf.Max(Mathf.Abs(a.q - b.q), Mathf.Abs(a.r - b.r), Mathf.Abs(a.s - b.s));
    }

    public static Cube operator +(Cube a, Cube b)
    {
        return new Cube(a.q + b.q, a.r + b.r, a.s + b.s);
    }

    public static Cube operator -(Cube a, Cube b)
    {
        return new Cube(a.q - b.q, a.r - b.r, a.s - b.s);
    }

    public static Cube operator *(int c, Cube a)
    {
        return new Cube(c * a.q, c * a.r, c * a.s);
    }

    public static Cube operator *(Cube a, int c)
    {
        return c * a;
    }

    public override bool Equals(object obj)
    {
        if (obj is Cube)
        {
            return Equals((Cube)obj);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public bool Equals(Cube other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.q == this.q && other.r == this.r && other.s == this.s)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator ==(Cube a, Cube b)
    {
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else if (UnityEngine.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else
        {
            return a.Equals(b);
        }
    }

    public static bool operator !=(Cube a, Cube b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return string.Format("Cube(" + q + ", " + r + ", " + s + ")");
    }

    public override int GetHashCode()
    {
        return new Vector2Int(q, r).GetHashCode();
    }
}

// TODO: Add Axial directions and neighbors (and diagonals?)

public class Axial
{
    Vector2Int vec2;

    public Axial(int q, int r)
    {
        this.vec2 = new Vector2Int(q, r);
    }

    public int q { get { return vec2.x; } }

    public int r { get { return vec2.y; } }

    public Cube ToCube()
    {
        return new Cube(q, r, -q - r);
    }

    public static Axial operator +(Axial a, Axial b)
    {
        return new Axial(a.q + b.q, a.r + b.r);
    }

    public static Axial operator -(Axial a, Axial b)
    {
        return new Axial(a.q - b.q, a.r - b.r);
    }

    public static Axial operator *(int c, Axial a)
    {
        return (c * new Cube(a.q, a.r)).ToAxial();
    }

    public static Axial operator *(Axial a, int c)
    {
        return c * a;
    }

    public override bool Equals(object obj)
    {
        if (obj is Axial)
        {
            return Equals((Axial)obj);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public bool Equals(Axial other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.q == this.q && other.r == this.r)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator ==(Axial a, Axial b)
    {
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else if (UnityEngine.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else
        {
            return a.Equals(b);
        }
    }

    public static bool operator !=(Axial a, Axial b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return string.Format("Axial(" + q + ", " + r + ")");
    }

    public override int GetHashCode()
    {
        return new Vector2Int(q, r).GetHashCode();
    }
}

public class OffsetCoord
{
    private Vector2Int vec2;

    public OffsetCoord(int col, int row)
    {
        this.vec2 = new Vector2Int(col, row);
    }

    public int col { get { return vec2.x; } }

    public int row { get { return vec2.y; } }

    public Cube ToCube()
    {
        var oddR_q = col - (row - (row & 1)) / 2;
        var oddR_r = row;
        return new Cube(oddR_q, oddR_r);
    }

    public static OffsetCoord operator +(OffsetCoord a, OffsetCoord b)
    {
        return new OffsetCoord(a.col + b.col, a.row + b.row);
    }

    public static OffsetCoord operator -(OffsetCoord a, OffsetCoord b)
    {
        return new OffsetCoord(a.col - b.col, a.row - b.row);
    }

    public static OffsetCoord operator *(int c, OffsetCoord a)
    {
        return (c * new Cube(a.col, a.row)).ToOffsetCoord();
    }

    public static OffsetCoord operator *(OffsetCoord a, int c)
    {
        return c * a;
    }

    public override bool Equals(object obj)
    {
        if (obj is OffsetCoord)
        {
            return Equals((OffsetCoord)obj);
        }
        else
        {
            return base.Equals(obj);
        }
    }

    public bool Equals(OffsetCoord other)
    {
        if (other == null)
        {
            return false;
        }
        else if (other.col == this.col && other.row == this.row)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool operator ==(OffsetCoord a, OffsetCoord b)
    {
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else if (UnityEngine.Object.ReferenceEquals(a, b))
        {
            return true;
        }
        else
        {
            return a.Equals(b);
        }
    }

    public static bool operator !=(OffsetCoord a, OffsetCoord b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return string.Format($"OffsetCoord( {col} , {row} )");
    }

    public override int GetHashCode()
    {
        return new Vector2Int(col, row).GetHashCode();
    }
}