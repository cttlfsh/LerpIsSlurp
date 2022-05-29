using System;
using UnityEngine;

/// <summary>
/// This class creates an approximated sphere mesh starting from an octahedron. This because
/// there is a big issue with high resolution sphere meshes: the vertices density at poles
/// is much much higher than at the equatorial zone. For this reason it scales very badly
/// in game engines.
/// 
/// Octahedrons are platonic solids, like cubes, dodecahedron and icosahedrons, and are a
/// fair tradeoff between number of vertices and vertices distribution over the mesh.
/// Unfortunately, Unity does not provide by default this kind of meshes :(
/// 
/// OCTAHEDRON SPECS: 
///     - vertices: 6
///     - edges: 12
///     - faces: 8
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class OctahedronSphere : MonoBehaviour
{
    [Range(0, 6)] public int octahedronDiv = 0; //limited at 6 because Unity explodes with higher numbers
    public float sphereRadius = 1f;

    // FIRST IMPLEMENTATION (OLD)
    // the base 6 vertices
    //private static Vector3[] baseVertices = {
    //    Vector3.down, Vector3.down, Vector3.down, Vector3.down, // quadruplicated the vertices because of UV rotation at poles
    //    Vector3.forward,
    //    Vector3.left,
    //    Vector3.back,
    //    Vector3.right,
    //    Vector3.forward, // added because otherwise the UV would have flipped on the last face
    //    Vector3.up, Vector3.up, Vector3.up, Vector3.up // quadruplicated the vertices because of UV rotation at poles
    //};
    // the main 8 triangles
    //private readonly static int[] baseTriangles = {
    //    0, 4, 5, // down, forward, left
    //    1, 5, 6, // down, left, back
    //    2, 6, 7, // down, back, right
    //    3, 7, 8, // down, right, forward
    //    9, 5, 4, // up, left, forward
    //    10, 6, 5, // up, back, left
    //    11, 7, 6, // up, right, back
    //    12, 8, 7 // up, forward, right
    //};

    private bool regenerateMesh = false;
    private int resolution;
    private int[] meshTriangles;
    private MeshFilter mf;
    private Vector2[] uv;
    private Vector3[] normals;
    private Vector3[] meshVertices;

    private Vector3[] facesDirections = { Vector3.left, Vector3.back, Vector3.right, Vector3.forward };


    /// <summary>
    /// Method which generates the mesh
    /// </summary>
    /// <param name="numDiv">Number of division of the octahedron sphere</param>
    /// <param name="radius">Radius of the octahedron sphere </param>
    /// <returns>The generated mesh</returns>
    public Mesh Create(int numDiv, float radius)
    {
        SetupMeshParameters();
        GenerateOctahedron(meshVertices, meshTriangles, resolution);
        NormalizeVertices(meshVertices, normals);
        UVGenerator(meshVertices, uv);
        for (int i = 0; i < meshVertices.Length; i++)
        {
            meshVertices[i] *= radius;
        }

        Mesh mesh = new Mesh();
        mesh.name = "Octahedron";
        mesh.vertices = meshVertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = meshTriangles;
        return mesh;
    }

    /// <summary>
    /// Generator of the octahedron sphere topology. It loops over the vertices
    /// and gives them the correct topological position and orientation
    /// </summary>
    /// <param name="vertices"></param>
    /// <param name="tris"></param>
    /// <param name="resolution"></param>
    private void GenerateOctahedron(Vector3[] vertices, int[] tris, int resolution)
    {
        int currentVertex = 0;
        int currentTriangle = 0;
        int bottomPrevRowVertexIndex = 0;

        GenerateLowerEmisphere(vertices, tris, resolution, ref currentVertex, ref currentTriangle, ref bottomPrevRowVertexIndex);
        GenerateUpperEmisphere(vertices, tris, resolution, ref currentVertex, ref currentTriangle, ref bottomPrevRowVertexIndex);

        
    }
    
    /// <summary>
    /// Generator of the southern emisphere of the octahedton mesh. First it creates the pole 4 vertices
    /// (one for each quadrant) and then iterates on the resolution level to add vertices and to create 
    /// the triangles. The position of each vertex is determined by lerping between Vector3.down and one
    /// of the direction of the quadrants (left, back,right, forward)
    /// </summary>
    /// <param name="vertices">List where the vertices will be added</param>
    /// <param name="tris">List where the triangles will be added</param>
    /// <param name="resolution">Number of lines of a basic octahedron</param>
    /// <param name="currentVertex">Index of the current processed vertex</param>
    /// <param name="currentTriangle">Index of the current processed triangle</param>
    /// <param name="bottomPrevRowVertexIndex">Index of the vertex on the previous line (used for triangulating)</param>
    private void GenerateLowerEmisphere(Vector3[] vertices, int[] tris, int resolution, ref int currentVertex, ref int currentTriangle, ref int bottomPrevRowVertexIndex)
    {
        //Starting with the first 4 vertices of the southern pole, which are
        // 4 copies of Vector3.down
        for (int i = 0; i < 4; i++)
        {
            vertices[currentVertex++] = Vector3.down;
        }

        for (int i = 1; i <= resolution; i++)
        {
            float progress = (float)i / resolution;
            Vector3 fromDirection;
            Vector3 toDirection;

            vertices[currentVertex++] = Vector3.Lerp(Vector3.down, Vector3.forward, progress);
            toDirection = Vector3.Lerp(Vector3.down, Vector3.forward, progress);

            for (int dir = 0; dir < facesDirections.Length; dir++)
            {
                fromDirection = toDirection;
                toDirection = Vector3.Lerp(Vector3.down, facesDirections[dir], progress);
                currentTriangle = CreateLowerTriStrip(i, currentVertex, bottomPrevRowVertexIndex, currentTriangle, tris);
                currentVertex = CreateVertexLine(fromDirection, toDirection, i, currentVertex, vertices);

                if (bottomPrevRowVertexIndex + i > 1)
                {
                    bottomPrevRowVertexIndex += i - 1;
                }
                else
                {
                    bottomPrevRowVertexIndex += 1;
                }
            }

            bottomPrevRowVertexIndex = currentVertex - 1 - i * 4;
        }
    }

    /// <summary>
    /// Generator of the northern emisphere of the octahedron mesh. Iterates on the resolution level to 
    /// add vertices and to the triangles. The position of each vertex is determined by lerping between 
    /// Vector3.up and one of the direction of the quadrants (left, back,right, forward). At the end the 
    /// last loop creates the last resolution line and at last the 4 vertices of the northern pole.
    /// </summary>
    /// <param name="vertices">List where the vertices will be added</param>
    /// <param name="tris">List where the triangles will be added</param>
    /// <param name="resolution">Number of lines of a basic octahedron</param>
    /// <param name="currentVertex">Index of the current processed vertex</param>
    /// <param name="currentTriangle">Index of the current processed triangle</param>
    /// <param name="prevStripTrisIndex">Index of the vertex on the previous line (used for triangulating)</param>
    private void GenerateUpperEmisphere(Vector3[] vertices, int[] tris, int resolution, ref int currentVertex, ref int currentTriangle, ref int prevStripTrisIndex)
    {
        for (int i = resolution - 1; i >= 1; i--)
        {
            float progress = (float)i / resolution;
            Vector3 fromDirection;
            Vector3 toDirection;

            vertices[currentVertex++] = Vector3.Lerp(Vector3.up, Vector3.forward, progress);
            toDirection = Vector3.Lerp(Vector3.up, Vector3.forward, progress);

            for (int dir = 0; dir < facesDirections.Length; dir++)
            {
                fromDirection = toDirection;
                toDirection = Vector3.Lerp(Vector3.up, facesDirections[dir], progress);
                currentTriangle = CreateUpperTrisStrip(i, currentVertex, prevStripTrisIndex, currentTriangle, tris);
                currentVertex = CreateVertexLine(fromDirection, toDirection, i, currentVertex, vertices);

                prevStripTrisIndex += i + 1;
            }

            prevStripTrisIndex = currentVertex - 1 - i * 4;

        }

        for (int i = 0; i < 4; i++)
        {
            tris[currentTriangle++] = prevStripTrisIndex;
            tris[currentTriangle++] = currentVertex;
            tris[currentTriangle++] = ++prevStripTrisIndex;
            vertices[currentVertex++] = Vector3.up;

        }
    }

    /// <summary>
    /// Helper of the GenerateOctahedron which creates a line of vertices on a face
    /// </summary>
    /// <param name="from">Direction of the starting vertex</param>
    /// <param name="to">Direction of the last vertex of the line</param>
    /// <param name="lineSteps">Resolution level (aka line number)</param>
    /// <param name="currentVertex">Current vertex to assing</param>
    /// <param name="vertices">List of octahedron vertices</param>
    /// <returns>A vertex on the correct line</returns>
    private int CreateVertexLine(Vector3 from, Vector3 to, int lineSteps, int currentVertex, Vector3[] vertices)
    {
        for (int i = 1; i <= lineSteps; i++)
        {
            vertices[currentVertex++] = Vector3.Lerp(from, to, (float)i / lineSteps);
        }
        return currentVertex;
    }

    /// <summary>
    /// Helper of the GenerateOctahedron which generates a triangle strip for a lower octahedron face. It
    /// is invoked before actually defining the vertices that will be used, since we know the
    /// starting indices of the top and bottom vertex we need.
    /// </summary>
    /// <param name="lineSteps">Lenght of the strip minus 1</param>
    /// <param name="topVertex">Top starting vertex for the strip</param>
    /// <param name="bottomVertex">Bottom starting vertex for the strip</param>
    /// <param name="triangleIndex">Triangle index</param>
    /// <param name="tris">List where the triangle strip will be stored</param>
    /// <returns>The last used triangle index</returns>
    private int CreateLowerTriStrip(int lineSteps, int topVertex, int bottomVertex, int triangleIndex, int[] tris)
    {
        for (int i = 1; i < lineSteps; i++)
        {
            /* Tricky, maybe could be done better. Trying to explain it visually
             * 
             * 
             * 
             * First three lines ↓
             *          
             *     top-1     top
             *     
             *          btm
             *          
             *  Second three lines ↓
             *  
             *               top++            
             *  
             *         btm++        btm
             *         
             *  Outside loop lines
             *  
             *               top-1       top
             *               
             *                      btm 
             */
            tris[triangleIndex++] = bottomVertex;
            tris[triangleIndex++] = topVertex - 1;
            tris[triangleIndex++] = topVertex;

            tris[triangleIndex++] = bottomVertex++;
            tris[triangleIndex++] = topVertex++;
            tris[triangleIndex++] = bottomVertex;

        }

        tris[triangleIndex++] = bottomVertex;
        tris[triangleIndex++] = topVertex - 1;
        tris[triangleIndex++] = topVertex;

        return triangleIndex;
    }

    /// <summary>
    /// Helper of the GenerateOctahedron which generates a triangle strip for a upper octahedron face. It
    /// is invoked before actually defining the vertices that will be used, since we know the
    /// starting indices of the top and bottom vertex we need.
    /// </summary>
    /// <param name="lineSteps">Lenght of the strip minus 1</param>
    /// <param name="topVertex">Top starting vertex for the strip</param>
    /// <param name="bottomVertex">Bottom starting vertex for the strip</param>
    /// <param name="triangleIndex">Triangle index</param>
    /// <param name="tris">List where the triangle strip will be stored</param>
    /// <returns>The last used triangle index</returns>
    private int CreateUpperTrisStrip(int lineSteps, int topVertex, int bottomVertex, int triangleIndex, int[] tris)
    {
        tris[triangleIndex++] = bottomVertex++;
        tris[triangleIndex++] = topVertex - 1;
        tris[triangleIndex++] = bottomVertex;

        for (int i = 1; i <= lineSteps; i++)
        {
           // Explained in `CreateLowerFace` method
            tris[triangleIndex++] = topVertex - 1;
            tris[triangleIndex++] = topVertex;
            tris[triangleIndex++] = bottomVertex;

            tris[triangleIndex++] = bottomVertex++;
            tris[triangleIndex++] = topVertex++;
            tris[triangleIndex++] = bottomVertex;

        }


        return triangleIndex;
    }

    /// <summary>
    /// Method which generates the normals for the faces of the octahedron
    /// </summary>
    /// <param name="vertices">List of vertices</param>
    /// <param name="normals">List of normals</param>
    private void NormalizeVertices(Vector3[] vertices, Vector3[] normals)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            normals[i] = vertices[i] = vertices[i].normalized;
        }
    }

    /// <summary>
    /// Method which generates the UV for the octahedron. In order to to this, we need
    /// to map vertex position into texture coordinates. 
    /// 
    /// The function are:
    ///     - X: atan2(x, z) / -2π
    ///     - Y: asin(y) / π + ½
    ///     
    /// Two main problem is that in the X case the function produces values that can be 
    /// negative, so we add 1 if this happens.
    /// </summary>
    /// <param name="vertices">The vertices of the octahedron</param>
    /// <param name="uv">The uv values for each vertices</param>
    private void UVGenerator(Vector3[] vertices, Vector2[] uv)
    {
        float prevX = 1f;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector2 textureCoord = new Vector2
                (
                    Mathf.Atan2(vertex.x, vertex.z) / (-2f * Mathf.PI),
                    Mathf.Asin(vertex.y) / Mathf.PI + 0.5f
                );

            /* If I get the same value twice in a row, it means i have just passed a seam
             * and started a new triangle, so the previous veretex was at one side of the 
             * seam and the current one is at the other. If we set the X value of the
             * texture coordinate to 1 at the previous vertex we fix the flip problem on
             * te last face (the one "touching" the seam)
             */
            if (vertex.x == prevX) 
            {
                uv[i - 1].x = 1f;
            }
            prevX = vertex.x;

            if (textureCoord.x < 0f) textureCoord.x += 1f;


            uv[i] = textureCoord;
        }
        // Manually adjust polar vertices (could not find other way for the moment)
        // because each vertices need to match its triangle.
        uv[vertices.Length - 4].x = uv[0].x = 0.125f;
        uv[vertices.Length - 3].x = uv[1].x = 0.375f;
        uv[vertices.Length - 2].x = uv[2].x = 0.625f;
        uv[vertices.Length - 1].x = uv[3].x = 0.875f;
    }

    /// <summary>
    /// Helper which sets the mesh parameters: resolution, number of vertices and number of tris
    /// depending on the subdivision level.
    /// It also initialize the normals and uv arrays.
    /// </summary>
    private void SetupMeshParameters()
    {
        resolution = (int) Mathf.Pow(2, octahedronDiv);
        meshTriangles = new int[ 3 * (int) Mathf.Pow(2, (2 * octahedronDiv + 3))];
        meshVertices = new Vector3[4 * (resolution + 1) * (resolution + 1) - 3 * (2 * resolution - 1)];
        normals = new Vector3[meshVertices.Length];
        uv = new Vector2[meshVertices.Length];
    }

    #region UNITY_METHODS
    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mf.sharedMesh = Create(octahedronDiv, sphereRadius);

    }

    private void Update()
    {
        if (regenerateMesh)
        {
            regenerateMesh = false;
            if (mf != null)
            {
                mf.sharedMesh.Clear();
            }
            mf.sharedMesh = Create(octahedronDiv, sphereRadius);
        }
    }

    private void OnValidate()
    {
        regenerateMesh = true;
    }

    #endregion

}
