using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Converts sprites to follow an object in the XZ plane, rather than the XY
/// plane they were originally coded for.  This allows OTSprites to be used in
/// conjunction with the Unity Terrain objects.  Specifically, this script adds
/// an extra step to authoring (requiring you to attach sprites to invisible
/// virtual agents through this script), but circumvents some complicated
/// computation (does not attempt to programmatically solve the issue via
/// inverted transformation matrices).
/// </summary>
[RequireComponent (typeof (OTSprite))]
public class SpriteProjector : MonoBehaviour {

    /// <summary>
    /// The game object the sprite should follow.
    /// </summary>
    public GameObject FollowTarget = null;
    /// <summary>
    /// The sprite attached to this game object.
    /// </summary>
    private OTSprite Sprite { get; set; }
    /// <summary>
    /// Dictionary mapping string representations of directions to vectors.
    /// </summary>
    private Dictionary<string, Vector3> DirectionMap { get; set; }

	// Use this for initialization
	void Start () {
        this.Sprite = this.GetComponent<OTSprite>();
	}
	
	// Update is called once per frame
	void Update () {
        //Pretty straight forward, just set the sprite position to target position.
        if (this.FollowTarget != null) {
            Vector3 pos = this.FollowTarget.transform.position;

            //This is overridden somewhere in the Orthello sprite code, so we have to set z separately.
            this.transform.position = pos;
            this.Sprite.depth = pos.z;
        }
	}
}
