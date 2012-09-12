using System.Collections.Generic;

using UnityEngine;
using RAIN.Path;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;

/// <summary>
/// Controls and represents an animated character within the world.
/// </summary>
public class CharacterScript : MonoBehaviour {

    /// <summary>
    /// The character's name - same as the game object name.
    /// </summary>
    public string Name {
        get { return this.name; }
        set { this.gameObject.name = value; }
    }
    /// <summary>
    /// The character's x-z position in the world.
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
    /// The named locale of the character within the world.
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
    /// The collection of items owned by this character.
    /// </summary>
    public List<Item> Inventory { get; private set; }
    /// <summary>
    /// True, if the character is dead.
    /// </summary>
    public bool Dead { get; set; }
    /// <summary>
    /// Offset, in screenspace, for the label.
    /// </summary>
    public Vector2 LabelOffset = new Vector2(0, 24);
    /// <summary>
    /// If true, a label will be drawn for this character.
    /// </summary>
    public bool DisplayLabel = true;
    /// <summary>
    /// If true, additional information will be present in the label.
    /// </summary>
    public bool ExpandedLabelInfo { get; set; }

    /// <summary>
    /// The current task for this character, if any.
    /// </summary>
    public Task ActiveTask { get; set; }

    /// <summary>
    /// Called a single time, before the first call to Update().
    /// </summary>
    public void Start() {
        this.Inventory = new List<Item>();
    }


    /// <summary>
    /// Called once per frame.  Continaully locks the character's sprite to
    /// it's parent object's transform, as sprites are operating on a
    /// different coordinate system.
    /// </summary>
    public void Update() {
        ////Get our position and the sprite script.
        //Vector3 position = this.transform.position;
        //OTAnimatingSprite sprite = this.gameObject.GetComponentInChildren<OTAnimatingSprite>();

        ////Lock the sprite's position in the y dimension.
        //sprite.position = new Vector2(position.x, 1);
        //sprite.depth = position.z;

        ////Lock the sprite's rotation.
        //Quaternion rotation = this.transform.rotation;
        //sprite.transform.localRotation = Quaternion.Inverse(rotation);
    }

    /// <summary>
    /// Stops pathing movement.
    /// </summary>
    public void Stop() {
        this.GetComponentInChildren<PathManager>().moveTarget.VectorTarget = this.transform.position;
    }

    public void OnGUI() {
        if (!this.DisplayLabel) return;

        //Get the screen coordinates of the label.
        Camera cam = Camera.mainCamera;
        Vector3 pos = cam.WorldToScreenPoint(this.transform.position);

        List<string> text = new List<string>();
        text.Add(this.Name);
        if (this.Dead) text[0] += " (Dead)";

        if (this.ExpandedLabelInfo) {
            if (this.Inventory.Count == 0) {
                text.Add("No Possessions.");
            } else {
                //Display the list of all owned items:
                text.Add("Possessions:");
                foreach (Item item in this.Inventory) {
                    text.Add(item.Name);
                }
            }
        }

        //'String' it all together.  Yeah.  I did that.
        string label = text[0];
        int length = text[0].Length;
        for (int i = 1; i < text.Count; i++) {
            length = (text[i].Length > length) ? text[i].Length : length;
            label += "\n" + text[i];
        }
        length *= 8; //Stab in the dark at average character length in pixels.
        int height = 12 + text.Count * 14; //Stab in the dark at total box height.

        //Damn library can't agree on the origin for 2D space, so we have to fix that.
        GUI.Box(new Rect(pos.x - length / 2 + this.LabelOffset.x, cam.pixelHeight - pos.y + this.LabelOffset.y, length, height), label);
    }
}