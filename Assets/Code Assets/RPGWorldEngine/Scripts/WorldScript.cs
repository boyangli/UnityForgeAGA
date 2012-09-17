using System;
using System.Collections.Generic;

using UnityEngine;
using RAIN.Core;

using BattleEngine;
using BattleEngine.Creatures;
using BattleEngine.Powers;
using StoryEngine;
using StoryEngine.Trace;
using WorldEngine;
using WorldEngine.Items;

public class WorldScript : MonoBehaviour {

    #region Attributes
    /// <summary>
    /// The XML asset containing a serialized bestiary.
    /// </summary>
    public TextAsset BestiaryXML = null;
    /// <summary>
    /// The XML asset containing a serialized catalog.
    /// </summary>
    public TextAsset CatalogXML = null;
    /// <summary>
    /// The XML asset containing a serialized powerbook.
    /// </summary>
    public TextAsset PowerBookXML = null;
    /// <summary>
    /// The prefabricated object to use for spawning Items.
    /// </summary>
    public GameObject ItemPrefab = null;

    /// <summary>
    /// The collection of Locales in the world.
    /// </summary>
    public List<LocaleScript> Locales { get; set; }
    /// <summary>
    /// The character representing the player's party.
    /// </summary>
    public CharacterScript PartyCharacter;
    /// <summary>
    /// The list of all non-player characters in the world.
    /// </summary>
    public List<CharacterScript> NPCs { get; private set; }
    /// <summary>
    /// The list of all intantiated items in the world.
    /// </summary>
    public List<Item> Items { get; private set; }
    /// <summary>
    /// The list of unowned items lying about the world.
    /// </summary>
    public List<ItemScript> ItemScripts { get; private set; }
    
    /// <summary>
    /// The creatures comprising the player's party.
    /// </summary>
    public List<CreaturePrototype> PartyMembers { get; private set; }
    #endregion

    // Use this for initialization
	void Start () {
        #region Temp catalog...
        //Item a = new Item("Sword", "A dulled iron sword.");
        //Item b = new Item("Shield", "A battered wooden shield.");
        //Item c = new Item("Robe", "Linen robes of moderate quality.");
        //Item d = new Item("Potion", "A small vial filled with a dimly glowing, blue liquid.");

        //Catalog inv = new Catalog();
        //inv.Items.Add(a);
        //inv.Items.Add(b);
        //inv.Items.Add(c);
        //inv.Items.Add(d);

        //string invXML = inv.Serialize();
        //StreamWriter writer = new StreamWriter("inventory.xml");
        //writer.Write(invXML);
        //writer.Close();
        #endregion

        //Destroy ourselves immediately if we already exist in the shared space.
        if (Globals.Instance.WorldRoot != null) {
            UnityEngine.Object.DestroyImmediate(this.gameObject);
            return;
        }

        //Set this object to be persistent.
        UnityEngine.Object.DontDestroyOnLoad(this.gameObject);

        //Deserialize domain specifics data and create the DramaManager.
        Globals.Instance.WorldRoot = this.gameObject;
        Globals.Instance.Catalog = Catalog.Deserialize(this.CatalogXML.text);
        Globals.Instance.PowerBook = PowerBook.Deserialize(this.PowerBookXML.text);
        Globals.Instance.Bestiary = Bestiary.Deserialize(this.BestiaryXML.text, Globals.Instance.PowerBook);
        
        //Initialize collections.
        this.Locales = new List<LocaleScript>(this.GetComponentsInChildren<LocaleScript>());
        this.NPCs = new List<CharacterScript>(this.GetComponentsInChildren<CharacterScript>());

        this.InitializeItems();
        
        this.PartyMembers = new List<CreaturePrototype>();
        //Create default party members.
        this.PartyMembers.Add(Globals.Instance.Bestiary.GetCreature("Aluatra"));
        this.PartyMembers.Add(Globals.Instance.Bestiary.GetCreature("Oloril"));
        this.PartyMembers.Add(Globals.Instance.Bestiary.GetCreature("Magora"));
        this.PartyMembers.Add(Globals.Instance.Bestiary.GetCreature("Walmorn"));
    }

    /// <summary>
    /// Initializes the items within the world and their scripts.
    /// </summary>
    private void InitializeItems() {
        //We're going to use a world with 1 copy of each item in the catalog.
        this.Items = Globals.Instance.Catalog.Items;
        this.ItemScripts = new List<ItemScript>();

        //Create an itemscript object for each item.
        Vector3 position = new Vector3(0, 0, 8);
        foreach (Item item in this.Items) {
            GameObject go = (GameObject)GameObject.Instantiate(this.ItemPrefab, position, Quaternion.identity);
            go.transform.parent = this.gameObject.transform;
            go.name = item.Name;
			
            //Assign the Item to the ItemScript and register the owner and script.
            ItemScript iScript = go.GetComponent<ItemScript>();
            iScript.Item = item;
            item.Owner = go;
            this.ItemScripts.Add(iScript);

            position.x += 2;
        }
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            LocaleScript ls = this.PartyCharacter.Locale;
            Debug.Log(ls);
        }
    }

    /// <summary>
    /// Gets an instantiated character within the world by its name.
    /// </summary>
    /// <param name="name">The character name.</param>
    /// <returns>The characted with a name matching the input parameter.</returns>
    public CharacterScript GetCharacterByName(string name) {
        //Bounce back null.
        if (name == null) return null;

        //Special case shortcut for grabbing the player's party.
        if (name == "player") return this.PartyCharacter;
        foreach (CharacterScript npc in this.NPCs) {
            if (String.Compare(name, npc.Name, true) == 0) return npc;
        }

        //This character doesn't exist. =[
        return null;
    }
    /// <summary>
    /// Gets the first instantiated item within the world with a given name.
    /// </summary>
    /// <param name="name">The item name.</param>
    /// <returns>The first item found with a name matching the input parameter.</returns>
    public Item GetItemByName(string name) {
        //Bounce back null.
        if (name == null) return null;
        
        foreach (Item item in this.Items) {
            if (String.Compare(name, item.Name, true) == 0) return item;
        }

        //This item doesn't exist. =[
        return null;
    }
    /// <summary>
    /// GEts the first instantiated locale within the world with a given name.
    /// </summary>
    /// <param name="name">The locale name.</param>
    /// <returns>The first locale found with a name matching the input parameter.</returns>
    public LocaleScript GetLocaleByName(string name) {
        //Bounce back null.
        if (name == null) return null;

        foreach (LocaleScript ls in this.Locales) {
            if (String.Compare(name, ls.name, true) == 0) return ls;
        }

        //This locale doesn't exist. =[
        return null;
    }
    
    /// <summary>
    /// Teleports a chracter to a given locale.
    /// </summary>
    /// <param name="cs">The character to move.</param>
    /// <param name="locale">The locale destination.</param>
    public void Teleport(CharacterScript cs, LocaleScript locale) {
        Agent agent = cs.GetComponent<RAINAgent>().Agent;
        agent.Kinematic.Position = locale.transform.position;

        cs.Stop();
    }

    /// <summary>
    /// Teleports an Item to a given locale.  There should be some polymorphism
    /// going on here, but it's a bit late to refactor that.
    /// </summary>
    /// <param name="iScript">The item script to move.</param>
    /// <param name="locale">The locale destination.</param>
    public void Teleport(ItemScript iScript, LocaleScript locale) {
        //Yeah, it's that simple - yay for property get / set.
        iScript.Location = locale.Location;
    }

    /// <summary>
    /// Assigns an Item currently in the world to a CharacterScript.
    /// </summary>
    /// <param name="cs">The character to pickup the item.</param>
    /// <param name="item">The item to pickup.</param>
    /// <returns>True, if the item was picked up.</returns>
    public bool PickupItem(CharacterScript cs, Item item) {
        //Verify that the item is not held by a character.
        ItemScript iScript = item.Owner.GetComponent<ItemScript>();
        if (iScript == null) return false;

        //Deactivate the ItemScript, add to inventory, change owner.
        iScript.gameObject.active = false;
        cs.Inventory.Add(item);
        item.Owner = cs.gameObject;
        return true;
    }
    /// <summary>
    /// Assigns an Item currently in the world to a CharacterScript.
    /// </summary>
    /// <param name="cs">The character to pickup the item.</param>
    /// <param name="itemName">The name of the item to pickup.</param>
    /// <returns>True, if the item was picked up.</returns>
    public bool PickupItem(CharacterScript cs, string itemName) {
        Item item = this.GetItemByName(itemName);
        if (item == null) return false;
        return this.PickupItem(cs, item);
    }

    /// <summary>
    /// Drops an item from a character's inventory and places it in the world.
    /// </summary>
    /// <param name="item">The item to drop.</param>
    /// <returns>True, if the item was dropped.</returns>
    public bool DropItem(Item item) {
        //Verify that this item is actually held by a character.
        CharacterScript cs = item.Owner.GetComponent<CharacterScript>();
        if (cs == null) return false;
        
        //Remove from inventory.
        cs.Inventory.Remove(item);
        
        //Find this item's script.
        ItemScript iScript = null;
        foreach (ItemScript script in this.ItemScripts) {
            if (script.Item == item) {
                iScript = script;
                break;
            }
        }
		
        //Activte script and reassign item owner.
        iScript.gameObject.active = true;
        iScript.transform.position = cs.transform.position;
        item.Owner = iScript.gameObject;

        return true;
    }

    /// <summary>
    /// Loads the combat engine into the scene using information from a Task.
    /// </summary>
    public void LoadBattle(Task task) {
        //On the offchance we were handed a bad task.
        if (task.Type != "enter-combat") return;

        //Set up the battle with at hand information.
        Battle battle = new Battle();
        battle.AddFriendlyPrototypes(this.PartyMembers);

        //Collect the information from the task.
        List<string> enemyNames = new List<string>();
//        string enemy = null;

//        //There's a 'hard' coded set of 4 enemies in stuff handed over by the ScenGen.
//        enemy = task.StoryEvent.GetValue("enemy1");
//        enemyNames.Add(this.CapCreatureName(enemy));
//        enemy = task.StoryEvent.GetValue("enemy2");
//        enemyNames.Add(this.CapCreatureName(enemy));
//        enemy = task.StoryEvent.GetValue("enemy3");
//        enemyNames.Add(this.CapCreatureName(enemy));
        //Quick name swap to put the actee in combat.
        switch (task.Actee.Name) {
            case "Yeti":
                enemyNames.Add("Snowier");
                break;
            case "Witchdoctor":
                enemyNames.Add("Wind Ghost");
                break;
            default:
                enemyNames.Add("Aliot");
                break;
        }


        foreach (string name in enemyNames) {
            Debug.Log("Attemping to look up " + name);
            CreaturePrototype cp = Globals.Instance.Bestiary.GetCreature(name);
            Creature c = new Creature(cp);

            battle.HostileTeam.Add(c);
            //battle.HostileTeam.Add(new Creature(Globals.Instance.Bestiary.GetCreature(name)));
        }

        //Set the battle to be fought, deactivate this object, load combat.
        Globals.Instance.Battle = battle;
        this.ActivateAll(false);

        Application.LoadLevel("RPGBattle");
    }

    /// <summary>
    /// Fixed a creature name, since planners are lame and keep everything lower
    /// case.  Bestiary, creature, and display components are all properly case
    /// sensitive.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private string CapCreatureName(string name) {
        //Because some of the creatures have double names...
        string[] parts = name.Split();
        string capped = "";
        //Capitalize the first letter for each word.
        for (int i = 0; i < parts.Length; i++) {
            string end = parts[i].Remove(0, 1);
            parts[i] = char.ToUpper(parts[i][0]) + end;
        }
        capped = String.Join(" ", parts);


        //string end = parts[0].Remove(0, 1);
        //capped += char.ToUpper(parts[0][0]) + end;

        //for (int i = 1; i < parts.Length; i++)  {
        //    end = parts[i].Remove(0, 1);
        //    capped += " " + char.ToUpper(parts[i][0]) + end;
        //}

        return capped;
    }

    #region Game object activation and deactivation
    /// <summary>
    /// Recursively sets activation status for this object and all children.
    /// </summary>
    /// <param name="active">The value to assign to GameObject.active</param>
    public void ActivateAll(bool active) {
        this.gameObject.active = active;
        //Set all children active.
        for (int i = 0; i < this.transform.childCount; i++) {
            this.RecursiveActivate(active, this.transform.GetChild(i).gameObject);
        }

        //If we're reactivating, check the ItemScripts.
        if (active) {
            foreach (ItemScript iScript in this.ItemScripts) {
                iScript.gameObject.active = iScript.Item.Owner == iScript.gameObject;
            }
        }
    }
    /// <summary>
    /// Recursively sets activation status for an object and all children.
    /// </summary>
    /// <param name="active">The value to assign to GameObject.active</param>
    /// <param name="o">The current recursion object.</param>
    private void RecursiveActivate(bool active, GameObject o) {
        o.active = active;
        //Set all children active.
        for (int i = 0; i < o.transform.childCount; i++) {
            this.RecursiveActivate(active, o.transform.GetChild(i).gameObject);
        }
    }
    #endregion
}