using UnityEngine;
using System.Collections;

/// <summary>
/// Simplistic testing script that performs raycasts into the scene from the
/// mouse's current position and displays information about colliders under
/// the mouse.
/// </summary>
public class MouseCollisionTest : MonoBehaviour {
    /// <summary>
    /// The offset of the display from the lower right corner.
    /// </summary>
    public Vector2 DisplayOffset = new Vector2(16, 16);
    /// <summary>
    /// The size of the display.
    /// </summary>
    public Vector2 DisplaySize = new Vector2(192, 96);

    /// <summary>
    /// The layermask to use when raycasting.
    /// </summary>
    public LayerMask Mask = new LayerMask();

    /// <summary>
    /// True, if the most recent raycast struck an object.
    /// </summary>
    private bool MouseHit;
    /// <summary>
    /// The specific results of the most recent raycast.
    /// </summary>
    private RaycastHit MouseRaycast;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.MouseCast();
	}

    /// <summary>
    /// Performs a raycast into the world from the current mouse position.
    /// </summary>
    private void MouseCast() {
        //Get mouse ray.
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);

        //Get ray collision with the mask.
        this.MouseHit = Physics.Raycast(ray, out this.MouseRaycast, 128f, this.Mask);
    }

    // OnGUI is called once per event (multiple times per frame)
    void OnGUI() {
        //Create bounding rectangle for the GUI widget.
        Rect bounds = new Rect(
            Screen.width - this.DisplayOffset.x - this.DisplaySize.x,
            Screen.height - this.DisplayOffset.y - this.DisplaySize.y,
            this.DisplaySize.x,
            this.DisplaySize.y);

        GUI.Window(0, bounds, this.MouseRayInfo, "Mouse Collision Info");
    }

    private void MouseRayInfo(int windowID) {
        //Begin automatic vertical layout.  Must be followed by EndVertical().
        GUILayout.BeginVertical();

        //Basic debug output: object name, layer, and distance.
        string objectName = (this.MouseHit) ? this.MouseRaycast.transform.gameObject.name : "(No Object Hit)";
        GUILayout.Label(objectName);
        if (this.MouseHit) {
            string layer = LayerMask.LayerToName(this.MouseRaycast.transform.gameObject.layer);
            GUILayout.Label("Layer: " + layer);
            GUILayout.Label("Distance: " + this.MouseRaycast.distance);
        }

        //End automatic vertical layout.
        GUILayout.EndVertical();
    }
}