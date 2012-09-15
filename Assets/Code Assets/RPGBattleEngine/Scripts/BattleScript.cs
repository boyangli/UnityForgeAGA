using UnityEngine;
using System;
using System.Collections.Generic;

using BattleEngine;
using BattleEngine.Powers;
using BattleEngine.Creatures;

/// <summary>
/// Script that controls a single battle.
/// </summary>
public class BattleScript : MonoBehaviour {
    
    #region Attributes...
    /// <summary>
    /// The battle structure containing the participating creatures.
    /// </summary>
    public Battle Battle { get; set; }
    /// <summary>
    /// The length of time before a player is assumed to have passed his turn.
    /// </summary>
    public float TurnTime = 10;
    /// <summary>
    /// The time at which the current turn started.
    /// </summary>
    private float? turnStartTime = null;
    /// <summary>
    /// The length of time the damage notifications are displayed.
    /// </summary>
    public float NotificationTime = 3;
    /// <summary>
    /// The time at which the notification display started.
    /// </summary>
    private float? notificationStartTime = null;

    /// <summary>
    /// The GUI script running with this scenario.
    /// </summary>
    public BattleGUI GUI;
    /// <summary>
    /// The background sprite for the battle.
    /// </summary>
    public GameObject BackgroundSprite;

    /// <summary>
    /// The ordered turn list of creatures.
    /// </summary>
    public List<AnimatedCreature> TurnList { get; private set; }

    public float? TurnTimeRemaining {
        get {
            return this.TurnTime + this.turnStartTime - Time.time;
        }
    }
    /// <summary>
    /// The amount of time left to display the current notification.
    /// </summary>
    public float? NotificationTimeRemaining {
        get {
            return this.NotificationTime + this.notificationStartTime - Time.time;
        }
    }
    /// <summary>
    /// The active creature in the turn list.
    /// </summary>
    public AnimatedCreature ActiveCreature {
        get {
            return this.TurnList[0];
        }
    }
    /// <summary>
    /// The overall battlereport.
    /// </summary>
    public BattleReport Report { get; set; }
    /// <summary>
    /// The most recent damage report.
    /// </summary>
    public DamageReport DamageReport {
        get {
            return this.Report.DamageReports[this.Report.DamageReports.Count - 1];
        }
    }
    public int Score { get; set; }

    /// <summary>
    /// Dictionary mapping sprite instances to creature instances.
    /// </summary>
    public List<AnimatedCreature> AnimatedCreatures { get; private set; }
    /// <summary>
    /// The empty animated sprite prefab from the OT package.
    /// </summary>
    public GameObject OTAnimatedPrefab;

    /// <summary>
    /// The delegate method for taking AI-controlled turns.
    /// </summary>
    public AITakeTurn AITurnDelegate { get; set; }
    #endregion

	// Use this for initialization
	public void Start () {
        //Create the dictionary.
        AnimatedCreature.AnimatedSpritePrefab = this.OTAnimatedPrefab;
        AnimatedCreature.InputDelegate = this.OnInput;
        this.AnimatedCreatures = new List<AnimatedCreature>();

        //Create friendly team.
        this.SpawnSingleColumn(this.Battle.FriendlyTeam, this.Origin, this.Offset, this.Scale, false);

        //Create hostile team.
        Vector3 hostileOrigin = this.Origin; hostileOrigin.x *= -1;
        Vector3 hostileOffset = this.Offset; hostileOffset.x *= -1;

        float raw = this.Battle.HostileTeam.Count / 3f;
        int columns = (int)Math.Ceiling(raw);

        //Don't shuffle before spawning, or my assumptions break.
        this.SpawnMultipleColumns(this.Battle.HostileTeam, columns, 4, hostileOrigin, hostileOffset, this.Scale, true);
        this.ShuffleTurnList();

        //Go through the list of AI creatures and mark them as such.
        foreach (Creature ai in this.Battle.HostileTeam) {
            ai.AIControlled = true;
            ai.Hostile = true;
        }

        //Instantiate the background sprite.
        GameObject bg = (GameObject)Instantiate(this.BackgroundSprite);
        bg.transform.parent = this.gameObject.transform;
	}

    // Update is called once per frame
    bool backed = false;
    void Update() {
        //Do nothing if damage report is still active.
        if (this.notificationStartTime != null) {
            if (this.NotificationTimeRemaining < 0.5f && !backed) {
                //Set animation to backwards cast..
                this.ActiveCreature.Sprite.PlayOnceBackward("Cast");
                backed = true;
            }
            if (this.NotificationTimeRemaining > 0) return;
            this.notificationStartTime = null;

            //Set animation of active creature back to ready.
            this.ActiveCreature.Sprite.PlayLoop("Rest");

            this.TurnList.Add(this.TurnList[0]);
            this.TurnList.RemoveAt(0);
            backed = false;
        }

        List<AnimatedCreature> toDestroy = new List<AnimatedCreature>();
        //Mark people as dead and play proper sprites if needed.
        foreach (AnimatedCreature ac in this.AnimatedCreatures) {
            if (ac.Creature.Health == 0 && !ac.Creature.Dead) {
                ac.Creature.Dead = true;
                toDestroy.Add(ac);
            }
        }
        foreach (AnimatedCreature ac in toDestroy) {
            this.TurnList.Remove(ac);
            this.AnimatedCreatures.Remove(ac);
            Destroy(ac.Sprite);
        }

        //Check health levels for victory condition.
        bool friendliesDead = true;
        foreach (Creature f in this.Battle.FriendlyTeam) {
            friendliesDead &= f.Dead;
        }

        bool hostilesDead = true;
        foreach (Creature h in this.Battle.HostileTeam) {
            hostilesDead &= h.Dead;
        }

        //Destroy battle if either team is dead.
        if (friendliesDead || hostilesDead) {
            Debug.Log("Battle ended - destroying self.");
            Destroy(this.gameObject);
            return;
        }

        if (this.ActiveCreature.Creature.AIControlled) {
            //Allow the character to take its turn.
            DamageReport dr;
            if (this.AITurnDelegate == null) {
                dr = this.RandomTurn(this.TurnList);
            } else {
                dr = this.AITurnDelegate(this.TurnList);
            }
            //Use a random ability on a random target.
            //Creature target = this.Battle.GetRandomTarget(this.ActiveCreature.Creature);
            //DamageReport dr = this.ActiveCreature.Creature.UseRandomPower(target);
            
            
            this.Report.DamageReports.Add(dr);
            this.notificationStartTime = Time.time;
            this.TallyScore(dr);

            //Set animation of active creature to cast.
            this.ActiveCreature.Sprite.PlayOnce("Cast");
        } else {
            //Don't want to set this in Start(), as there may be some delay caused by initialization.
            if (this.turnStartTime == null) this.turnStartTime = Time.time;

            if (this.TurnTimeRemaining <= 0) {
                //Pass the turn.
                this.turnStartTime = null;
                this.TurnList.Add(this.TurnList[0]);
                this.TurnList.RemoveAt(0);

                //In case a spell was selected, but not yet cast.
                this.GUI.SelectedIndex = -1;
            }
        }
    }

    #region Initialization and spawning
    //In an actual implementation, these would be exposed to the editor.
    //I don't want to muck up the script with a ton of variables in this case.
    private Vector3 Origin = new Vector3(11, 6, 0);
    private Vector3 Offset = Vector3.Normalize(new Vector3(-3, 3, 0));
    private float Scale = 3;
    /// <summary>
    /// Spawns a team in a single column formation.
    /// </summary>
    /// <param name="creatures">The team to spawn.</param>
    /// <param name="origin">The origin of the formation.</param>
    /// <param name="offset">The unit vector determining the direction of the column.</param>
    /// <param name="scale">The scale of the column.  Column length is 2 * scale.</param>
    /// <param name="flipped">Whether or not this team should be horizontally mirrored.</param>
    private void SpawnSingleColumn(List<Creature> creatures, Vector3 origin, Vector3 offset, float scale, bool flipped) {
        //Special case for a single creature.
        if (creatures.Count > 1) {
            //Calculate delta stepping.
            Vector3 delta = 2 * offset / (creatures.Count - 1);
            //Create team.
            for (int i = 0; i < creatures.Count; i++) {
                AnimatedCreature ac = new AnimatedCreature(creatures[i], this.gameObject);
                ac.Position = origin + scale * (offset - delta * i);
                ac.FlipHorizontal = flipped;

                this.AnimatedCreatures.Add(ac);
            }
        } else {
            //Create single creature at origin.
            AnimatedCreature ac = new AnimatedCreature(creatures[0], this.gameObject);
            ac.Position = origin;
            ac.FlipHorizontal = flipped;

            this.AnimatedCreatures.Add(ac);
        }
    }

    /// <summary>
    /// Spawns a team in a stacked formation of multiple columns.
    /// </summary>
    /// <param name="creatures">The team to spawn.</param>
    /// <param name="columns">The number of columns.</param>
    /// <param name="spacing">The horizontal space between columns.</param>
    /// <param name="origin">The origin of the formation.</param>
    /// <param name="offset">The unit vector determining the direction of the columns.</param>
    /// <param name="scale">The scale of the columns.  Column length is 2 * scale.</param>
    /// /// <param name="flipped">Whether or not this team should be horizontally mirrored.</param>
    private void SpawnMultipleColumns(List<Creature> creatures, int columns, float spacing, Vector3 origin, Vector3 offset, float scale, bool flipped) {
        //A little idiot-proofing.
        if (columns < 2) {
            this.SpawnSingleColumn(creatures, origin, offset, scale, flipped);
            return;
        }

        //Split team in subteams.
        List<Creature>[] subteams = new List<Creature>[columns];
        int teamSize = (int)Math.Ceiling((float)creatures.Count / columns);
        int i = 0;
        for (i = 0; i < subteams.Length - 1; i++) {
            subteams[i] = creatures.GetRange(i * teamSize, teamSize);
        }
        //Catch the last, (likely) smaller column.
        subteams[i] = creatures.GetRange(i * teamSize, creatures.Count - i * teamSize);

        //Calculate spacing offset.
        float spacingOffset = (columns - 1) * spacing / 2;
        Vector3 columnOffset = Vector3.zero;
        //Spawn columns.
        int j = 0;
        for (j = 0; j < subteams.Length - 1; j++) {
            columnOffset.x = spacingOffset - spacing * j;
            this.SpawnSingleColumn(subteams[j], origin + columnOffset, offset, scale, flipped);
        }
        //Make the last column a bit smaller - based on how full it is.
        columnOffset.x = spacingOffset - spacing * j;
        this.SpawnSingleColumn(subteams[j], origin + columnOffset, offset, scale * subteams[j].Count / subteams[0].Count, flipped);
    }

    /// <summary>
    /// Shuffles the starting turn order. (Fisher–Yates Shuffle)
    /// </summary>
    private void ShuffleTurnList() {
        //Obviously, we'll need this.
        System.Random rng = new System.Random();

        //Throw all the creatures into a temmporary turn list.
        List<AnimatedCreature> list = new List<AnimatedCreature>();
        list.AddRange(this.AnimatedCreatures);

        //Shuffle!
        int n = list.Count - 1;
        while (n > 1) {
            int k = rng.Next(n + 1);
            AnimatedCreature ac = list[k];
            list[k] = list[n];
            list[n] = ac;
            n--;
        }

        //Convert the list to the desired queue.
        this.TurnList = new List<AnimatedCreature>(list);
    }
    #endregion

    /// <summary>
    /// Input delegate for Orthello sprites.
    /// </summary>
    /// <param name="owner">The owning object that the input acted on.</param>
    public void OnInput(OTObject owner) {
        //Checking if the input occured on a left click and a spell is active.
        if (Input.GetMouseButtonDown(0) && this.GUI.SelectedIndex != -1) {
            //Fetch the creature and power.
            Power power = this.GUI.SelectedPower;

            //Find the target creature.
            Creature target = null;
            foreach (AnimatedCreature ac in this.AnimatedCreatures) {
                if (ac.Sprite.gameObject == owner.gameObject) {
                    target = ac.Creature;
                    break;
                }
            }

            this.turnStartTime = null;

            DamageReport dr = power.Use(this.ActiveCreature.Creature, target);
            this.Report.DamageReports.Add(dr);
            this.notificationStartTime = Time.time;
            this.TallyScore(dr);

            this.GUI.SelectedIndex = -1;

            //Set animation of active creature to cast.
            this.ActiveCreature.Sprite.PlayOnce("Cast");
        }
    }

    /// <summary>
    /// Alters the current score based on a damage report.
    /// </summary>
    /// <param name="dr">The damage report.</param>
    private void TallyScore(DamageReport dr) {
        //Determine whether the player is attacking or being attacked.
        if (!dr.Attacker.AIControlled && dr.Defender.AIControlled) {
            //PC attacked NPC, tally score based on multiplier.
            if (dr.Multiplier == 1) this.Score += 2;
            if (dr.Multiplier > 1) this.Score += 5;
        } else if (dr.Attacker.AIControlled && !dr.Defender.AIControlled) {
            //NPC attacked PC, decrement score.
            this.Score--;
        }
    }

    /// <summary>
    /// Delegate method that takes an AI's turn.
    /// </summary>
    /// <param name="turnOrder">The current turn order for the battle.</param>
    /// <returns>The damage report from the turn.</returns>
    public delegate DamageReport AITakeTurn(List<AnimatedCreature> turnOrder);

    public DamageReport RandomTurn(List<AnimatedCreature> turnOrder) {
        //Use a random ability on a random target.
        Creature target = this.Battle.GetRandomTarget(this.ActiveCreature.Creature);
        DamageReport dr = this.ActiveCreature.Creature.UseRandomPower(target);

        return dr;
    }
}