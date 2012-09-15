using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Steers its gameobject along paths defined by mouse clicks.
/// </summary>
public class MouseSteerer : Steerer {

    /// <summary>
    /// The 'fudge' distance for considering a point on a path reached.
    /// </summary>
    public float CloseEnough = 0.75f;

    /// <summary>
    /// The collision mask for determining the intended destination.
    /// </summary>
    public LayerMask TraversableLayer;

    /// <summary>
    /// The material used when rendering the path.
    /// </summary>
    public Material LineMaterial;

    /// <summary>
    /// The collection of points in the world that make up a path.  The object
    /// this script is attached to will be moved along this path.
    /// </summary>
    public List<Vector3> Path { get; private set; }

    /// <summary>
    /// The line renderer that this script attaches to the game object. Displays
    /// the current path.
    /// </summary>
    private LineRenderer PathRenderer { get; set; }

	// Use this for initialization
	void Start () {
        this.Path = new List<Vector3>();

        //Configure the path renderer.
        this.PathRenderer = this.gameObject.AddComponent<LineRenderer>();
        this.PathRenderer.material = this.LineMaterial;//(this.LineMaterial != null) ? this.LineMaterial : new Material(Shader.Find("Diffuse"));
        //this.PathRenderer.SetColors(Color.red, Color.red);
        this.PathRenderer.SetWidth(0.125f, 0.125f);
	}
	
	// Update is called once per frame
	void Update () {
        //On left mouse click, add another point to the path.
        if (Input.GetMouseButtonDown(0)) this.MouseClick();

        //Best to do this every frame, since Path could be altered externally.
        this.SetRenderPoints();

        //If no points in the path, the rest of the update can be skipped.
        if (this.Path.Count == 0) return;

        //Check distance to next point.  Don't remove last point so agent can arrive.
        float distance = (this.Path[0] - this.transform.position).magnitude;
        if (distance < this.CloseEnough && this.Path.Count > 1) this.Path.RemoveAt(0);

        //Check for velocity at path end.
        if (this.rigidbody.velocity.magnitude < 0.25 && 
            distance < this.CloseEnough &&
            this.Path.Count == 1) {
            this.rigidbody.velocity = Vector3.zero;
            return;
        }


        //Continue towards next point.
        if (this.Path.Count > 1) this.Seek(this.Path[0]);
        else this.Arrive(this.Path[0]);
	}

    /// <summary>
    /// Handles left mouse click events.  Finds the location under the mouse
    /// and updates the Path.
    /// </summary>
    private void MouseClick() {
        //Get location under mouse, if valid.
        Vector3? point = this.MouseCast();
        if (point == null) return;

        //INSERT YOUR PATHING SOLUTION HERE. ----------------------------------

        //Something simple to get you started.
        this.Path.Add((Vector3)point);

        //---------------------------------------------------------------------
    }

    /// <summary>
    /// Performs a raycast into the world from the current mouse position.  If
    /// a valid object is struck, return 
    /// </summary>
    private Vector3? MouseCast() {
        //Get mouse ray.
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);

        //Get ray collision with the mask.
        RaycastHit raycast;
        bool hit = Physics.Raycast(ray, out raycast, 128f, this.TraversableLayer);

        //If hit, return the location.
        if (hit) {
            return raycast.point;
        } else return null;
    }

    /// <summary>
    /// Sets PathRenderer vector data to match current contents of Path.
    /// </summary>
    private void SetRenderPoints() {
        this.PathRenderer.SetVertexCount(this.Path.Count + 1);

        //A small epsilon value to offset the line vertically.
        Vector3 epsilon = 0.01f * Vector3.up;

        //Set first point to current location, then add path points.
        this.PathRenderer.SetPosition(0, this.gameObject.transform.position + epsilon);
        for (int i = 0; i < this.Path.Count; i++) {
            this.PathRenderer.SetPosition(i + 1, this.Path[i] + epsilon);
        }
    }
}