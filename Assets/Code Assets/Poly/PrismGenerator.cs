using System.Collections.Generic;
using Poly;
using UnityEngine;

/// <summary>
/// A script capable of generating and placing 3D prisms in the scene.
/// </summary>
public class PrismGenerator : MonoBehaviour {

    /// <summary>
    /// The number of polygons to generate.
    /// </summary>
    public int NumPrisms = 32;
    /// <summary>
    /// The maximum distance to place polygons from this object.  The polygons
    /// are distributed across a square of length 2 * MaxOffset in the x-z
    /// plane.
    /// </summary>
    public float MaxOffset = 16;

    /// <summary>
    /// The maximum number of vertices generated polygons should have.
    /// </summary>
    public int MaxVertices = 8;
    /// <summary>
    /// The minimum number of vertices generated polygons should have.  Values
    /// less than 3 are ignored.
    /// </summary>
    public int MinVertices = 3;
    /// <summary>
    /// The minimum offset of each vertex from the origin of the polygon as a
    /// percentage.  This value modifies the initially unit length vertices.
    /// Note that the origin is not the center or centroid of the polygon by
    /// any definition.
    /// </summary>
    public float MinVertexOffset = 0.2f;
    /// <summary>
    /// The generated polygon is created by randomly offsetting the initially
    /// regular central angle of vertices on the unit circle.  This offset
    /// value modifies the range of the allowable offset as a percentage of the
    /// full interior angle.
    /// </summary>
    public float MinAngularOffset = 0.1f;

    /// <summary>
    /// The maximum scale of the generated mesh.
    /// </summary>
    public float MaxScale = 4;
    /// <summary>
    /// The minimum scale of the generated mesh.
    /// </summary>
    public float MinScale = 1;
    /// <summary>
    /// The height of the generated prisms.
    /// </summary>
    public float PrismHeight = 1;

    /// <summary>
    /// The radius to clear of any generated prisms around the generator.    /// </summary>
    public float ClearRadius = 2;

    /// <summary>
    /// The material to apply to generated polygons.
    /// </summary>
    public Material PrismMaterial = null;

    /// <summary>
    /// If true, the generator will use a given random seed value.
    /// </summary>
    public bool UseFixedSeed = false;
    /// <summary>
    /// The seed to use for random generation.
    /// </summary>
    public int Seed = 0;
    /// <summary>
    /// If true, the generator will draw debugging lines at the vertices of all
    /// prisms.
    /// </summary>
    public bool DrawDebug = false;

    /// <summary>
    /// A collection containing all prisms generated at runtime.
    /// </summary>
    public List<Prism> GeneratedPrisms { get; private set; }

    /// <summary>
    /// The offset of the display from the lower right corner.
    /// </summary>
    public Vector2 DisplayOffset = new Vector2(16, 16);
    /// <summary>
    /// The size of the display.
    /// </summary>
    public Vector2 DisplaySize = new Vector2(192, 96);

    /// <summary>
    /// The seed used during generation.
    /// </summary>
    private int generationSeed;

	// Use this for initialization
	void Start () {
        //LineRenderer line = this.gameObject.AddComponent<LineRenderer>();
        //line.material = new Material(Shader.Find("Particles/Additive"));
        
        //line.SetColors(Color.red, Color.blue);
        //line.SetWidth(0.25f, 1);

        //int points = 4;
        //float dist = 8;

        //float x, y, z;
        //line.SetVertexCount(points);
        //for (int i = 0; i < points; i++) {
        //    x = -dist / 2 + i * dist / (points - 1);
        //    y = 0;
        //    z = 0;

        //    Vector3 vec = new Vector3(x, y, z);
        //    line.SetPosition(i, vec);
        //}

        if (this.UseFixedSeed) Random.seed = this.Seed;

        //Record seed for display, in case it changes later.
        this.generationSeed = Random.seed;

        //Generate a bunch of prisms.
        this.GeneratedPrisms = new List<Prism>();
        for (int i = 0; i < this.NumPrisms; i++) {
            Prism prism = this.SpawnPrism();
            this.GeneratedPrisms.Add(prism);
        }

        //Delete any prisms in the center of the generator.
        if (this.ClearRadius > 0) this.ClearOrigin();
	}

    // Update is called once per frame
    void Update() {
        if (this.DrawDebug) {
            //Draw little stubby lines outward from each vertex.
            foreach (Prism prism in this.GeneratedPrisms) {
                foreach (Vector3 vertex in prism.TransformedVertices) {
                    Vector3 end = vertex + Vector3.Normalize(vertex - prism.Origin);
                    Debug.DrawLine(vertex, end, Color.red);
                }
            }
        }
	}

    // OnGUI is called once per event (multiple times per frame)
    void OnGUI() {
        //Create bounding rectangle for the GUI widget.
        Rect bounds = new Rect(
            this.DisplayOffset.x,
            Screen.height - this.DisplayOffset.y - this.DisplaySize.y,
            this.DisplaySize.x,
            this.DisplaySize.y);

        GUI.Window(1, bounds, this.SeedInfo, "Generation Seed");
    }

    private void SeedInfo(int windowID) {
        GUILayout.BeginVertical();

        GUILayout.Label(this.generationSeed.ToString());

        GUILayout.EndVertical();
    }

    /// <summary>
    /// Places a randomly created prism into the world.
    /// </summary>
    /// <returns>The generated prism.</returns>
    private Prism SpawnPrism() {
        //Generate a prism.
        Prism prism = this.CreatePolygonPrism();

        //Create new game object.
        GameObject go = new GameObject("GenPrism " + prism.ID);
        go.AddComponent<MeshRenderer>();
        go.renderer.material = (this.PrismMaterial != null) ? this.PrismMaterial : new Material(Shader.Find("Diffuse"));
        go.AddComponent<MeshFilter>().mesh = prism.Mesh;
        go.AddComponent<MeshCollider>();

        //Place gameobject in world.
        go.transform.parent = this.transform;
        go.transform.localPosition = this.RandomPosition();
        go.transform.localScale = this.RandomScale();
        go.layer = this.gameObject.layer;

        prism.GameObject = go;

        return prism;
    }

    /// <summary>
    /// Returns a randomized location within the allowable offset.
    /// </summary>
    /// <returns>A local position offset.</returns>
    private Vector3 RandomPosition() {
        float x = Random.Range(-this.MaxOffset, this.MaxOffset);
        float z = Random.Range(-this.MaxOffset, this.MaxOffset);

        //Introduce a small epsilon y offset to counter floating point errors.
        float y = Random.Range(-0.01f, 0.01f);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Returns a randomized scaling within the allowable range.
    /// </summary>
    /// <returns>A local scaling.</returns>
    private Vector3 RandomScale() {
        float x = Random.Range(this.MinScale, this.MaxScale);
        float z = Random.Range(this.MinScale, this.MaxScale);
        return new Vector3(x, 1, z);

    }

    /// <summary>
    /// Deletes any generated prisms within ClearRadius of the generator object.
    /// </summary>
    private void ClearOrigin() {
        Vector3 upper = this.transform.position + Vector3.up;
        Vector3 lower = this.transform.position + Vector3.down;
        //Drop a vertical ray at the generator's location and collect all colliders.
        Ray ray = new Ray(upper, lower);
        RaycastHit[] hits = Physics.SphereCastAll(ray, this.ClearRadius, (upper - lower).magnitude);
        //Delete all hit objects.
        int deleted = 0;
        foreach (RaycastHit hit in hits) {
            GameObject go = hit.collider.gameObject;
            //If this isn't a prism, carry on.
            if (go.layer != this.gameObject.layer) continue;

            //Delete prism and remove from reference list.
            for (int i = 0; i < this.GeneratedPrisms.Count; i++) {
                if (this.GeneratedPrisms[i].GameObject == go) {
                    this.GeneratedPrisms.RemoveAt(i);
                    deleted++;
                    break;
                }
            }

            Object.DestroyImmediate(hit.collider.gameObject);
        }

        Debug.Log("Destroyed " + deleted + " objects.");
    }

    #region Polygonal Prism Mesh Creation.  Lots of array index math inside.
    /// <summary>
    /// Creates a random polygonal prism mesh.  Don't touch this method unless
    /// you *really* understand what's happening with the vertex order and
    /// triangle indices.
    /// </summary>
    private Prism CreatePolygonPrism() {
        //Choose the number of vertices this polygon should have.
        int numVerts = Random.Range((this.MinVertices < 3) ? 3 : this.MinVertices, this.MaxVertices);

        //Create x-z plane polygon vertices.
        Vector3[] polyVertices = new Vector3[numVerts];
        float centralAngle = 2 * Mathf.PI / numVerts;
        for (int i = 0; i < polyVertices.Length; i++) {
            //I could 'center' the allowable angles, but as long as it's consistent, it's fine.
            float theta = centralAngle * i + (1 - this.MinAngularOffset) * Random.Range(0, centralAngle);
            float r = Random.Range(this.MinVertexOffset, 1);

            polyVertices[i] = r * new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
        }

        
        //Create x-y-z mesh vertices.
        Vector3[] meshVertices = new Vector3[2 * polyVertices.Length];
        float top = this.PrismHeight / 2;
        float bot = -top;
        for (int i = 0; i < polyVertices.Length; i++) {
            Vector3 vertex = polyVertices[i];

            Vector3 upper = new Vector3(vertex.x, top, vertex.z);
            Vector3 lower = new Vector3(vertex.x, bot, vertex.z);

            meshVertices[2 * i] = upper;
            meshVertices[2 * i + 1] = lower;
        }

        //Create triangle list from vertex indices.
        int[] meshTriangles = new int[3 * (meshVertices.Length)];
        for (int i = 0; i < meshVertices.Length; i++) {
            int index = 3 * i;

            //Even triangles are upper edge triangles.  Odds are lower.
            if (i % 2 == 0) {
                meshTriangles[index] = i;
                meshTriangles[index + 1] = i + 2;
                meshTriangles[index + 2] = i + 1;
            } else {
                meshTriangles[index] = i;
                meshTriangles[index + 1] = i + 1;
                meshTriangles[index + 2] = i + 2;
            }

            //Check for wrap around.
            if (meshTriangles[index + 1] >= meshVertices.Length) meshTriangles[index + 1] -= meshVertices.Length;
            if (meshTriangles[index + 2] >= meshVertices.Length) meshTriangles[index + 2] -= meshVertices.Length;
        }

        //Weld the top and bottom onto the prism.
        this.Weld(ref meshVertices, ref meshTriangles);

        //Create mostly useless texture coordinates just to make the rendering shaders happy.
        Vector2[] meshUVs = new Vector2[meshVertices.Length];
        for (int i = 0; i < meshUVs.Length; i++) {
            meshUVs[i] = new Vector2(meshVertices[i].x, meshVertices[i].z);
        }

        //Create the mesh.
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.uv = meshUVs;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        return new Prism(mesh, polyVertices);
    }

    /// <summary>
    /// Welds a top and bottom onto a generated polygonal prism's sides.  Don't
    /// touch this method unless you *really* understand what's happening with
    /// the vertex order and triangle indices.
    /// </summary>
    /// <param name="vertices">The vertex list of the prism's sides.</param>
    /// <param name="triangles">The triangle list of vertex indices.</param>
    private void Weld(ref Vector3[] vertices, ref int[] triangles) {
        //Generation creates upper vertices in even indices and lower in odd.
        Vector3 topVertex = new Vector3(0, vertices[0].y, 0);
        Vector3 botVertex = new Vector3(0, vertices[1].y, 0);
        int polygonEdges = vertices.Length / 2;

        //Expand vertex array. Kinda wish I'd used lists now. >_>
        int numVertices = vertices.Length;
        Vector3[] newVerts = new Vector3[numVertices + 2];
        //Using System namespace causes ambiguous reference with UnityEngine.Random and System.Random. <_<
        System.Array.Copy(vertices, newVerts, vertices.Length);
        //Add the new vertices to the array.
        int top = numVertices;
        newVerts[top] = topVertex;
        int bot = numVertices + 1;
        newVerts[bot] = botVertex;
        
        //Expand triangle list to accomdate the 2(polygonEdges) additional triangles.
        int startIndex = triangles.Length;
        int[] newTris = new int[startIndex + 6 * polygonEdges];
        System.Array.Copy(triangles, newTris, startIndex);

        //Weld the top and bottom shut.
        for (int i = 0; i < polygonEdges; i++) {
            int index = startIndex + 6 * i;

            //Top triangle.
            newTris[index] = top;
            newTris[index + 1] = 2 * i + 2;
            newTris[index + 2] = 2 * i;

            //Bottom triangle.
            newTris[index + 3] = bot;
            newTris[index + 4] = 2 * i + 1;
            newTris[index + 5] = 2 * i + 3;

            //Check for wrap around.
            if (newTris[index + 1] >= numVertices) newTris[index + 1] -= numVertices;
            if (newTris[index + 2] >= numVertices) newTris[index + 2] -= numVertices;
            if (newTris[index + 4] >= numVertices) newTris[index + 4] -= numVertices;
            if (newTris[index + 5] >= numVertices) newTris[index + 5] -= numVertices;
        }

        //A couple swaps and we're good to go.
        vertices = newVerts;
        triangles = newTris;
    }
    #endregion
}