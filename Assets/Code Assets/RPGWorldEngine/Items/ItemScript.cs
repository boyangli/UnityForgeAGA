using System.Collections.Generic;

using UnityEngine;
using RAIN.Path;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;

/// <summary>
/// Controls and represents an animated character within the world.
/// </summary>
public class ItemScript : MonoBehaviour {

    /// <summary>
    /// The item's name - same as the game object name.
    /// </summary>
    public string Name {
        get { return this.name; }
        set { this.gameObject.name = value; }
    }
    /// <summary>
    /// The item's x-z position in the world.
    /// </summary>
    public Vector2 Location {
        get {
            Vector3 worldPos = this.transform.position;
            return new Vector2(worldPos.x, worldPos.z);
        }
        set {
            Vector3 worldPos = this.transform.position;
            worldPos.x = value.x;
            worldPos.z = value.y;
            this.transform.position = worldPos;
        }
    }
    /// <summary>
    /// The named locale of the item within the world.
    /// </summary>
    public LocaleScript Locale {
        get {
            //Get world script.
            WorldScript ws = Globals.Instance.WorldScript;
            //Find the closest locale.
            float dist = float.PositiveInfinity;
            LocaleScript closest = null;
            foreach (LocaleScript ls in ws.Locales) {
                Vector3 collision = ls.collider.ClosestPointOnBounds(this.transform.position);
                float d = Vector3.Distance(collision, this.transform.position);
                if (d < dist) {
                    dist = d;
                    closest = ls;
                }
            }

            //If we're within 2 units of the closest, then we'll consider ourselves at the location.
            return (dist < 2) ? closest : null;
        }
    }
    /// <summary>
    /// The item this script holds in the world.
    /// </summary>
    public Item Item { get; set; }
    /// <summary>
    /// Offset, in screen space, for the label.
    /// </summary>
    public Vector2 LabelOffset = new Vector2(0, 24);

    public Material HeartOfIceMaterial = null;
    public Material MalletMaterial = null;
    public Material NecronomiconMaterial = null;

    /// <summary>
    /// Called a single time, before the first call to Update().
    /// </summary>
    public void Start() {
        
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    public void Update() {
        //Way to tired to do this properly.
        switch (this.Name) {
            case "Heart of Ice":
                this.gameObject.renderer.material = this.HeartOfIceMaterial;
                break;
            case "Mallet":
                this.gameObject.renderer.material = this.MalletMaterial;
                break;
            case "Necronomicon":
                this.gameObject.renderer.material = this.NecronomiconMaterial;
                break;
        }

    }

    /// <summary>
    /// Called multiple times per frame.
    /// </summary>
    public void OnGUI() {
        //Get the screen coordinates of the label.
        Camera cam = Camera.mainCamera;
		Vector3 pos = cam.WorldToScreenPoint(this.transform.position);

        string text = this.Item.Name;

        int length = text.Length * 10;
        //Damn library can't agree on the origin for 2D space, so we have to fix that.
        GUI.Box(new Rect(pos.x - length / 2 + this.LabelOffset.x, cam.pixelHeight - pos.y + this.LabelOffset.y, length, 24), text);
    }
}