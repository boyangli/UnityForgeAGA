using UnityEngine;
using System.Collections;

/// <summary>
/// Script that slaves the camera to track another GameObject.
/// </summary>
public class FollowCam : MonoBehaviour {

    /// <summary>
    /// The other object to follow.
    /// </summary>
    public GameObject TrackObject = null;
    /// <summary>
    /// By default, the camera operates from fixed y positions, looking downward.
    /// If true, the camera will instead used a fixed z position, looking backward.
    /// </summary>
    public bool FrontCam = false;
    /// <summary>
    /// The maximum distance to preserve when zooming.
    /// </summary>
    public float MaxDistance = 64;
    /// <summary>
    /// The minimum distance to preserve when zooming.
    /// </summary>
    public float MinDistance = 4;
    /// <summary>
    /// The speed multiplier used when zooming.
    /// </summary>
    public float ZoomSpeed = 1;
    /// <summary>
    /// If true, the mouse wheel values are inverted when zooming.
    /// </summary>
    public bool InvertZoom = false;
    /// <summary>
    /// If true, zooming inputs are ignored.
    /// </summary>
    public bool DisableZoom = false;

    public void Update() {
        if (!this.DisableZoom) {
            //Zoom in and out with the MouseWheel.
            float zoom = this.ZoomSpeed * Input.GetAxis("Mouse ScrollWheel");
            zoom = (this.InvertZoom) ? -zoom : zoom;

            //Choose appropriate zoom mode.
            if (this.camera.orthographic) {
                this.OrthographicZoom(zoom);
            } else {
                this.PerspectiveZoom(zoom);
            }
        }

        //Get the desired position.
        Vector3 target;
        if (this.TrackObject == null) target = this.transform.position;
        else target = this.TrackObject.transform.position;
        
        //Preserve y (or z) component, take the rest.
        float preserved = (this.FrontCam) ? this.transform.position.z : this.transform.position.y;
        if (this.FrontCam) {
            this.transform.position = new Vector3(target.x, target.y, preserved);
        } else {
            this.transform.position = new Vector3(target.x, preserved, target.z);
        }
    }

    /// <summary>
    /// Zooms a perspective, top-down camera by altering the height.
    /// </summary>
    /// <param name="zoom">The zoom amount.</param>
    private void PerspectiveZoom(float zoom) {
        float distance = (this.FrontCam)? this.transform.position.z :this.transform.position.y;
        distance += zoom;

        //Bounds check the height.
        if (distance > this.MaxDistance) distance = this.MaxDistance;
        if (distance < this.MinDistance) distance = this.MinDistance;

        Vector3 current = this.transform.position;
        if (this.FrontCam) {
            this.transform.position = new Vector3(current.x, current.y, distance);
        } else {
            this.transform.position = new Vector3(current.x, distance, current.z);
        }
    }

    /// <summary>
    /// Zooms an orthographic camera by altering the size.
    /// </summary>
    /// <param name="zoom">The zoom amount.</param>
    private void OrthographicZoom(float zoom) {
        float size = this.camera.orthographicSize + zoom;

        //Bounds check the size.
        if (size > this.MaxDistance) size = this.MaxDistance;
        if (size < this.MinDistance) size = this.MinDistance;

        this.camera.orthographicSize = size;
    }
}