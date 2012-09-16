using UnityEngine;
using System.IO;

using StoryEngine;
using WorldEngine;
using WorldEngine.Items;
using System.Collections.Generic;
using System.Collections;

public class WorldGUI : MonoBehaviour
{

    #region Attributes
	/// <summary>
	/// A reference to the world script running within the game.
	/// </summary>
	public WorldScript WorldScript = null;
	/// <summary>
	/// The skin to use for the GUI.
	/// </summary>
	public GUISkin Skin;
	/// <summary>
	/// The size of the font used for the various GUI windows.
	/// </summary>
	public int FontSize = 16;

	/// <summary>
	/// The size of the episode selection window.
	/// </summary>
	public Vector2 EpisodeBounds = new Vector2 (256, 512);
	/// <summary>
	/// Flag for displaying the episodes window.
	/// </summary>
	public bool  DisplayEpisodes { get; private set; }
	/// <summary>
	/// One-frame buffer for DisplayEpisodes bool, as OnGUI() is called faster than Update().
	/// </summary>
	private bool displayEpisodesBuffer = false;
	/// <summary>
	/// The current scroll position of the episode window.
	/// </summary>
	private Vector2 epScroll = Vector2.zero;
	/// <summary>
	/// If true, the episode selection dialog will check folders next to the
	/// Unity executable, rather than using internally stored assets.
	/// </summary>
	private bool externalEpisode = false;
	/// <summary>
	/// The currently selected episode.
	/// </summary>
	private int selectedEpisode = -1;
	/// <summary>
	/// True, if an attempt was made to load an invalid external episode file.
	/// </summary>
	private bool validEpisode = true;

	/// <summary>
	/// X, Y offset from upper left for drawing the dialog window.
	/// </summary>
	public Vector2 DialogueOffset = Vector2.zero;
	/// <summary>
	/// The size of the dialog window.
	/// </summary>
	public float DialogueWidth = 350;
	/// <summary>
	/// Flag for displaying the inventory window.
	/// </summary>
	public bool DisplayDialogue { get; set; }
	/// <summary>
	/// The current Dialogue to display.
	/// </summary>
	public List<Dialogue> Dialogues {get; set;}
	/// <summary>
	/// One-frame buffer for DisplayDialog bool, as OnGUI() is called faster than Update().
	/// </summary>
	private bool displayDialogueBuffer = false;

	/// <summary>
	/// X, Y offset from upper right for drawing the inventory window.
	/// </summary>
	public Vector2 InventoryOffset = Vector2.zero;
	/// <summary>
	/// The size of the inventory window.
	/// </summary>
	public float InventoryWidth = 350;
	/// <summary>
	/// Flag for displaying the inventory window.
	/// </summary>
	public bool DisplayInventory { get; private set; }
	/// <summary>
	/// One-frame buffer for DisplayInventory bool, as OnGUI() is called faster than Update().
	/// </summary>
	private bool displayInventoryBuffer = false;
	/// <summary>
	/// The current scrolling position of the inventory window.
	/// </summary>
	private Vector2 invScroll = Vector2.zero;

	/// <summary>
	/// X, Y offset from upper right for drawing the story window.
	/// </summary>
	public Vector2 StoryOffset = Vector2.zero;
	/// <summary>
	/// The size of the story window.
	/// </summary>
	public float StoryWidth = 350;
	/// <summary>
	/// Flag for displaying the story window.
	/// </summary>
	public bool DisplayStory { get; private set; }
	/// <summary>
	/// One-frame buffer for DisplayStory bool, as OnGUI() is called faster than Update().
	/// </summary>
	private bool displayStoryBuffer = false;
	/// <summary>
	/// The current scrolling position of the story window.
	/// </summary>
	private Vector2 storyScroll = Vector2.zero;

	/// <summary>
	/// The y offset of the locale window from the bottom of the screen.
	/// </summary>
	public int LocaleOffset = 16;
	/// <summary>
	/// The width and height of the locale window.
	/// </summary>
	public Vector2 LocaleBounds = new Vector2 (512, 320);
	/// <summary>
	/// Flag for displaying the locale window.
	/// </summary>
	public bool DisplayLocales {
		get { return this.m_displayLocales; }
		set {
			this.m_displayLocales = value;
			this.selectedLocale = -1;
		}
	}
	/// <summary>
	/// Private member variable.  Use WorldGUI.DisplayLocales instead.
	/// </summary>
	private bool m_displayLocales = false;
	/// <summary>
	/// One-frame buffer for DisplayLocales bool, as OnGUI() is called faster than Update().
	/// </summary>
	private bool displayLocalesBuffer = false;
	/// <summary>
	/// The current scrolling position of the inventory window.
	/// </summary>
	private Vector2 localeScroll = Vector2.zero;
	/// <summary>
	/// The selected locale index.
	/// </summary>
	private int selectedLocale = -1;

	/// <summary>
	/// True, if the mouse is currently over the GUI.
	/// </summary>
	public bool MouseOverGUI {
		get {
			Vector2 mouse = Input.mousePosition;
			//Because the mouse origin is lower left, while screen is upper left... >_>
			mouse.y = Screen.height - mouse.y;
			return (this.episodeBounds.Contains (mouse) && this.displayEpisodesBuffer) ||
                   (this.dialogueBounds.Contains (mouse) && this.displayDialogueBuffer) ||
                   (this.inventoryBounds.Contains (mouse) && this.displayInventoryBuffer) ||
                   (this.storyBounds.Contains (mouse) && this.displayStoryBuffer) ||
                   (this.localeBounds.Contains (mouse) && this.displayLocalesBuffer) ||
                   (this.actionBounds.Contains (mouse) && this.displayActionOptions) ||
                   (this.actorInventoryBounds.Contains (mouse) && this.displayActorInventory) ||
                   (this.acteeInventoryBounds.Contains (mouse) && this.displayActeeInventory) ||
                   this.DisplayThanks;
		}
	}

	public Vector2 ThanksBounds = new Vector2 (320, 256);    
	/// <summary>
	/// Displays the GTFO screen.
	/// </summary>
	public bool DisplayThanks { get; set; }

	/// <summary>
	/// Reference to the default skin for the purposes of swapping.
	/// </summary>
	private GUISkin defaultSkin = null;
	/// <summary>
	/// The bounding rectangle for the episode window.
	/// </summary>
	private Rect episodeBounds;
	/// <summary>
	/// The bounding rectangle for the dialog window.
	/// </summary>
	private Rect dialogueBounds;
	/// <summary>
	/// The bounding rectangle for the story window.
	/// </summary>
	private Rect storyBounds;
	/// <summary>
	/// The bounding rectangle for the inventory window.
	/// </summary>
	private Rect inventoryBounds;
	/// <summary>
	/// The bounding rectangle for the locale window.
	/// </summary>
	private Rect localeBounds;
	/// <summary>
	/// The bounding rectangle for the thanks window.
	/// </summary>
	private Rect thanksBounds;
	
    #endregion
	
	//Action Selection dialogue box
	public bool DisplayActionOptions { get; set; }

	private bool displayActionOptions;
	public Vector2 ActionBounds = new Vector2 (480, 320);
	private Rect actionBounds;
	public int selectedAction;
	
	//Actor Inventory Selection dialogue box
	public bool DisplayActorInventory { get; set; }

	private bool displayActorInventory;
	public Vector2 ActorInventoryBounds = new Vector2 (320, 256);
	private Rect actorInventoryBounds;
	public int selectedActorInventoryItem;
	public CharacterScript Actor;
	
	//Actee Inventory Selection dialogue box
	public bool DisplayActeeInventory { get; set; }

	private bool displayActeeInventory;
	public Vector2 ActeeInventoryBounds = new Vector2 (320, 256);
	private Rect acteeInventoryBounds;
	public int selectedActeeInventoryItem;
	public CharacterScript Actee;
	
	// Use this for initialization
	void Start ()
	{
		this.DisplayEpisodes = true;
		this.DisplayDialogue = false;
		this.DisplayInventory = false;
		this.DisplayStory = false;
		this.DisplayLocales = false;
		this.DisplayThanks = false;
		this.DisplayActionOptions = false;
		this.DisplayActorInventory = false;
		this.DisplayActeeInventory = false;
		selectedAction = -1;
		selectedActorInventoryItem = -1;
		selectedActeeInventoryItem = -1;
		Dialogues = new List<Dialogue> ();
	}

	// Update is called once per frame
	void Update ()
	{
		//Buffer the booleans to prevent issues with mouse clicking as windows disappear.
		this.displayEpisodesBuffer = this.DisplayEpisodes;
		this.displayDialogueBuffer = this.DisplayDialogue;
		this.displayInventoryBuffer = this.DisplayInventory;
		this.displayStoryBuffer = this.DisplayStory;
		this.displayLocalesBuffer = this.DisplayLocales;
		this.displayActionOptions = this.DisplayActionOptions;
		this.displayActorInventory = this. DisplayActorInventory;
		this.displayActeeInventory = this. DisplayActeeInventory;
		//Check to see if the DM is finished running.
		this.DisplayEpisodes = Globals.Instance.DMScript.DramaManager.EpisodeFinished;

		//Check for inventory keypress.
		if (Input.GetKeyDown (KeyCode.I)) {
			this.DisplayInventory = !this.DisplayInventory;
		}

		//Check for story keypress.
		if (Input.GetKeyDown (KeyCode.S)) {
			this.DisplayStory = !this.DisplayStory;
		}
	}

	public void OnGUI ()
	{
		//Grab current skin and overwrite.
		this.defaultSkin = GUI.skin;
		GUI.skin = this.Skin;

		//Quick font size adjustment.
		int size = this.Skin.GetStyle ("LightText").fontSize;
		this.Skin.GetStyle ("LightText").fontSize = this.FontSize;
		this.Skin.GetStyle ("LegendaryText").fontSize = this.FontSize;
		this.Skin.GetStyle ("CursedText").fontSize = this.FontSize;

		//Calculate the window bounds.
		this.episodeBounds = new Rect (
            (Screen.width - this.EpisodeBounds.x) / 2,
            (Screen.height - this.EpisodeBounds.y) / 2,
            this.EpisodeBounds.x,
            this.EpisodeBounds.y
            );
		this.dialogueBounds = new Rect (
            this.DialogueOffset.x,
            this.DialogueOffset.y,
            this.DialogueWidth,
            Screen.height / 2 - 2 * this.DialogueOffset.y);
		this.inventoryBounds = new Rect (
            this.InventoryOffset.x,
            Screen.height / 2 + this.InventoryOffset.y,
            this.InventoryWidth,
            Screen.height / 2 - 2 * this.InventoryOffset.y);
		this.storyBounds = new Rect (
            Screen.width - this.StoryOffset.x - this.StoryWidth,
            this.StoryOffset.y,
            this.StoryWidth,
            Screen.height - 2 * this.StoryOffset.y);
		this.localeBounds = new Rect (
            Screen.width / 2 - this.LocaleBounds.x / 2,
            Screen.height - this.LocaleBounds.y - this.LocaleOffset,
            this.LocaleBounds.x,
            this.LocaleBounds.y);
		this.thanksBounds = new Rect (
            (Screen.width - this.ThanksBounds.x) / 2,
            Screen.height - this.ThanksBounds.y,
            this.ThanksBounds.x,
            this.ThanksBounds.y);
		this.actionBounds = new Rect (
			(Screen.width - this.ActionBounds.x * 1.5f) / 2,
            Screen.height - this.ActionBounds.y * 1.25f,
            this.ActionBounds.x * 1.5f,
            this.ActionBounds.y * 1.25f
			);
		this.actorInventoryBounds = new Rect (
			(Screen.width - this.ActorInventoryBounds.x) / 4 - 50,
            Screen.height - this.ActorInventoryBounds.y,
            this.ActorInventoryBounds.x * 3,
            this.ActorInventoryBounds.y
			);
		this.acteeInventoryBounds = new Rect (
			(Screen.width - this.ActeeInventoryBounds.x) * 3 / 4 + 50,
            Screen.height - this.ActeeInventoryBounds.y,
            this.ActeeInventoryBounds.x,
            this.ActeeInventoryBounds.y
			);

		//Don't draw the window if we're not supposed to be displaying.
		if (this.DisplayEpisodes)
			GUI.Window (0, this.episodeBounds, this.DrawEpisodes, "", this.defaultSkin.window);
		if (this.displayDialogueBuffer)
			GUI.Window (1, this.dialogueBounds, this.DrawDialogue, "");
		if (this.displayInventoryBuffer)
			GUI.Window (2, this.inventoryBounds, this.DrawInventory, "");
		if (this.displayStoryBuffer)
			GUI.Window (3, this.storyBounds, this.DrawStory, "");
		//if (this.displayLocalesBuffer) GUI.Window(4, this.localeBounds, this.DrawLocales, "");
		if (this.DisplayThanks)
			GUI.Window (5, this.thanksBounds, this.DrawThanks, "");
		if (this.displayActionOptions)
			GUI.Window (6, this.actionBounds, this.drawActions, "");
		if (this.displayActorInventory)
			GUI.Window (7, this.actorInventoryBounds, this.drawActorInventory, "");
		if (this.displayActeeInventory)
			GUI.Window (8, this.acteeInventoryBounds, this.drawActeeInventory, "");
		//Reset font sizes.
		this.Skin.GetStyle ("LightText").fontSize = size;
		this.Skin.GetStyle ("LegendaryText").fontSize = size;
		this.Skin.GetStyle ("CursedText").fontSize = size;

		//Replace previous skin.
		GUI.skin = this.defaultSkin;
	}

    #region Episode display...
	/// <summary>
	/// Displays the episode selection window.
	/// </summary>
	/// <param name="windowID">The Unity window ID.</param>
	private void DrawEpisodes (int windowID)
	{
		//Fetch all the episodes in the system.
		string[] episodeNames = this.GetEpisodeNames ();

		GUI.skin = this.defaultSkin;
		GUILayout.BeginVertical ();

		GUILayout.Label ("Choose an episode to load...");

		if (!this.validEpisode) {
			GUILayout.Label ("The selected file was invalid, please select a valid Episode XML file.");
		}

		this.epScroll = GUILayout.BeginScrollView (this.epScroll, false, false);
		this.selectedEpisode = GUILayout.SelectionGrid (this.selectedEpisode, episodeNames, 1);
		GUILayout.EndScrollView ();

		GUILayout.Space (16);
		bool external = GUILayout.Toggle (this.externalEpisode, "Use external episodes.");
		GUILayout.Space (4);
		bool load = GUILayout.Button ("Load Selected");

		GUILayout.EndVertical ();
		GUI.skin = this.Skin;

		//If external flag has been changed, reset indices and scroll bars.
		if (external != this.externalEpisode) {
			this.selectedEpisode = -1;
			this.epScroll = Vector2.zero;
			//Don't forget this. >_>
			this.externalEpisode = external;
		}
		//Load in the episode if the load button was pushed and we have a selection.
		if (load && this.selectedEpisode != -1) {
			string episodeData = this.GetEpisodeData ();
			this.validEpisode = Globals.Instance.DMScript.DramaManager.LoadEpisode (episodeData);
			//If it's good, let's move on.
			if (this.validEpisode)
				this.DisplayEpisodes = false;
		}
	}

	private string[] GetEpisodeNames ()
	{
		string[] names = null;
		//Two completely different ways of doing this...
		if (this.externalEpisode) {
			//Get the external folder contents, creating the folder if needed.
			bool exists = Directory.Exists ("Episodes");
			if (!exists)
				Directory.CreateDirectory ("Episodes");
			names = Directory.GetFiles ("Episodes", "*.xml");
		} else {
			//Get all the episodes in Resources/Episodes within the Project Assets.
			Object[] episodes = Resources.LoadAll ("Episodes", typeof(TextAsset));
			names = new string[episodes.Length];
			for (int i = 0; i < episodes.Length; i++)
				names [i] = episodes [i].name;
		}
		return names;
	}

	private string GetEpisodeData ()
	{
		string data = null;
		//Again, two completely different ways of doing this...
		if (this.externalEpisode) {
			string[] fileNames = Directory.GetFiles ("Episodes", "*.xml");
			data = File.ReadAllText (fileNames [this.selectedEpisode]);
		} else {
			Object[] episodes = Resources.LoadAll ("Episodes", typeof(TextAsset));
			data = ((TextAsset)episodes [this.selectedEpisode]).text;
		}
		return data;
	}

    #endregion

    #region Dialogue display...
	/// <summary>
	/// Displays the dialogue window.
	/// </summary>
	/// <param name="windowID">The Unity window ID.</param>
	private void DrawDialogue (int windowID)
	{
		// use the spike function to add the spikes
		this.AddSpikes (this.dialogueBounds.width);

		//add a fancy top using the fancy top function
		this.FancyTop (this.dialogueBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		for (int i = 0; i < this.Dialogues.Count; i ++) {
			GUILayout.Label (this.Dialogues [i].Speaker.Name);
			GUILayout.Label ("", "Divider");
			GUILayout.Label (this.Dialogues [i].Line, "LightText");	
			if (i < this.Dialogues.Count - 1)
				GUILayout.Label ("", "Divider");
		}        
		GUILayout.FlexibleSpace ();
		bool done = GUILayout.Button ("Continue");
		
		this.DisplayDialogue = !done;
		if (done) {
			//Reset window and mark dialogue as spoken.        
			foreach (Dialogue d in this.Dialogues) {
				d.Spoken = true;
			}
			this.Dialogues = new List<Dialogue>();
		}

		GUILayout.EndVertical ();

		// add a wax seal at the bottom of the window
		//this.WaxSeal(this.dialogueBounds.width, this.dialogueBounds.height);
	}
    #endregion

    #region Inventory display...
	/// <summary>
	/// Displays the inventory window.
	/// </summary>
	/// <param name="windowID">The Unity window ID.</param>
	private void DrawInventory (int windowID)
	{
		// use the spike function to add the spikes
		this.AddSpikes (this.inventoryBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label ("Inventory");

		//Check to see if the party has any items.
		if (this.WorldScript.PartyCharacter.Inventory.Count == 0) {
			//Quick style swap.
			FontStyle style = this.Skin.GetStyle ("LightText").fontStyle;
			this.Skin.GetStyle ("LightText").fontStyle = FontStyle.Italic;
			GUILayout.Label ("You do not posses any items.", "LightText");
			//Replace the style.
			this.Skin.GetStyle ("LightText").fontStyle = style;
		}

		//Some very selective style swapping in order to render a better scroll bar.
		GUI.skin = this.defaultSkin;
		this.invScroll = GUILayout.BeginScrollView (this.invScroll, false, false);
		GUI.skin = this.Skin;

		//Draw each item to the window.
		foreach (Item item in this.WorldScript.PartyCharacter.Inventory) {
			GUILayout.Label (item.Name, "LegendaryText");
			GUILayout.Label (item.Description, "LightText");
			GUILayout.Space (12);
		}
		//A little more very selective swapping.
		GUI.skin = this.defaultSkin;
		GUILayout.EndScrollView ();
		GUI.skin = this.Skin;


		GUILayout.EndVertical ();
	}
    #endregion

    #region Story display...
	/// <summary>
	/// Displays the story window.
	/// </summary>
	/// <param name="windowID">The Unity window ID.</param>
	private void DrawStory (int windowID)
	{
		DramaManager dm = Globals.Instance.DMScript.DramaManager;

		// use the spike function to add the spikes
		this.AddSpikes (this.storyBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label ("Episode Script");

		//Check to see if the drama manager is currently running.
		if (dm.EpisodeFinished) {
			//Quick style swap.
			FontStyle style = this.Skin.GetStyle ("LightText").fontStyle;
			this.Skin.GetStyle ("LightText").fontStyle = FontStyle.Italic;
			GUILayout.Label ("No episode loaded.", "LightText");
			//Replace the style.
			this.Skin.GetStyle ("LightText").fontStyle = style;

			//Return.
			GUILayout.EndVertical ();
			return;
		}

		//Some very selective style swapping in order to render a better scroll bar.
		GUI.skin = this.defaultSkin;
		this.storyScroll = GUILayout.BeginScrollView (this.storyScroll, false, false);
		GUI.skin = this.Skin;

		//Draw each task in the window.
		foreach (Task task in dm.Script) {
			string s = task.Actor.Name + ":  " + task.Description;
			//Indicate the current task with blue text.
			if (task == dm.CurrentTask) {
				GUILayout.Label (s, "LegendaryText");
			} else {
				GUILayout.Label (s, "LightText");
			}
			GUILayout.Space (12);
		}

		//A little more very selective swapping.
		GUI.skin = this.defaultSkin;
		GUILayout.EndScrollView ();
		GUI.skin = this.Skin;

		GUILayout.EndVertical ();
	}

    #endregion

    #region Locales display...
	/// <summary>
	/// Displays the locale fast-travel window.
	/// </summary>
	/// <param name="windowID">The Unity window ID.</param>
	private void DrawLocales (int windowID)
	{
		//Collect locale names.
		string[] locales = new string[this.WorldScript.Locales.Count];
		for (int i = 0; i < locales.Length; i++) {
			locales [i] = this.WorldScript.Locales [i].name;
		}
        
		this.AddSpikes (this.localeBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label ("Your party sets out for...");

		//Some very selective style swapping in order to render a better scroll bar and buttons.
		GUI.skin = this.defaultSkin;
		this.localeScroll = GUILayout.BeginScrollView (this.localeScroll, false, false);
		this.selectedLocale = GUILayout.SelectionGrid (this.selectedLocale, locales, 3);
		GUILayout.EndScrollView ();
		GUI.skin = this.Skin;

		GUILayout.EndVertical ();

		//Check for button press.
		if (this.selectedLocale != -1) {
			this.WorldScript.Teleport (this.WorldScript.PartyCharacter, this.WorldScript.Locales [this.selectedLocale]);
			//Do this last, as it overwrites the selected index.
			this.DisplayLocales = false;
		}
	}
    #endregion

	/// <summary>
	/// Thanks, I guess?
	/// </summary>
	private void DrawThanks (int windowID)
	{
		this.AddSpikes (this.thanksBounds.width);
		this.FancyTop (this.thanksBounds.width);

		GUILayout.BeginVertical ();
		GUILayout.Label ("Thanks!");
		GUILayout.Label ("You may terminate the program now.", "LightText");
		GUILayout.EndVertical ();

	}
	
	private void drawActions (int windowID)
	{
		//Collect locale names.
		string[] actions = new string[11]{"Talk", "Pick Up Item", "Trade Item", "Give Item", "Take Item", "Fight", "Steal Item", "Loot Item", "Kill With Item", "Revive With Item", "Cancel"};
        
		this.AddSpikes (this.actionBounds.width);
		this.FancyTop (this.actionBounds.width);
		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label ("Select an action");
		this.selectedAction = GUILayout.SelectionGrid (this.selectedAction, actions, 3);
		//Some very selective style swapping in order to render a better scroll bar and buttons.
		GUI.skin = this.defaultSkin;
		GUI.skin = this.Skin;

		GUILayout.EndVertical ();        
		if (this.selectedAction != -1) {
			print ("Selected Action " + this.selectedAction);
			//this.displayActionOptions = false;
		}
	}
	
	/// <summary>
	/// Draws the actor inventory.
	/// </summary>
	/// <param name='windowID'>
	/// Window I.
	/// </param>
	private void drawActorInventory (int windowID)
	{
		// use the spike function to add the spikes
		this.AddSpikes (this.actorInventoryBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label (this.Actor.Name + "'s Inventory");

		//Check to see if the party has any items.
		if (this.Actor.Inventory.Count == 0) {
			//Quick style swap.
			FontStyle style = this.Skin.GetStyle ("LightText").fontStyle;
			this.Skin.GetStyle ("LightText").fontStyle = FontStyle.Italic;
			GUILayout.Label (this.Actor.Name + " does not posses any items.", "LightText");
			//Replace the style.
			this.Skin.GetStyle ("LightText").fontStyle = style;
		}

		//Some very selective style swapping in order to render a better scroll bar.
		GUI.skin = this.defaultSkin;
		this.invScroll = GUILayout.BeginScrollView (this.invScroll, false, false);
		GUI.skin = this.Skin;
		
		string[] ActorInventory = new string[this.Actor.Inventory.Count + 1];
		int i = 0;
		ActorInventory [i++] = "No Item Selected";
		
		//Draw each item to the window.
		foreach (Item item in this.Actor.Inventory) {
//            GUILayout.Label(item.Name, "LegendaryText");
//            GUILayout.Label(item.Description, "LightText");
//            GUILayout.Space(12);
			ActorInventory [i++] = item.Name;
		}
		
		this.selectedActorInventoryItem = GUILayout.SelectionGrid (this.selectedActorInventoryItem, ActorInventory, 2);
		
		//A little more very selective swapping.
		GUI.skin = this.defaultSkin;
		GUILayout.EndScrollView ();
		GUI.skin = this.Skin;

		GUILayout.EndVertical ();
		        
		if (this.selectedActorInventoryItem != -1) {
			//print ("Selected Actor " + this.selectedActorInventoryItem);
			this.displayActorInventory = false;
		}
	}
	
	/// <summary>
	/// Draws the actee inventory.
	/// </summary>
	/// <param name='windowID'>
	/// Window I.
	/// </param>
	private void drawActeeInventory (int windowID)
	{
		// use the spike function to add the spikes
		this.AddSpikes (this.acteeInventoryBounds.width);

		//GUILayout.Space(8);
		GUILayout.BeginVertical ();
		GUILayout.Label (this.Actee.Name + "'s Inventory");

		//Check to see if the party has any items.
		if (this.Actee.Inventory.Count == 0) {
			//Quick style swap.
			FontStyle style = this.Skin.GetStyle ("LightText").fontStyle;
			this.Skin.GetStyle ("LightText").fontStyle = FontStyle.Italic;
			GUILayout.Label (this.Actee.Name + " does not posses any items.", "LightText");
			//Replace the style.
			this.Skin.GetStyle ("LightText").fontStyle = style;
		}

		//Some very selective style swapping in order to render a better scroll bar.
		GUI.skin = this.defaultSkin;
		this.invScroll = GUILayout.BeginScrollView (this.invScroll, false, false);
		GUI.skin = this.Skin;
		
		string[] ActeeInventory = new string[this.Actee.Inventory.Count + 1];
		int i = 0;
		ActeeInventory [i++] = "No Item Selected";
		
		//Draw each item to the window.
		foreach (Item item in this.Actee.Inventory) {
//            GUILayout.Label(item.Name, "LegendaryText");
//            GUILayout.Label(item.Description, "LightText");
//            GUILayout.Space(12);
			ActeeInventory [i++] = item.Name;
		}
	
		this.selectedActeeInventoryItem = GUILayout.SelectionGrid (this.selectedActeeInventoryItem, ActeeInventory, 2);
		
		//A little more very selective swapping.
		GUI.skin = this.defaultSkin;
		GUILayout.EndScrollView ();
		GUI.skin = this.Skin;

		GUILayout.EndVertical ();
		        
		if (this.selectedActeeInventoryItem != -1) {
			//print ("Selected Actee " + this.selectedActeeInventoryItem);
			this.displayActeeInventory = false;
		}
	}

    #region Various 'flair' functions converted from the Necromancer GUITestScript.
	public void AddSpikes (float width)
	{
		int spikeCount = (int)(width - 152) / 22;
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("", "SpikeLeft");//-------------------------------- custom
		for (int i = 0; i < spikeCount; i++) {
			GUILayout.Label ("", "SpikeMid");//-------------------------------- custom
		}
		GUILayout.Label ("", "SpikeRight");//-------------------------------- custom
		GUILayout.EndHorizontal ();
	}

	public void FancyTop (float width)
	{
		float leafOffset = (width / 2) - 64;
		float frameOffset = (width / 2) - 27;
		float skullOffset = (width / 2) - 20;
		GUI.Label (new Rect (leafOffset, 18, 0, 0), "", "GoldLeaf");//-------------------------------- custom	
		GUI.Label (new Rect (frameOffset, 3, 0, 0), "", "IconFrame");//-------------------------------- custom	
		GUI.Label (new Rect (skullOffset, 12, 0, 0), "", "Skull");//-------------------------------- custom	
	}

	public void WaxSeal (float x, float y)
	{
		float WSwaxOffsetX = x - 120;
		float WSwaxOffsetY = y - 115;
		float WSribbonOffsetX = x - 114;
		float WSribbonOffsetY = y - 83;

		GUI.Label (new Rect (WSribbonOffsetX, WSribbonOffsetY, 0, 0), "", "RibbonBlue");//-------------------------------- custom	
		GUI.Label (new Rect (WSwaxOffsetX, WSwaxOffsetY, 0, 0), "", "WaxSeal");//-------------------------------- custom	
	}
    #endregion
}
