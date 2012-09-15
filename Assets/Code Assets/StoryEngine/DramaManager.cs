using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

using UnityEngine;

using StoryEngine.Trace;
using WorldEngine;
using WorldEngine.Items;

namespace StoryEngine
{
	/// <summary>
	/// A simplistic drama manager.  This class interprets an Episode, altering
	/// the world such that the event's preconditions (if any) are satisfied.
	/// When the task is completed, the DramaManager advances the story.
	/// </summary>
	public class DramaManager
	{

		/// <summary>
		/// The script of tasks converted from an Episode.
		/// </summary>
		public List<Task> Script { get; private set; }

		public TaskNode scriptRoot;
		public List<TaskNode> history;
		/// <summary>
		/// The raw episode, in case that's needed.
		/// </summary>
		private Episode Episode { get; set; }
		/// <summary>
		/// The world in which the story occurs.
		/// </summary>
		private WorldScript World { get; set; }
		/// <summary>
		/// The current task that's being executed.
		/// </summary>
		public Task CurrentTask { get; set; }
		/// <summary>
		/// If true, the DramaManager will perform emergency repairs on the
		/// script for the player as well as NPCs.
		/// </summary>
		public bool RepairPlayer { get; set; }
		/// <summary>
		/// True, if the episode has finish executing.
		/// </summary>
		public bool EpisodeFinished { get; set; }

		/// <summary>
		/// Index of the current task.
		/// </summary>
		private int currentIndex = -1;

		/// <summary>
		/// Creates a drama manager for a given world.
		/// </summary>
		/// <param name="world">The world in which the story should occur.</param>
		public DramaManager (WorldScript world)
		{
			this.World = world;
			this.EpisodeFinished = true;
		}

		/// <summary>
		/// Loads the given XML serialized episode and begins execution.
		/// </summary>
		/// <param name="episodeData">An XML serialized episode.</param>
		/// <returns>True, if the episode was successfully loaded.</returns>
		public bool LoadEpisode (string episodeData)
		{
			try {
				this.Episode = Episode.Deserialize (episodeData);
			} catch (Exception) {
				//Something fucked up.
				return false;
			}
			
			String s = "";
			foreach (Link l in this.Episode.Links) {
				s += l.Source + " -> " + l.Target + "\n";
			}
			
			Debug.Log (s);
			
			this.scriptRoot = buildTaskGraph (this.Episode.Events, this.Episode.Links);
			this.history = new List<TaskNode> ();           

			this.InitializeWorld ();
			//Set the DM to start executing.
			this.EpisodeFinished = false;
			//Get the first task in there, so prevent an off-by-one null exception.
			this.AdvanceScript ();
			return true;
		}
		
		private TaskNode buildTaskGraph (List<StoryEvent> events, List<Link> links)
		{
			Dictionary<int, TaskNode> dictionary = new Dictionary<int, TaskNode> ();
			foreach (StoryEvent e in events) {
				int id = e.ID;
				Task t = new Task (e);
				TaskNode node = new TaskNode (t);
				dictionary.Add (id, node);
			}
			
			foreach (Link l in links) {				
				TaskNode parentNode = dictionary [l.Source];
				TaskNode childNode = dictionary [l.Target];
				if (!parentNode.children.Contains (childNode)) 
					parentNode.children.Add (childNode);
				if (!childNode.parents.Contains (parentNode)) 
					childNode.parents.Add (parentNode);
			}
			
			List<TaskNode> roots = new List<TaskNode> ();
			foreach (KeyValuePair<int, TaskNode> pair in dictionary) {
				if (pair.Value.parents.Count == 0)
					roots.Add (pair.Value);
			}
			// TODO: throw an exception when there are more than one root
			return roots.ElementAt (0);			
		}
		
		/// <summary>
		/// Unloads all episode data, minimally resets the world, and halts
		/// execution.
		/// </summary>
		public void UnloadEpisode ()
		{
			this.Episode = null;
			this.Script = null;

			//Drop all items so that later initializes won't fail.
			foreach (Item item in this.World.Items)
				this.World.DropItem (item);

			this.EpisodeFinished = true;
		}

		public void Update ()
		{
			//If the episode is finished, just spin.
			if (this.EpisodeFinished)
				return;

			// hack: all tasks have the same actor, which is the player
			PlayerController player = null;
			if (history.Count != 0)
				player = history [0].data.Actor.gameObject.GetComponent<PlayerController> ();
			else if (this.scriptRoot != null)
			{
				player = scriptRoot.data.Actor.gameObject.GetComponent<PlayerController> ();
				CharacterScript cs = scriptRoot.data.Actor.gameObject.GetComponent<CharacterScript> ();
				cs.ActiveTask = scriptRoot.data;
			}
			
			if (player != null) {
				List<TaskNode> active = player.activeTasks;
			
				WorldGUI wgui = Globals.Instance.WorldGUI;
				if (wgui.Dialogues.Count == 0 && active != null && (active.Exists (node => node.done) || active.Count == 0)) {
					history.AddRange (active.Where (node => node.done));
					AdvanceScript ();
				}
			}
		}

		/// <summary>
		/// Advances the script.  Not my most helpful comment ever.
		/// </summary>
		/// <returns>True, if the script's event have been exhausted.</returns>
		public void AdvanceScript ()
		{

			List<TaskNode> fringe = new List<TaskNode> ();		
			if (history.Count == 0) {
				fringe.Add (this.scriptRoot);
			} else {
				foreach (TaskNode h in history) {
					foreach (TaskNode kid in h.children) {
						if (!history.Contains (kid) && kid.parents.TrueForAll (p => history.Contains (p))) {
							fringe.Add (kid);
						}					
					}
				}
			}
			
			foreach (TaskNode node in fringe) {
				// process one task in the frigine
				PlayerController player = node.data.Actor.gameObject.GetComponent<PlayerController> ();
				if (player == null)
					return;
				
				// allow this to happen
				this.EmergencyRepair (node.data);
				//this.CurrentTask = next;
				//this.CurrentTask.Actor.ActiveTask = next;
				List<TaskNode> active = player.activeTasks;
				if(active == null) active = new List<TaskNode>();
				WorldGUI wgui = Globals.Instance.WorldGUI;
				
				String text = "";
				bool show = false;
				if (!active.Contains (node)) {
					active.Add (node);					
					active = active.Select (n => !n.done) as List<TaskNode>;
					player.activeTasks = active;
					Dialogue d = node.data.PreDialogue;
					wgui.Dialogues.Add (d);
					show = true;
					Debug.Log ("activated task: " + node.data.Type + ", " + node.data.Description);
				}
				
				wgui.DisplayDialogue = show;				

			}
		}
        

		/// <summary>
		/// Initializes the world, enforcing all Propositions in the Episode's
		/// initial state predicates.
		/// </summary>
		private void InitializeWorld ()
		{
			List<Proposition> healthStatuses = this.Episode.InitialState.HealthStatuses;
			List<Proposition> locations = this.Episode.InitialState.Locations;
			List<Proposition> possessions = this.Episode.InitialState.Possessions;

			Debug.Log ("Initializing " + healthStatuses.Count + " healths, " + locations.Count +
                " locations, and " + possessions.Count + " possessions.");

			//Enforce health values.
			foreach (Proposition prop in healthStatuses) {
				CharacterScript cs = this.World.GetCharacterByName (prop.GetValue ("character"));
				cs.Dead = "dead" == prop.GetValue ("health");
			}
			//Enforce locations.
			foreach (Proposition prop in locations) {
				//The 'noun' can be either a chracter OR an item. =S
				CharacterScript cs = this.World.GetCharacterByName (prop.GetValue ("noun"));
				Item item = this.World.GetItemByName (prop.GetValue ("noun"));
				LocaleScript ls = this.World.GetLocaleByName (prop.GetValue ("location"));
                
				//Figure out which one of these to do and move it.
				if (cs != null)
					cs.Location = ls.Location;
				else
					this.World.Teleport (item.Owner.GetComponent<ItemScript> (), ls);
			}
			//Enforce inventories.
			foreach (Proposition prop in possessions) {
				CharacterScript cs = this.World.GetCharacterByName (prop.GetValue ("character"));
				Item item = this.World.GetItemByName (prop.GetValue ("item"));
				this.World.PickupItem (cs, item);
			}
		}

		/// <summary>
		/// Validates a given task, forcing any open preconditions.  In theory,
		/// nothing should ever happen within this method - but just in case.
		/// </summary>
		/// <param name="task">The task to validate.</param>
		private void EmergencyRepair (Task task)
		{
			//Saving myself some typing.
			WorldScript ws = Globals.Instance.WorldScript;

			//Teleporting around the world is always valid.
			if (task.Type == "goto")
				return;

			//Make sure the actor and actee (if not null) are in the location.
			if (task.Actor.Locale != task.Locale && (task.Actor != ws.PartyCharacter || this.RepairPlayer)) {
				Debug.LogWarning (task.Actor.name + " not at locale " + task.Locale.name + " to execute " + task.Type + ".  Repairing.");
				ws.Teleport (task.Actor, task.Locale);
			}
			if (task.Actee != null && task.Actee.Locale != task.Locale && (task.Actee != ws.PartyCharacter || this.RepairPlayer)) {
				Debug.LogWarning (task.Actee.name + " not at locale " + task.Locale.name + " to execute " + task.Type + ".  Repairing.");
				ws.Teleport (task.Actee, task.Locale);
			}

			//Nothing more to check here.
			if (task.Type == "enter-combat")
				return;
            
			//If the item is supposed to be picked up, make sure it's on the ground at location.
			if (task.Type == "pickup") {
				ItemScript iScript = task.Item.Owner.GetComponent<ItemScript> ();
				if (iScript == null) {
					Debug.LogWarning (task.Item.Name + " not on ground to execute " + task.Type + ".  Repairing.");
					ws.DropItem (task.Item);
					iScript = task.Item.Owner.GetComponent<ItemScript> ();
				}
				if (iScript.Locale != task.Locale) {
					Debug.LogWarning (task.Item.Name + " not at locale " + task.Locale.name + " to execute " + task.Type + ".  Repairing.");
					ws.Teleport (iScript, task.Locale);
				}
			}
			//If the item is supposed to be taken, make sure the actee has it.
			if ((task.Type == "collect" || task.Type == "steal" || task.Type == "loot-corpse") &&
                !task.Actee.Inventory.Contains (task.Item)) {
				Debug.LogWarning (task.Actee.name + " does not own " + task.Item.Name + " to execute " + task.Type + ".  Repairing.");
				//Figure out if it needs to be picked up or traded.
				if (task.Item.Owner.GetComponent<ItemScript> () != null)
					ws.PickupItem (task.Actee, task.Item);
				else {
					task.Item.Owner.GetComponent<CharacterScript> ().Inventory.Remove (task.Item);
					task.Actee.Inventory.Add (task.Item);
				}
			}
			//If the item is supposed to be given or used, make sure the actor has it.
			if ((task.Type == "deliver" || task.Type == "kill-by-item" || task.Type == "revive-by-item") &&
                !task.Actor.Inventory.Contains (task.Item)) {
				Debug.LogWarning (task.Actor.name + " does not own " + task.Item.Name + " to execute " + task.Type + ".  Repairing.");
				//Figure out if it needs to be picked up or traded.
				if (task.Item.Owner.GetComponent<ItemScript> () != null)
					ws.PickupItem (task.Actor, task.Item);
				else {
					task.Item.Owner.GetComponent<CharacterScript> ().Inventory.Remove (task.Item);
					task.Actor.Inventory.Add (task.Item);
				}
			}

			//....I think that's everything....
		}

        #region Unused, complicated stuff that doesn't work. But I didn't want to delete it.
		///// <summary>
		///// Alters the world state, such that a given task can occur.  In other
		///// world, solves all preconditions for a task.
		///// </summary>
		///// <param name="task">The task that needs to be completed.</param>
		//public void AlterWorld(Task task) {
		//    //Tell the actor what to do.
		//    //task.Actor.ActiveTask = task;

		//    /*
		//     * TODO this is Alex's grand list of driving Michael crazy.
		//     * (1) need WaitFor function to check conditional; possibly conjunctions?
		//     *      (a) character inventory contains item
		//     *      (b) character at a locale
		//     *      (c) character "interacts" with something
		//     * (2) need pre + post dialog tasks that can take parameters (text)
		//     * (3) need locale to include positions for checking (class with a transform probably)
		//     * (4) how do we check who should deliver the dialog? do we just fix it to a given character?
		//     *     the dialog tasks may become a method to select the appropriate target and give it...
		//     * (5) need combat events to take more parameters (enemy team composition, potentially variable length)
		//     * (6) see list of not implemented tasks and methods at the bottom, not sure where these should live
		//     */


		//    // set tasks for agents via the switch
		//    // Narration action in BT
		//    switch (task.Type) {
		//        case("goto"):
		//            // NPCs need to be teleported first, then same sequence
		//            if (task.Actor.Name != "player") {
		//                this.Teleport(task.Actor, task.Locale); // TODO must have two locale options here...
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            // wait for player to get there or move
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // task.Actor.transform.position == task.Locale
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actor, task.Locale);
		//            }

		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog               
		//            break;
		//        case("steal"):
		//            this.Teleport(task.Actee, task.Locale); // set up person to be stolen from
		//            this.MoveItem(task.Actee, task.Item); // give item
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // should be saying to move to target
		//                task.Actee.ActiveTask = new Task("give", task.Actee, task.Actor, task.Item, task.Locale); // theft = give victim's item to thief
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case("collect"):
		//            goto case("steal"); // identical actions, only difference is dialog shown
		//        case("retrieve"):
		//            // only player can retrieve ATM <-- THEN WHY THE FUCK IS IT IN HERE?!
		//            this.Teleport(task.Actee, task.Locale); // set up person to retrieve item
		//            this.MoveItem(task.Locale, task.Item); // move item to locale
		//            this.WaitFor(); // task.Actor.transform.position == task.Locale || player reaches locale
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            task.Actee.ActiveTask = new Task("moveto", task.Actee, task.Actee, task.Item, task.Locale); // move to item position
		//            task.Actee.ActiveTask = new Task("pickup", task.Actee, task.Actee, task.Item, task.Locale); // get item
		//            task.Actee.ActiveTask = new Task("moveto", task.Actee, task.Actor, task.Locale); // move to player position
		//            task.Actee.ActiveTask = new Task("give", task.Actee, task.Actor, task.Item, task.Locale); // give item to player/actor
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case("pickup"):
		//            this.MoveItem(task.Locale, task.Item); // move item to locale
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            if (task.Actor.Name == "player") {
		//                WaitFor(); // player.inventory.contains(item)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Item, task.Locale); // move to item
		//                this.MoveItem(task.Actee, task.Item); //TODO replace with explicit command to "pick up" the item or just teleport it into NPC inventory?
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case("loot-corpse"):
		//            this.Teleport(task.Actee, task.Locale); // set up corpse
		//            this.MoveItem(task.Actee, task.Item); // set up corpse item
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee
		//                this.MoveItem(task.Actee, task.Item); //TODO replace with explicit command to "pick up" the item or just teleport it into NPC inventory?
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case("deliver"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                this.MoveItem(task.Actor, task.Item); // give item to NPC
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // NOT actor.inventory.contains(task.item)
		//                this.WaitFor(); // actee.inventory.contains(task.item)
		//            } else {
		//                task.Actor.ActiveTask = new Task("give", task.Actor, task.Actee, task.Item, task.Locale); // give item to player/actee
		//            }

		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("hurt-by-item"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                this.MoveItem(task.Actor, task.Item); // give item to NPC
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // NOT actor.inventory.contains(task.item)
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee
		//                task.Actor.ActiveTask = new Task("consumeitem", task.Actor, task.Actee, task.Item, task.Locale); // actor uses item on actee
		//            }
		//            this.ChangeHealth(task.Actee, "injured"); // change health state of actee after item was used
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("heal-by-item"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                this.MoveItem(task.Actor, task.Item); // give item to NPC
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // NOT actor.inventory.contains(task.item)
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee
		//                task.Actor.ActiveTask = new Task("consumeitem", task.Actor, task.Actee, task.Item, task.Locale); // actor uses item on actee
		//            }
		//            this.ChangeHealth(task.Actee, "healthy"); // change health state of actee after item was used
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("kill-by-item"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // actor.inventory.contains(task.item)
		//            } else {
		//                this.MoveItem(task.Actor, task.Item); // give item to NPC
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // NOT actor.inventory.contains(task.item)
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee
		//                task.Actor.ActiveTask = new Task("consumeitem", task.Actor, task.Actee, task.Item, task.Locale); // actor "uses" item on actee
		//            }
		//            this.ChangeHealth(task.Actee, "dead"); // change health state of actee after item was used
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("rescue"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name != "player") {
		//                task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // task.Actor.transform.position == task.Locale
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//                // teleport target to safety?
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("kidnap"):
		//            goto case ("rescue"); // identical actions, different text...
		//        case ("buy"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name != "player") {
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // task.Actor.transform.position == task.Locale
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }

		//            //TODO dialog for buying?

		//            task.Actee.ActiveTask = new Task("give", task.Actee, task.Actor, task.Item, task.Locale); // give item to player/actor
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialogbreak;
		//            break;
		//        case ("barter"):
		//            goto case ("buy"); // identical actions, different text
		//        case ("enter-combat"):
		//            this.Teleport(task.Actee, task.Locale); // set up target
		//            if (task.Actor.Name != "player") {
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog

		//            if (task.Actor.Name == "player") {
		//                this.WaitFor(); // interactwith(task.Actor, task.Actee)
		//            } else {
		//                task.Actee.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Locale); // move to actee position
		//            }
		//            this.StartCombat(task.Actor, task.Actee, task.Item);

		//            task.Actor.ActiveTask = new Task("dialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            this.ChangeHealth(task.Actee, "dead");
		//            break;
		//        // AEZ: not clear whether I need to implement these or they are methods...
		//        case ("dialog"): //TODO
		//            task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            task.Actor.ActiveTask = new Task("postdialog", task.Actor, task.Actee, task.Item, task.Locale); // post dialog
		//            break;
		//        case ("moveto"): //TODO
		//            // go to a location
		//            break;
		//        case ("give"): //TODO
		//            task.Actor.ActiveTask = new Task("moveto", task.Actor, task.Actee, task.Item, task.Locale); // pre dialog
		//            // transfer item from actor's inventory to actee's inventory
		//            break;
		//        case ("consumeitem"): //TODO
		//            // "destroys" item by moving it to hidden location; may be DM action
		//            break;
		//        case ("postdialog"): //TODO
		//            // display the dialog
		//            break;
		//        default:
		//            break;
		//    }
		//}





		///// <summary>
		///// Start a battle including at least the actor and actee. 
		///// Additional combatants should be drawn from the "dummy" variable.
		///// </summary>
		///// <param name="actor">actor who initiates combat, typically player</param>
		///// <param name="actee">actee who will be killed by combat</param>
		///// <param name="dummy">dummy filler for parameters indicating remainder of actee's team</param>
		//private void StartCombat(CharacterScript actor, CharacterScript actee, Item dummy) {
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Alter the health state of the actor to health.
		///// </summary>
		///// <param name="actor">actor to have health modified</param>
		///// <param name="health">health value to set for actor</param>
		//private void ChangeHealth(CharacterScript actor, string health) {
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Move item to a random position within a locale from wherever the item currently is.
		///// </summary>
		///// <param name="locale">locale to place the item in</param>
		///// <param name="item">item to move</param>
		//private void MoveItem(string locale, Item item) {
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Moves the item from it's current location in the world into the inventory of the actor.
		///// Assumes items are unique
		///// </summary>
		///// <param name="actor">actor to receive item in inventory</param>
		///// <param name="item">item to be moved</param>
		//private void MoveItem(CharacterScript actor, Item item) {
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Teleports the actor to the given locale.
		///// </summary>
		///// <param name="actor">actor to teleport to a locale</param>
		///// <param name="locale">name of the locale to teleport to</param>
		//private void Teleport(CharacterScript actor, string locale) {
		//    throw new NotImplementedException();
		//}

		///// <summary>
		///// Waits for the world state to meet some condition.
		///// TODO needs to take in condition for checking...
		///// </summary>
		//private void WaitFor() {
		//    throw new NotImplementedException();
		//}

        #endregion
	}
}