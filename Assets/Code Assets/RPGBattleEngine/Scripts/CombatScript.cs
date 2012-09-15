using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using BattleEngine;
using BattleEngine.Creatures;
using BattleEngine.Powers;
using Com;
using StateSpace;
using WorldEngine;

public class CombatScript : MonoBehaviour {

    ///// <summary>
    ///// The XML asset containing a serialized bestiary.
    ///// </summary>
    //public TextAsset BestiaryXML = null;
    ///// <summary>
    ///// The XML asset containing a serialized powerbook.
    ///// </summary>
    //public TextAsset PowerBookXML = null;
    ///// <summary>
    ///// The deserialized Bestiary.
    ///// </summary>
    //private Bestiary bestiary;
    ///// <summary>
    ///// The deserialized PowerBook.
    ///// </summary>
    //private PowerBook powerBook;

    /// <summary>
    /// The prefab for spawning new battles.
    /// </summary>
    public GameObject BattlePrefab;
    /// <summary>
    /// The currently running battle.
    /// </summary>
    private GameObject battleObject;

    /// <summary>
    /// The battle taking place in this combat.
    /// </summary>
    public Battle Battle { get; set; }
    /// <summary>
    /// Flag toggling the completion state of the battle.
    /// </summary>
    private bool battling;
    
    /// <summary>
    /// If true, the script will contact a remote AFABL server to run agent logic.
    /// </summary>
    public bool UseServerAgents = false;

    public Communicator Comms { get; set; }

	// Use this for initialization
	void Start () {
        ////These kind of have to be deserialized in this order to make any sense.
        //this.powerBook = PowerBook.Deserialize(this.PowerBookXML.text);
        //this.bestiary = Bestiary.Deserialize(this.BestiaryXML.text, this.powerBook);

        //#region Temp Battle...
        //Battle battle = new Battle();
        //battle.FriendlyTeam.Add(new Creature(this.bestiary.GetCreature("Magora")));
        //battle.HostileTeam.Add(new Creature(this.bestiary.GetCreature("Skeleton")));
        //this.Battle = battle;
        //#endregion

        this.LoadBattle();

        //Connect to the server, if needed.
        if (this.UseServerAgents) {
            //Specify location of remote host.
            IPEndPoint remote = new IPEndPoint(IPAddress.Loopback, 12345);
            this.Comms = new Communicator(null, remote);
            //Establish connection with AFABL.
            try {
                this.Comms.Connect();
            } catch (SocketException) {
                Debug.LogError("Error establishing connection with remote AFABL server, using random agent behavior.");
                this.battleObject.GetComponent<BattleScript>().AITurnDelegate = null;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        //Figure out if we're still battling.
        this.battling = (this.battleObject != null);


        if (!this.battling) {
            Application.LoadLevel("RPGWorld");
            Globals.Instance.ActivateRoot();

            //Send a dummy terminal state and close socket.
            BattleState state = new BattleState(new List<Creature>());
            state.FriendliesWon = true;
            this.Comms.Close();
        } else {
            //Snag the score each frame.  Excessive, but it can't be accessed later.
            Globals.Instance.Score = this.battleObject.GetComponent<BattleScript>().Score;
        }
	}

    /// <summary>
    /// Instantiates the next battle.
    /// </summary>
    private void LoadBattle() {
        //Create new battle gameobject.
        this.battleObject = (GameObject)Instantiate(this.BattlePrefab);
        this.battleObject.GetComponent<BattleScript>().Battle = Globals.Instance.Battle;

        BattleScript bScript = this.battleObject.GetComponent<BattleScript>();

        //Set the initial score.
        bScript.Score = Globals.Instance.Score;

        //Create new battle report and set the object to record data there.
        BattleReport report = new BattleReport();
        bScript.Report = report;
        Globals.Instance.Reports.Add(report);

        //Set the delegate method if applicable.
        if (this.UseServerAgents) bScript.AITurnDelegate = this.AFABLTurn;
    }

    /// <summary>
    /// Contacts an AFABL agent server to run character agent battle logic.
    /// </summary>
    /// <param name="turnOrder">The battle's current turn order.</param>
    /// <returns>The damage report from the ability used.</returns>
    private DamageReport AFABLTurn(List<AnimatedCreature> rawTurnOrder) {
        //Convert the turn order into a usable state.
        List<Creature> turnOrder = new List<Creature>();
        foreach (AnimatedCreature ac in rawTurnOrder) {
            turnOrder.Add(ac.Creature);
        }

        BattleState state = new BattleState(turnOrder);
        state.FriendliesWon = false;
        state.HostilesWon = false;

        //Pipe the state over.
        string data = state.Serialize() + ";";
        this.Comms.Send(data);

        //Wait for a reply.
        string response = this.Comms.RecieveBlocking();
        //Execute reply.
        return this.ExecuteResponse(response, turnOrder);
    }

    /// <summary>
    /// Executes a reponse recieved from the remote AFABL server.
    /// </summary>
    /// <param name="response">The response to execute.</param>
    private DamageReport ExecuteResponse(string response, List<Creature> turnOrder) {
        //We'll keep this protocol really simple.
        //[pName] [pIndex] [cName] [cIndex]

        Debug.Log(response);
        
        char[] delimiters = { ',', ';'};
        string[] parts = response.Split(delimiters);
        Debug.Log("Using " + parts[0] + " on " + parts[2] +".");

        Power power = turnOrder[0].Prototype.AtWillPowers[Int32.Parse(parts[1])];
        Creature target = turnOrder[Int32.Parse(parts[3])];

        return power.Use(turnOrder[0], target);
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.  Since it's
    /// called for both rendering and events, this may be called multiple times
    /// per frame.
    /// </summary>
    public void OnGUI() {
        //Yeah, you guys don't care about this at all.

        //Rect bounds = new Rect(
        //    16,
        //    16,
        //    384,
        //    24);
        //string message = "";
        //if (this.Comms.Connected) {
        //    message = "Connected to AFABL server, running remote agents.";
        //} else {
        //    message = "No local AFABL server found, running default agents.";
        //}

        //GUI.Box(bounds, message);
    }
}