using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

using BattleEngine;
using BattleEngine.Powers;
using BattleEngine.Creatures;
using DataOutput;

/// <summary>
/// Script for executing a series of battles between creatures where one side
/// is controlled by a player, and the other side is AI controlled.
/// </summary>
public class ScenarioScript : MonoBehaviour {

    #region Attributes
    /// <summary>
    /// The XML asset containing a serialized scenario.
    /// </summary>
    public TextAsset ScenarioXML = null;
    /// <summary>
    /// The XML asset containing a serialized bestiary.
    /// </summary>
    public TextAsset BestiaryXML = null;
    /// <summary>
    /// The XML asset containing a serialized powerbook.
    /// </summary>
    public TextAsset PowerBookXML = null;
    /// <summary>
    /// The deserialized Scenario.
    /// </summary>
    private Scenario scenario;
    /// <summary>
    /// The deserialized Bestiary.
    /// </summary>
    private Bestiary bestiary;
    /// <summary>
    /// The deserialized PowerBook.
    /// </summary>
    private PowerBook powerBook;
    /// <summary>
    /// The ID number for the player of this scenario.
    /// </summary>
    private int pID = -1;

    /// <summary>
    /// The prefab for spawning new battles.
    /// </summary>
    public GameObject BattlePrefab;
    /// <summary>
    /// The currently running battle.
    /// </summary>
    private GameObject battleObject;

    /// <summary>
    /// Flag for toggling the initial screen, where the playerID is input.
    /// </summary>
    private bool initialize = true;
    /// <summary>
    /// Flag toggling the state of the script, battling or questioning.
    /// </summary>
    private bool battling = true;
    /// <summary>
    /// Flag toggling the final GUI screen and data writing.
    /// </summary>
    private bool finalize = false;

    /// <summary>
    /// The collected data from all battles.
    /// </summary>
    private List<BattleReport> battleReports;
    /// <summary>
    /// The collected data from all surveys.
    /// </summary>
    private List<SurveyReport> surveyReports;
    /// <summary>
    /// Private struct for storing answers to survey questions.  I hate making
    /// private classes, but fuck it.
    /// </summary>
    private class SurveyReport {
        public string Q1 = "";
        public string Q2 = "";
        /// <summary>
        /// Creates a report with the given response values.
        /// </summary>
        /// <param name="q1">The response to question 1.</param>
        /// <param name="q2">The response to question 2.</param>
        public SurveyReport(string q1, string q2) { this.Q1 = q1; this.Q2 = q2; }
    }

    /// <summary>
    /// The overall score of the player.
    /// </summary>
    private int score = 0;
    #endregion

    // Use this for initialization
    public void Start() {
        //These kind of have to be deserialized in this order to make any sense.
        this.powerBook = PowerBook.Deserialize(this.PowerBookXML.text);
        this.bestiary = Bestiary.Deserialize(this.BestiaryXML.text, this.powerBook);
        this.scenario = Scenario.Deserialize(this.ScenarioXML.text, this.bestiary);

        #region Temp Bestiary...
        //Bestiary bst = new Bestiary();
        //bst.Creatures.Add(new Creature("Player Character Alpha"));
        //bst.Creatures.Add(new Creature("Player Character Bravo"));

        //string[] p1 = new string[] { "Acid Arrow", "Tectonic Shift" };
        //string[] p2 = new string[] { "Flame Burst", "Force Orb" };
        //bst.Creatures[0].AtWillPowerNames = new List<string>(p1);
        //bst.Creatures[1].AtWillPowerNames = new List<string>(p2);

        //bst.Creatures.Add(new Creature("Monster Yankee"));
        //bst.Creatures.Add(new Creature("Monster Zulu"));

        //string[] m = new string[] { "Strike" };
        //bst.Creatures[2].AtWillPowerNames = new List<string>(m);
        //bst.Creatures[3].AtWillPowerNames = new List<string>(m);

        //string bXML = bst.Serialize();
        //StreamWriter writer = new StreamWriter("ThisIsWhereOutputGoes.txt");
        //writer.Write("Nothing to see here.");
        //writer.Close();
        #endregion

        #region Temp Scenario...
        //Scenario scen = new Scenario();

        //Battle a = new Battle();
        //a.FriendlyNames.Add("Player Character Alpha");
        //a.FriendlyNames.Add("Player Character Bravo");
        //a.HostileNames.Add("Monster Zulu");

        //Battle b = new Battle();
        //b.FriendlyNames.Add("Player Character Alpha");
        //b.HostileNames.Add("Monster Yankee");
        //b.HostileNames.Add("Monster Zulu");

        //scen.Battles.Add(a);
        //scen.Battles.Add(b);

        //string sXML = scen.Serialize();
        //StreamWriter writer = new StreamWriter("Assets/Data/scenario.xml");
        //writer.Write(sXML);
        //writer.Close();
        #endregion

        //Make sure there's at least one battle, creature, and power.
        if (this.scenario.Battles.Count == 0 || this.bestiary.Prototypes.Count == 0 || this.powerBook.Powers.Count == 0) {
            Debug.LogError("Data files do not contain sufficient information to play a scenario.");
            Application.Quit();
        }

        this.battleReports = new List<BattleReport>();
        this.surveyReports = new List<SurveyReport>();
    }

    #region Updating and GUI...
    // Update is called once per frame
    public void Update() {
        //Figure out if we're still battling.
        this.battling = (this.battleObject != null || this.finalize || this.initialize);

        //Snag the current score value, if we're battling.
        if (this.battling && !this.finalize && ! this.initialize) {
            this.score = this.battleObject.GetComponent<BattleScript>().Score;
        }  
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.  Since it's
    /// called for both rendering and events, this may be called multiple times
    /// per frame.
    /// </summary>
    public void OnGUI() {
        //If the scenario just started, run initial setup.
        if (this.initialize) {
            this.DrawInitialSetup();
        }

        //Only draw the GUI if we're currently not battling.
        if (!this.battling) {
            this.DrawQuestionairre();
            //If the button was pushed this frame, load the next battle.
            if (this.battling) {
                if (this.scenario.Battles.Count > 0) {
                    this.LoadNextBattle();
                } else {
                    //Flag finalize and write data here, so it will only happen once.
                    this.finalize = true;
                    this.WriteData();
                }
            }
        }
        //If we're finished with the scenario, draw thanks and output data.
        if (this.finalize) {
            this.DrawThanks();
        }
    }

    /// <summary>
    /// Get the player ID number from input.
    /// </summary>
    public void DrawInitialSetup() {
        int width = 512;
        int height = 256;

        Rect bounds = new Rect(
            Screen.width / 2 - width / 2,
            Screen.height / 2 - height / 2,
            width,
            height);
        GUILayout.BeginArea(bounds);

        string message = "Please enter your ID number.";
        if (inputError) message += "\nInput must be a non-negative integer.";

        GUILayout.Box(message);
        this.input = GUILayout.TextField(this.input);

        this.initialize = !GUILayout.Button("Continue");

        GUILayout.EndArea();

        //Continue was pressed, check values and begin the scenario.
        if (!this.initialize) {
            try {
                this.pID = int.Parse(this.input);
                this.LoadNextBattle();
            } catch (Exception) {
                //Don't really care which of the three exceptions it is, they're all bad.
                this.initialize = true;
                this.inputError = true;
            }
        }
    }
    private string input = "";
    private bool inputError = false;

    /// <summary>
    /// Stubbing this out here, so the monstrosity doesn't wind up in OnGUI().
    /// </summary>
    private void DrawQuestionairre() {
        int width = 512;
        int height = 256;

        Rect bounds = new Rect(
            Screen.width / 2 - width / 2,
            Screen.height / 2 - height / 2,
            width,
            height);
        GUILayout.BeginArea(bounds);

        string[] optionsQ1 = new string[] {"1 very easy", "2", "3", "4", "5 very hard"};
		string[] optionsQ2 = new string[] {"1 little", "2", "3", "4", "5 very much"};

        GUILayout.Box("How difficult was this battle?\n(1 very easy, 5 very hard)");
        this.selectedQ1 = GUILayout.SelectionGrid(this.selectedQ1, optionsQ1, 5, "toggle");
		
		GUILayout.Box("How much did you enjoy this battle?\n(1 little, 5 very much)");
		this.selectedQ2 = GUILayout.SelectionGrid(this.selectedQ2, optionsQ2, 5, "toggle");
        
        this.battling = GUILayout.Button("Continue");
		
        GUILayout.EndArea();
        
        //Make sure both questions have been answered before continuing.
        if (this.battling && (this.selectedQ1 == -1 || this.selectedQ2 == -1)) {
            //this.battling = false;
        }
        
        //Record responses and reset selections.
		if (this.battling) {
            this.surveyReports.Add(new SurveyReport(optionsQ1[this.selectedQ1], optionsQ2[this.selectedQ2]));
   			this.selectedQ1 = -1;
			this.selectedQ2 = -1;
		}
    }
    private int selectedQ1 = -1;
	private int selectedQ2 = -1;

    /// <summary>
    /// Thanks, I guess?
    /// </summary>
    private void DrawThanks() {
        int width = 256;
        int height = 128;

        Rect bounds = new Rect(
            Screen.width / 2 - width / 2,
            Screen.height / 2 - height / 2,
            width,
            height);

        TextAnchor anchor = GUI.skin.box.alignment;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;

        GUI.Box(bounds, "Thanks for particpating!\nYou can close the game now.");

        GUI.skin.box.alignment = anchor;
    }
    #endregion

    /// <summary>
    /// Instantiates the next battle.
    /// </summary>
    private void LoadNextBattle() {
		
        //Create new battle gameobject.
        this.battleObject = (GameObject)Instantiate(this.BattlePrefab);
        this.battleObject.GetComponent<BattleScript>().Battle = this.scenario.Battles[0];

        //Create new battle report and set the object to record data there.
        BattleReport report = new BattleReport();
        this.battleObject.GetComponent<BattleScript>().Report = report;

        //Set the initial score.
        this.battleObject.GetComponent<BattleScript>().Score = this.score;

        //Update lists.
        this.scenario.Battles.RemoveAt(0);
        this.battleReports.Add(report);
    }

    /// <summary>
    /// Converts the collected BattleReports into the data format specified by Alex.
    /// </summary>
    private void WriteData() {
        //Create new output object.
        OutputData data = new OutputData();

        //Iterate over all battle reports, creating several variables vis post-processing.
        int turn = 0;
        for (int b = 0; b < this.battleReports.Count; b++) {
            List<DamageReport> damageReports = this.battleReports[b].DamageReports;
            //Iterate over all damage reports.
            for (int d = 0; d < damageReports.Count; d++) {
                DamageReport dr = damageReports[d];
                //Only output player actions.
                if (dr.Attacker.AIControlled) continue;

                OutputAction action = new OutputAction(this.pID, turn, b, dr.Power.Name, dr.Power.Element.ToString(),
                                                       dr.Multiplier, dr.Defender.Name, dr.Defender.Prototype.Element.ToString(), -1);
                data.actions.Add(action);

                //Increment counters.
                turn++;
            }
            //Output survey responses.
            OutputSurvey survey = new OutputSurvey(this.pID, b, this.surveyReports[b].Q1, this.surveyReports[b].Q2);
            data.responses.Add(survey);
        }

        data.myToCSV("ActionData", "SurveyData");
    }
}