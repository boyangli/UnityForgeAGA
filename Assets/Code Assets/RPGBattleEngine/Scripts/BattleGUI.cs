using UnityEngine;
using System;
using System.Collections;

using BattleEngine.Powers;
using BattleEngine.Creatures;

/// <summary>
/// A script solely responsible for displaying a GUI for a battle.
/// </summary>
public class BattleGUI : MonoBehaviour {

    #region Attributes
    /// <summary>
    /// The script running the logic behind the battle.
    /// </summary>
    public BattleScript BattleScript;
    /// <summary>
    /// The GUISkin to use for rendering components.
    /// </summary>
    public GUISkin Skin;
    /// <summary>
    /// The external horizontal margins for the window.  See HTML Box Model.
    /// </summary>
    public Vector2 Margin = new Vector2(38, 40);
    /// <summary>
    /// The internal padding for the window.  See HTML Box Model.
    /// </summary>
    public Vector2 Padding = new Vector2(8, 8);
    /// <summary>
    /// The height of the window.
    /// </summary>
    public int Height = 256;
    /// <summary>
    /// The background texture for the active character.
    /// </summary>
    public Texture2D ActiveCharacterTexture;

    /// <summary>
    /// The icons used to represent the elements.  The powers are indexed by their
    /// enumerated value.
    /// </summary>
    public Texture2D[] ElementIcons = new Texture2D[Enum.GetValues(typeof(Element)).Length];

    /// <summary>
    /// Runtime constructed GUIStyle for rendering the background of active
    /// characters.
    /// </summary>
    private GUIStyle activeHighlightStyle;

    private GUISkin DefaultSkin;
    /// <summary>
    /// The currently selected index..
    /// </summary>
    public int SelectedIndex { get; set; }
    /// <summary>
    /// The currently selected power.
    /// </summary>
    public Power SelectedPower {
        get {
            if (this.SelectedIndex == -1) return null;
            return this.BattleScript.ActiveCreature.Creature.Prototype.AtWillPowers[this.SelectedIndex];
        }
    }

    #endregion

    // Use this for initialization
	void Start () {
        this.activeHighlightStyle = new GUIStyle(GUIStyle.none);
        this.activeHighlightStyle.normal.background = this.ActiveCharacterTexture;
        this.activeHighlightStyle.stretchHeight = true;
        this.activeHighlightStyle.stretchWidth = true;

        this.SelectedIndex = -1;
	}
	
	// Update is called once per frame
	void Update () {

	}

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.  Since it's
    /// called for both rendering and events, this may be called multiple times
    /// per frame.
    /// </summary>
    public void OnGUI() {
        this.DefaultSkin = GUI.skin;
        GUI.skin = this.Skin;

        #region Background window and content area creation.
        Rect notificationArea = new Rect(
            Screen.width / 2 - 256,
            Screen.height - this.Height - 48,
            512,
            48);
        Rect hoverArea = new Rect(
            0,
            0,
            128,
            64);
        //Meh, turns out I'm not strictly obeying the HTML Box Model, but whatever.
        Rect wrapper = new Rect(
            0,
            Screen.height - this.Height,
            Screen.width,
            this.Height);
        int outerOffset = 30; //Account for some weird padding above windows from the skin.
        Rect outer = new Rect(
            -this.Margin.x,
            -outerOffset,
            wrapper.width + 2 * this.Margin.x,
            wrapper.height + this.Margin.y + outerOffset);
        int contentOffset = 16; //More weirdness.
        Rect content = new Rect(
            this.Padding.x,
            this.Padding.y,
            wrapper.width - 2 * this.Padding.x,
            wrapper.height - this.Padding.y - contentOffset);
        #endregion

        //Begin wrapper group and draw border.
        GUI.BeginGroup(wrapper);
        GUI.Box(outer, "", this.Skin.window);

        //Begin content group.
        GUI.BeginGroup(content);

        #region Slicing the content area.
        Rect characterStatus = new Rect(
            0,
            0,
            content.width / 4,
            content.height);
        Rect spellSelection = new Rect(
            characterStatus.width,
            0,
            content.width / 3,
            content.height);
        Rect scoreDisplay = new Rect(
            content.width * 7 /12,
            0,
            content.width * 1 / 6,
            content.height);
        Rect turnList = new Rect(
            content.width * 3 / 4,
            0,
            content.width * 1 / 4,
            content.height);
        #endregion

        //GUI.Box(characterStatus, "CharacterStatus", this.DefaultSkin.box);
        //GUI.Box(spellSelection, "SpellSelection", this.DefaultSkin.box);
        //GUI.Box(scoreDisplay, "ScoreDisplay", this.DefaultSkin.box);
        //GUI.Box(turnList, "TurnList", this.DefaultSkin.box);

        this.DrawCharacterStatus(characterStatus);
        this.DrawSpellSelection(spellSelection);
        this.DrawScore(scoreDisplay);
        this.DrawTurnList(turnList);

        GUI.EndGroup();//content
        GUI.EndGroup();//wrapper

        this.DrawNotifcation(notificationArea);
        this.DrawHoverInfo(hoverArea);
        this.DrawMouseIcon();

        /*
        Rect concept = new Rect(0, 0, 1008, 753);
        GUI.Label(concept, "", this.conceptStyle);

        this.Skin.GetStyle("CursedText").fontSize = 28;
        Rect temp = new Rect(143, 437, 256, 32);
        GUI.Label(temp, "Super Effective!", "CursedText");
        this.Skin.GetStyle("CursedText").fontSize = 0;
        */
    }

    /// <summary>
    /// Displays the party members and their health.
    /// </summary>
    /// <param name="content">The area allotted for the GUI element.</param>
    private void DrawCharacterStatus(Rect content) {
        GUI.BeginGroup(content);

        int numCharacters = 4;
        int height = (int)content.height / numCharacters;
        //Create the various subcomponent rectangles.
        #region Subcomponents...
        Rect active = new Rect(0, 0, content.width, height);
        Rect name = new Rect(0, 0, content.width, 31);
        Rect health = new Rect(16, 24, content.width, 18);
        #endregion

        foreach (Creature pc in this.BattleScript.Battle.FriendlyTeam) {
            if (pc == this.BattleScript.ActiveCreature.Creature) {
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.Box(active, "", this.activeHighlightStyle);
                GUI.color = Color.white;
            }
            GUI.Label(name, pc.Name);
            GUI.Label(health, "Health: " + pc.Health, "LegendaryText");

            active.y += height;
            name.y += height;
            health.y += height;
        }
        GUI.EndGroup();
    }

    /// <summary>
    /// Displays the available spells for the active character.
    /// </summary>
    /// <param name="content">The area allotted for the GUI element.</param>
    private void DrawSpellSelection(Rect content) {
        //GUI.BeginGroup(content);
        //Spell selection.
        //Rect location = new Rect(
        //    Screen.width - this.Size.x - this.Padding,
        //    Screen.height - this.Size.y - this.Padding,
        //    this.Size.x,
        //    this.Size.y);

        //Get the current creature.
        Creature active = this.BattleScript.ActiveCreature.Creature;

        if (!active.AIControlled) {
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.Box(content, "", this.DefaultSkin.box);
            GUI.color = Color.white;

            this.SelectedIndex = GUI.SelectionGrid(content, this.SelectedIndex, active.Prototype.GetPowerNames(), 2, this.Skin.button);
            
            //No double casting.
            if (this.BattleScript.NotificationTimeRemaining > 0) this.SelectedIndex = -1;
        }
        //GUI.EndGroup();
    }

    /// <summary>
    /// Displays the current score.
    /// </summary>
    /// <param name="content">The area allotted for the GUI element.</param>
    private void DrawScore(Rect content) {
        GUI.BeginGroup(content);
        Rect label = new Rect(0, content.height / 3, content.width, content.height / 6);
        Rect score = new Rect(0, content.height / 2, content.width, content.height / 6);

        //Set style alignment.
        string style = "BoldText";
        this.Skin.GetStyle(style).alignment = TextAnchor.MiddleCenter;
        int origSize = this.Skin.GetStyle(style).fontSize;

        this.Skin.GetStyle(style).fontSize = 20;
        GUI.Label(label, "Score:", style);
        this.Skin.GetStyle(style).fontSize = 32;
        GUI.Label(score, this.BattleScript.Score.ToString(), style);

        //Reset style alignment.
        this.Skin.GetStyle(style).alignment = TextAnchor.MiddleLeft;
        this.Skin.GetStyle(style).fontSize = origSize;

        GUI.EndGroup();
    }

    /// <summary>
    /// Displays the upcoming turns.
    /// </summary>
    /// <param name="content">The area allotted for the GUI element.</param>
    private void DrawTurnList(Rect content) {
        GUI.BeginGroup(content);

        #region UnitList
        //Temporarily change style properties.
        int origSize = this.Skin.GetStyle("CursedText").fontSize;
        int tempSize = 20;

        this.Skin.GetStyle("CursedText").alignment = TextAnchor.MiddleRight;
        this.Skin.GetStyle("CursedText").fontSize = tempSize;
        this.Skin.GetStyle("LegendaryText").alignment = TextAnchor.MiddleRight;
        this.Skin.GetStyle("LegendaryText").fontSize = tempSize;

        Rect turn = new Rect(0, 0, content.width, content.height / 7);
        //Render each creature name
        foreach (AnimatedCreature ac in this.BattleScript.TurnList) {
            if (ac.Creature.AIControlled) {
                GUI.Label(turn, ac.Creature.Name, "CursedText");
            } else {
                GUI.Label(turn, ac.Creature.Name, "LegendaryText");
            }
            turn.y += turn.height;
        }

        //Reset the style alignments.
        this.Skin.GetStyle("CursedText").alignment = TextAnchor.MiddleLeft;
        this.Skin.GetStyle("CursedText").fontSize = origSize;
        this.Skin.GetStyle("LegendaryText").alignment = TextAnchor.MiddleLeft;
        this.Skin.GetStyle("LegendaryText").fontSize = origSize;
        #endregion

        #region Timer
        //Don't display timer for monster turns...
        if (this.BattleScript.TurnTimeRemaining != null) {
            Rect label = new Rect(0, content.height / 3, content.width / 2, content.height / 6);
            Rect timer = new Rect(0, content.height / 2, content.width / 2, content.height / 6);

            //Select style based on time remaining.
            int time = (int)Math.Round((float)this.BattleScript.TurnTimeRemaining);
            string style = "BoldText";
            if (time > this.BattleScript.TurnTime * 2 / 3) {
                style = "LegendaryText";
            }
            if (time < this.BattleScript.TurnTime / 3) {
                style = "CursedText";
            }

            //Set style alignment.
            this.Skin.GetStyle(style).alignment = TextAnchor.MiddleCenter;
            origSize = this.Skin.GetStyle("CursedText").fontSize;

            this.Skin.GetStyle(style).fontSize = 20;
            GUI.Label(label, "Time Remaining:", style);
            this.Skin.GetStyle(style).fontSize = 32;
            GUI.Label(timer, time.ToString(), style);

            //Reset style alignment.
            this.Skin.GetStyle(style).alignment = TextAnchor.MiddleLeft;
            this.Skin.GetStyle(style).fontSize = origSize;
        }
        #endregion
        GUI.EndGroup();
    }

    /// <summary>
    /// Draws the current notifcation, if any.
    /// </summary>
    /// <param name="notificationArea">The area allotted for the GUI element.</param>
    private void DrawNotifcation(Rect notificationArea) {
        if (this.BattleScript.NotificationTimeRemaining > 0) {
            int restoreFont = this.DefaultSkin.box.fontSize;
            TextAnchor restoreAnchor = this.DefaultSkin.box.alignment;

            this.DefaultSkin.box.fontSize = 20;
            this.DefaultSkin.box.alignment = TextAnchor.MiddleCenter;

            GUI.Box(notificationArea, this.BattleScript.DamageReport.ToString(), this.DefaultSkin.box);

            this.DefaultSkin.box.fontSize = restoreFont;
            this.DefaultSkin.box.alignment = restoreAnchor;
        }
    }

    /// <summary>
    /// Draws popup windows with creature information if the mouse is hovering
    /// over a creature.
    /// </summary>
    /// <param name="content">The area allotted for the GUI element.</param>
    private void DrawHoverInfo(Rect content) {
        //Find the creature being hovered, if it exists.
        AnimatedCreature hovered = null;
        foreach (AnimatedCreature ac in this.BattleScript.AnimatedCreatures) {
            if (ac.Hovering) {
                hovered = ac;
                break;
            }
        }
        if (hovered == null) return;

        //Map the creature's position to screen position.
        Vector3 pos = Camera.mainCamera.WorldToScreenPoint(hovered.Sprite.transform.position);
        if (hovered.Creature.Hostile) {
            content.x = pos.x + 48;
        } else {
            content.x = pos.x - content.width - 48;
        }
        content.y = Screen.height - pos.y - 32;

        //Create info string.
        string info = "";
        info += hovered.Creature.Name + "\n";
        info += "Element: " + hovered.Creature.Prototype.Element+ "\n";
        info += "Health: " + hovered.Creature.Health;

        int restoreFont = this.DefaultSkin.box.fontSize;
        TextAnchor restoreAnchor = this.DefaultSkin.box.alignment;

        //this.DefaultSkin.box.fontSize = 20;
        this.DefaultSkin.box.alignment = TextAnchor.MiddleLeft;

        GUI.Box(content, info, this.DefaultSkin.box);

        this.DefaultSkin.box.fontSize = restoreFont;
        this.DefaultSkin.box.alignment = restoreAnchor;
    }

    /// <summary>
    /// Draws the icon for the currently selected power.
    /// </summary>
    private void DrawMouseIcon() {
        //Get the proper icon, if such exists.
        if (this.SelectedIndex == -1) {
            Screen.showCursor = true;
            return;
        }
        Texture2D icon = this.ElementIcons[(int)this.SelectedPower.Element];
        if (icon == null) {
            Screen.showCursor = true;
            return;
        }

        Screen.showCursor = false;

        Rect rect = new Rect(
            Input.mousePosition.x - icon.width / 2,
            Screen.height - Input.mousePosition.y - icon.height / 2,
            icon.width,
            icon.height);
        GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
    }
}