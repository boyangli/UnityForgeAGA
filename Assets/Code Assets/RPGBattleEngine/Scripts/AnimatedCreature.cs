using UnityEngine;
using System.Collections;

using BattleEngine.Creatures;

/// <summary>
/// Script responsible for linking and controlling Creaures and OTAnimatingSprites.
/// </summary>
public class AnimatedCreature {

    /// <summary>
    /// The sprite script attached to this creature's gameobject.
    /// </summary>
    public OTAnimatingSprite Sprite { get; private set; }
    /// <summary>
    /// The head sprite, which is attached to the same object as Sprite.  This
    /// field is only used for some Sprites.
    /// </summary>
    //public OTAnimatingSprite Head { get; private set; }
    /// <summary>
    /// The instantiated creature.
    /// </summary>
    public Creature Creature { get; private set; }
    /// <summary>
    /// Flag for marking this creature as being hovered over by the mouse.
    /// </summary>
    public bool Hovering { get; private set; }
    /// <summary>
    /// The empty sprite prefab that ships with the OT package.
    /// </summary>
    public static GameObject AnimatedSpritePrefab { get; set; }
    /// <summary>
    /// The input delegate to use when the sprites are clicked.
    /// </summary>
    public static OTObject.ObjectDelegate InputDelegate { get; set; }

    //private Vector3 HeadOffset = new Vector3(0, 0.75f, 0);

    /// <summary>
    /// Creates a new AnimatedCreature from the given instantiated creature,
    /// attaching the Sprite asset to the given Battle.
    /// </summary>
    /// <param name="creature">The creature to insert into the world.</param>
    /// <param name="battleObject">The object to parent this creature to.</param>
    public AnimatedCreature(Creature creature, GameObject battleObject) {
        this.Creature = creature;

        //Create the sprite object.
        GameObject spriteObject = (GameObject)Object.Instantiate(AnimatedCreature.AnimatedSpritePrefab);
        spriteObject.name = creature.Name;
        spriteObject.transform.parent = battleObject.transform;

        //Remove spaces from the creature name.  Ghetto-hack.
        string[] parts = creature.Name.Split();
        string name = "";
        foreach (string part in parts) name += part;

        //Configure the sprite script.
        this.Sprite = spriteObject.GetComponent<OTAnimatingSprite>();
        this.Sprite.registerInput = true;
        this.Sprite.onInput = AnimatedCreature.InputDelegate;
        this.Sprite.onMouseEnterOT = this.OnMousEnter;
        this.Sprite.onMouseExitOT = this.OnMouseExit;
        this.Sprite.animation = OT.AnimationByName(name);

        this.Sprite.startAtRandomFrame = true;
        this.Sprite.Play("Rest");

        //if (creature.Name == "Alchemist") {
        //    GameObject headObject = (GameObject)Object.Instantiate(AnimatedCreature.AnimatedSpritePrefab);
        //    headObject.name = creature.Name + "Head";
        //    headObject.transform.parent = battleObject.transform;

        //    this.Head = headObject.GetComponent<OTAnimatingSprite>();
        //    this.Head.registerInput = false;
        //    this.Head.animation = OT.AnimationByName("RedHead");
        //    this.Head.depth = -1;
        //    this.Head.Play("southeast");
        //}
    }

    /// <summary>
    /// The position of the creature's sprites.
    /// </summary>
    public Vector3 Position {
        get { return this.Sprite.transform.position; }
        set {
            this.Sprite.transform.position = value;
            //if (this.Head != null) this.Head.transform.position = value + this.HeadOffset;
        }
    }
    /// <summary>
    /// Flags the creature's sprite(s) to be flipped horizontally.
    /// </summary>
    public bool FlipHorizontal {
        get { return this.Sprite.flipHorizontal;  }
        set {
            this.Sprite.flipHorizontal = value;
            //if (this.Head != null) this.Head.flipHorizontal = value;
        }
    }

    /// <summary>
    /// Destroys all sprites owned by this AnimatedCreature.
    /// </summary>
    public void Destroy() {
        Object.Destroy(this.Sprite.gameObject);
        //if (this.Head != null) Object.Destroy(this.Head.gameObject);
    }

    public void OnMousEnter(OTObject owner) {
        this.Hovering = true;
    }

    public void OnMouseExit(OTObject owner) {
        this.Hovering = false;
    }
}