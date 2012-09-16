using UnityEngine;

using RAIN.Path;
using RAIN.Sensors;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using System.Text;
using System.IO;
using StoryEngine.Trace;

public class PlayerController : MonoBehaviour
{

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
	
	public List<TaskNode> activeTasks { get; set; }

	private Task m_Context;
	/// <summary>
	/// Gets or sets the non-context actor.
	/// </summary>
	public CharacterScript NonContextActor {
		get {
			return this.nc_actor;
		}
		set {
			this.nc_actor = value;
		}
	}

	private CharacterScript nc_actor;
	/// <summary>
	/// Gets or sets the non-context actee.
	/// </summary>
	public CharacterScript NonContextActee {
		get {
			return this.nc_actee;
		}
		set {
			this.nc_actee = value;
		}
	}

	private CharacterScript nc_actee;
	/// <summary>
	/// Gets or sets the non-context item.
	/// </summary>
	public ItemScript NonContextItem {
		get {
			return this.nc_item;
		}
		set {
			this.nc_item = value;
		}
	}

	private ItemScript nc_item;
	public bool PostDialogueStarted = false;
	
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
	void Start ()
	{
		//this.gameObject.GetComponent<OTAnimatingSprite>().onInput = this.OnInput;
		activeTasks = new List<TaskNode> ();
		Acted = false;
	}

	/// <summary>
	/// Called once per frame.  Assigns move targets for the player party and
	/// handles task-contextual actions.
	/// </summary>
	public void Update ()
	{
		
		//If last frame's mouse object is a character, mark it as not hovered.
		if (this.MouseObject != null) {
			CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript> ();
			if (cScript != null)
				cScript.ExpandedLabelInfo = false;
		}

		//Get the object under mouse.
		this.MouseCast ();

		//If the mouse object is a character, mark it as hovered.
		if (this.MouseObject != null) {
			CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript> ();
			if (cScript != null)
				cScript.ExpandedLabelInfo = true;
		}

		//Handle contextual left clicks.
		if (Input.GetMouseButtonDown (0)) {
			//Debug.Log("random message");
			this.HandleClick ();	
		}
		
		
		
		
		//If Action Selection GUI is done and selection made.
		if (Globals.Instance.WorldGUI.selectedAction != -1) {
			Globals.Instance.WorldGUI.DisplayActionOptions = false;
			
			//"Talk", "Pick Up Item", "Trade Item", "Give Item", "Take Item", "Fight", "Steal Item", "Loot Item", "Kill With Item", "Revive With Item", "Cancel"
			string[] actions = new string[11]{"talk", "pickup", "trade", "deliver", "collect", "enter-combat", "steal", "loot", "kill-by-item", "revive-by-item", "cancel"};
			string selectedAction = actions [Globals.Instance.WorldGUI.selectedAction];
			
			switch (selectedAction) {
			case "trade":
			case "collect":
			case "steal":
			case "loot":
			case "deliver":
			case "kill-by-item":					
			case "revive-by-item":
				Globals.Instance.WorldGUI.DisplayActorInventory = true;
				Globals.Instance.WorldGUI.DisplayActeeInventory = true;
				break;
			default:
				Globals.Instance.WorldGUI.DisplayActorInventory = false;
				Globals.Instance.WorldGUI.DisplayActeeInventory = false;
				break;
			}
			
			if ((!Globals.Instance.WorldGUI.DisplayActorInventory || !Globals.Instance.WorldGUI.DisplayActeeInventory) || (Globals.Instance.WorldGUI.selectedActorInventoryItem > -1 && Globals.Instance.WorldGUI.selectedActeeInventoryItem > -1)) {
				this.ActInContext ();
				
				Globals.Instance.WorldGUI.DisplayActorInventory = false;
				Globals.Instance.WorldGUI.DisplayActeeInventory = false;
				Globals.Instance.WorldGUI.Actor = Globals.Instance.WorldGUI.Actee = NonContextActor = NonContextActee = null;
				NonContextItem = null;
			}
		}

		//Providing a 'clean' way to stop movement.
		if (this.Pathing) {
			PathManager pather = this.gameObject.GetComponentInChildren<PathManager> ();

			//If we're pathing to an entity, rather than a point, check remaining distance.
			if (this.approachEntity != null) {
				float dist2 = Vector3.SqrMagnitude (this.approachEntity.transform.position - this.transform.position);
				if (dist2 < this.EntityApproachRange * this.EntityApproachRange) {
					//Close enough, halt movement and attempt contextual action.
					pather.moveTarget.TransformTarget = this.transform;
					
					//If action not selected from Action Selection GUI, set flag to pop up GUI.
					if ((Globals.Instance.WorldGUI.selectedAction == -1) && (this.NonContextActor != this.NonContextActee || this.NonContextActor == null || this. NonContextActee == null)) {
						this.NonContextActor = this.gameObject.GetComponent<CharacterScript> ();
						this.NonContextActee = this.approachEntity.GetComponent<CharacterScript> ();
						this.NonContextItem = this.approachEntity.GetComponent<ItemScript> ();
						Globals.Instance.WorldGUI.Actor = this.NonContextActor;
						Globals.Instance.WorldGUI.Actee = this.NonContextActee;
						
						Globals.Instance.WorldGUI.DisplayActionOptions = true;
					}

					this.approachEntity = null;
					this.Pathing = false;
				}
			} else {
				float dist2 = Vector3.SqrMagnitude (pather.moveTarget.VectorTarget - this.transform.position);
				//If we're within the close enough distance, 'drop' the move vector.
				if (dist2 <= pather.closeEnoughDistance * pather.closeEnoughDistance) {
					pather.moveTarget.TransformTarget = this.transform;
					this.Pathing = false;
				}
			}
		}
		
		if (this.activeTasks != null) {
			//Check for completion of GoTo tasks each frame.
			foreach (TaskNode tn in this.activeTasks) {
				Task task = tn.data;
				if (task.Type.Equals ("goto") && task.Actor.Locale == task.Locale && !tn.done) {
					TaskDone (tn);
				}
			}
		}

	}
	
	/// <summary>
	/// Performs a raycast into the world from the current mouse position while
	/// yeilding to the GUI.
	/// </summary>
	private void MouseCast ()
	{
		//Yeild to the GUI.
		if (Globals.Instance.WorldGUI.MouseOverGUI)
			return;
		
		
		//Get mouse ray.
		Ray ray = Camera.mainCamera.ScreenPointToRay (Input.mousePosition);

		//Get ray collision with the mask.
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 1000f, this.MouseMask)) {
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
	private void HandleClick ()
	{
		//Ignore null, in case that happens.
		if (this.MouseObject == null)
			return;
		//Yield to GUI.
		if (Globals.Instance.WorldGUI.MouseOverGUI)
			return;

		//Well we clicked on something, we'll need this shortly.
		PathManager pather = this.gameObject.GetComponentInChildren<PathManager> ();

		//Get something, find out if it's a character, item, or the terrain.
		CharacterScript cScript = this.MouseObject.GetComponent<CharacterScript> ();
		ItemScript iScript = this.MouseObject.GetComponent<ItemScript> ();

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
	private void ActInContext ()
	{
		//Well, obviously we're not trying to do anything if there's no task. //WE ARE!!!
//        if (this.Context == null)
//		{
//			Globals.Instance.WorldGUI.selectedAction = -1;
//			return;
//		}
		
		string[] actions = new string[11]{"talk", "pickup", "trade", "deliver", "collect", "enter-combat", "steal", "loot", "kill-by-item", "revive-by-item", "cancel"};
		string selectedAction = actions [Globals.Instance.WorldGUI.selectedAction];
		
		Item selectedActorItem = null;
		Item selectedActeeItem = null;
		
		if (Globals.Instance.WorldGUI.selectedActorInventoryItem > 0) {
			selectedActorItem = this.NonContextActor.Inventory [Globals.Instance.WorldGUI.selectedActorInventoryItem - 1];
		}
		if (Globals.Instance.WorldGUI.selectedActeeInventoryItem > 0) {
			selectedActeeItem = this.NonContextActee.Inventory [Globals.Instance.WorldGUI.selectedActeeInventoryItem - 1];
		}
		
		//Debug.Log("Task Description: " + this.Context.Description);

		//Get the chracter script or item script that was clicked on. 
//        CharacterScript cScript = this.approachEntity.GetComponent<CharacterScript>();
//        ItemScript iScript = this.approachEntity.GetComponent<ItemScript>();
		
		if (this.NonContextItem != null && selectedAction == "pickup") {
			//If we clicked on a pickup task item, pick it up.  Herp derp.
			Globals.Instance.WorldScript.PickupItem (NonContextActor, this.NonContextItem.Item);
			foreach(TaskNode tn in activeTasks)
			{
				if (tn.data.Type.Equals ("pickup") && tn.data.Item == this.NonContextItem.Item)
					TaskDone(tn);
			}
		
		} else {
			bool correctItemNotSelected = false;
            
			//"talk", "pickup", "trade", "deliver", "collect", "enter-combat", "steal", "loot", "kill-by-item", "revive-by-item"
			//If we clicked on a task chracter, check through possible tasks.
			switch (selectedAction) {
			case "talk":
					//TODO
				break;
			case "trade":
				if (selectedActorItem == null || selectedActeeItem == null) {
					correctItemNotSelected = true;
				} else {
					this.NonContextActee.Inventory.Remove (selectedActeeItem);
					this.NonContextActor.Inventory.Add (selectedActeeItem);
					this.NonContextActor.Inventory.Remove (selectedActorItem);
					this.NonContextActee.Inventory.Add (selectedActorItem);
				}
				break;
			case "collect":
			case "steal":
			case "loot":
				if (selectedActeeItem == null) {
					correctItemNotSelected = true;
				} else {
					this.NonContextActee.Inventory.Remove (selectedActeeItem);
					this.NonContextActor.Inventory.Add (selectedActeeItem);
				}
				break;
			case "deliver":
				if (selectedActorItem == null) {
					correctItemNotSelected = true;
				} else {
					this.NonContextActor.Inventory.Remove (selectedActorItem);
					this.NonContextActee.Inventory.Add (selectedActorItem);
				}
				break;
			case "enter-combat":
				Globals.Instance.WorldScript.LoadBattle (new Task ("enter-combat", this.NonContextActor, this.NonContextActee, new Item ("FightItem", "Item For Fighting"), this.NonContextActor.Locale));
				this.NonContextActee.Dead = true;
				break;
			case "kill-by-item":
			case "revive-by-item":
				if (selectedActorItem == null) {
					correctItemNotSelected = true;
				} else {
					this.NonContextActee.Dead = selectedAction == "kill-by-item";
				}
				break;
			default:
				Debug.Log ("Attempting to contextually act on an unknown task.");
				break;
			}
			
			//If we've done the right thing here.
			foreach(TaskNode tn in this.activeTasks)
			{
				if (this.NonContextActee == tn.data.Actee && tn.data.Type.Equals (selectedAction) && !correctItemNotSelected)
					TaskDone(tn);
			}			

		}
		
		Globals.Instance.WorldGUI.selectedAction = -1;
		Globals.Instance.WorldGUI.selectedActorInventoryItem = -1;
		Globals.Instance.WorldGUI.selectedActeeInventoryItem = -1;
	}
	
	public void TaskDone (TaskNode task)
	{
		Debug.Log ("Task: " + task.data.Type + " done.");
		task.done = true;
		//Get the gui script and task.
		WorldGUI wgui = Globals.Instance.WorldGUI;

		//Set the dialogue and flag the GUI to display it.
		wgui.Dialogues.Add (task.data.PostDialogue);
		wgui.DisplayDialogue = true;		
		PostDialogueStarted = true;
	}
	
	public void PostDialogueTaskDone ()
	{
		//CharacterScript charScript = this.Context.Actor.gameObject.GetComponent<CharacterScript>();
		PostDialogueStarted = false;
		//charScript.ActiveTask = null;
	}

	public void OnInput (OTObject owner)
	{
		Debug.Log ("Click detected.");
	}
	
}