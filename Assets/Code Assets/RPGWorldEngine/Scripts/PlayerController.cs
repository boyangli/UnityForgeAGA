using UnityEngine;

using RAIN.Path;
using RAIN.Sensors;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;

public class PlayerController : MonoBehaviour {

    /// <summary>
    /// The layer mask containing objects for party control collisions.
    /// </summary>
    public LayerMask MouseMask;
    /// <summary>
    /// The range at which to stop approaching an entity.
    /// </summary>
    public float EntityApproachRange = 1f;

    /// <summary>
    /// The current task that the player is attempting to execute.  Provides
    /// context to what mouse clicks on game objects should mean.
    /// </summary>
    public Task Context {
        get {
            return this.m_Context;
        }
        set {
            this.m_Context = value;
            this.Acted = false;
        }
    }

    private Task m_Context;
    /// <summary>
    /// If true, indicates that the contextual task has been executed.
    /// </summary>
    public bool Acted { get; private set; }
    /// <summary>
    /// Because for some messed reason, RAIN has neither a Stop() method, nor
    /// any way of checking the current MoveTarget type, nor a way to query if
    /// we're moving.
    /// </summary>
    public bool Pathing { get; set; }

    /// <summary>
    /// The raw mouse raycasting information from the current frame.
    /// </summary>
    private RaycastHit MouseHit;
    /// <summary>
    /// The game object the mouse is hovering over, filtered into objects we 
    /// actually care about.
    /// </summary>
    private GameObject MouseObject = null;
    
    /// <summary>
    /// The entity that is currently being approached.
    /// </summary>
    private GameObject approachEntity = null;
	
	public bool displayWindow = false;
    // Use this for initialization
    void Start() {
        //this.gameObject.GetComponent<OTAnimatingSprite>().onInput = this.OnInput;
    }

    /// <summary>
    /// Called once per frame.  Assigns move targets for the player party and
    /// handles task-contextual actions.
    /// </summary>
    public void Update() {
		
		if(Globals.Instance.WorldGUI.selectedAction != -1)						
			Globals.Instance.WorldGUI.DisplayActionOptions = false;
        //If last frame's mouse object is a character, mark it as not hovered.
        if (this.MouseObject != null) {
            CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript>();
            if (cScript != null) cScript.ExpandedLabelInfo = false;
        }

        //Get the object under mouse.
        this.MouseCast();

        //If the mouse object is a character, mark it as hovered.
        if (this.MouseObject != null) {
            CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript>();
            if (cScript != null) cScript.ExpandedLabelInfo = true;
        }

        //Handle contextual left clicks.
        if (Input.GetMouseButtonDown(0)) this.HandleClick();

        //Providing a 'clean' way to stop movement.
        if (this.Pathing) {
            PathManager pather = this.gameObject.GetComponentInChildren<PathManager>();

            //If we're pathing to an entity, rather than a point, check remaining distance.
            if (this.approachEntity != null) {
                float dist2 = Vector3.SqrMagnitude(this.approachEntity.transform.position - this.transform.position);
                if (dist2 < this.EntityApproachRange * this.EntityApproachRange) {
                    //Close enough, halt movement and attempt contextual action.
					if(Globals.Instance.WorldGUI.selectedAction == -1)
						Globals.Instance.WorldGUI.DisplayActionOptions = true;
					else Globals.Instance.WorldGUI.DisplayActionOptions = false;
					
                    pather.moveTarget.TransformTarget = this.transform;
                    this.ActInContext();

                    this.approachEntity = null;
                    this.Pathing = false;
                }
            } else {
                float dist2 = Vector3.SqrMagnitude(pather.moveTarget.VectorTarget - this.transform.position);
                //If we're within the close enough distance, 'drop' the move vector.
                if (dist2 <= pather.closeEnoughDistance * pather.closeEnoughDistance) {
                    pather.moveTarget.TransformTarget = this.transform;
                    this.Pathing = false;
                }
            }
        }

        //Check for completion of GoTo tasks each frame.
        if (this.Context != null && this.Context.Type == "goto") {
            if (this.Context.Actor.Locale == this.Context.Locale) this.Acted = true;
        }
    }

    /// <summary>
    /// Performs a raycast into the world from the current mouse position while
    /// yeilding to the GUI.
    /// </summary>
    private void MouseCast() {
        //Yeild to the GUI.
        if (Globals.Instance.WorldGUI.MouseOverGUI) return;
		
		
        //Get mouse ray.
        Ray ray = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);

        //Get ray collision with the mask.
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, this.MouseMask)) {
            //Make sure we get a direct child of the root node, if possible.
            GameObject go = hit.transform.gameObject;
            while (go.transform.parent != null && go.transform.parent.gameObject != Globals.Instance.WorldRoot) {
                go = go.transform.parent.gameObject;
            }
            this.MouseObject = go;
            this.MouseHit = hit;
        }
    }

    /// <summary>
    /// Moves the player towards the clicked location of entity.
    /// </summary>
    private void HandleClick() {
        //Ignore null, in case that happens.
        if (this.MouseObject == null) return;
        //Yield to GUI.
        if (Globals.Instance.WorldGUI.MouseOverGUI) return;

        //Well we clicked on something, we'll need this shortly.
        PathManager pather = this.gameObject.GetComponentInChildren<PathManager>();

        //Get something, find out if it's a character, item, or the terrain.
        CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript>();
        ItemScript iScript = this.MouseObject.GetComponent<ItemScript>();

        //A few other colliders can be hit....
        //ObstacleAvoidanceCollider obstacle = hit.transform.gameObject.GetComponent<ObstacleAvoidanceCollider>();
        //ExpandingBoxSensor sensor = hit.transform.gameObject.GetComponent<ExpandingBoxSensor>();
        //if (obstacle != null) cScript = hit.transform.parent.GetComponent<CharacterScript>();
        //else if (sensor != null) cScript = hit.transform.parent.GetComponent<CharacterScript>();

        if (cScript != null) {
            //Character clicked.  Move towards character.
            pather.moveTarget.TransformTarget = cScript.transform;
            this.approachEntity = cScript.gameObject;
            this.Pathing = true;
        } else if (iScript != null) {
            //Item clicked.
            pather.moveTarget.TransformTarget = iScript.transform;
            this.approachEntity = iScript.gameObject;
            this.Pathing = true;
        } else {
            //Terrain clicked.
            pather.moveTarget.VectorTarget = this.MouseHit.point;
            this.Pathing = true;
        }
    }

    /// <summary>
    /// Attempts to complete the player's current story event, if a contextually
    /// appropriate entity was clicked.
    /// </summary>
    private void ActInContext() {
        //Well, obviously we're not trying to do anything if there's no task.
        if (this.Context == null) return;
		
		
		
        Debug.Log("Attempting contextual action: " + this.Context.Description);

        //Get the chracter script or item script that was clicked on. 
        CharacterScript cScript = this.approachEntity.GetComponent<CharacterScript>();
        ItemScript iScript = this.approachEntity.GetComponent<ItemScript>();

        if (iScript != null && this.Context.Type == "pickup" && iScript.Item == this.Context.Item) {
            //If we clicked on a pickup task item, pick it up.  Herp derp.
            Globals.Instance.WorldScript.PickupItem(this.Context.Actor, iScript.Item);
            this.Acted = true;
        } else if (cScript == this.Context.Actee) {
            //Presumably, we've done the right thing here.
            this.Acted = true;
            //If we clicked on a task chracter, check through possible tasks.
            switch (this.Context.Type) {
                case "collect":
                case "steal":
                case "loot":
                    this.Context.Actee.Inventory.Remove(this.Context.Item);
                    this.Context.Actor.Inventory.Add(this.Context.Item);
                    break;
                case "deliver":
                    this.Context.Actor.Inventory.Remove(this.Context.Item);
                    this.Context.Actee.Inventory.Add(this.Context.Item);
                    break;
                case "enter-combat":
                    Globals.Instance.WorldScript.LoadBattle(this.Context);
                    this.Context.Actee.Dead = true;
                    break;
                case "kill-by-item":
                case "revive-by-item":
                    this.Context.Actee.Dead = this.Context.Type == "kill-by-item";
                    break;
                default:
                    Debug.Log("Attempting to contextually act on an unknown task.");
                    this.Acted = false;
                    break;
            }

            this.Acted = true;
        }
    }

    public void OnInput(OTObject owner) {
        Debug.Log("Click detected.");
    }
	
}