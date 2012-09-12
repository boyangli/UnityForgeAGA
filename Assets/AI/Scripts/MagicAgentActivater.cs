using UnityEngine;
using RAIN.Core;
using RAIN.Belief;
using RAIN.BehaviorTrees;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;

public class MagicAgentActivater : BTActivationManager {

    //public WorldScript WorldScript;

    /*
     * CharacterScript (Me)
     *   -ActiveTask (stored as string)
     *   -TaskParameters (list / dictionary of strings)
     * WorldScript (MyWorld)
     */

    public override void InitBehavior(Agent actor) {
        //Find the relevant variables from the parent object.
        WorldScript ws = GameObject.Find("Root").GetComponent<WorldScript>();
        WorldGUI wGUI = GameObject.Find("Root").GetComponent<WorldGUI>();
        CharacterScript cs = this.transform.parent.GetComponent<CharacterScript>();
        
        if (ws == null || wGUI == null || cs == null) Debug.LogError("Error finding attached scripts during activation.");
        
        actor.actionContext.AddContextItem<WorldScript>("world", ws);
        actor.actionContext.AddContextItem<WorldGUI>("gui", wGUI);
        actor.actionContext.AddContextItem<CharacterScript>("character", cs);
        actor.actionContext.AddContextItem<GameObject>("moveTarget", cs.gameObject);

        #region Temp testing.
        //CharacterScript player = ws.PartyCharacter;
        //Item sword = ws.GetItemByName("Sword");
        //LocaleScript locale = ws.GetLocaleByName("Happyville");

        //Debug.Log(locale);

        //Task task = new Task("deliver", cs, player, sword, locale);
        //ws.DramaManager.EmergencyRepair(task);
        //cs.ActiveTask = task;
        #endregion
    }

    protected override void PreAction(Agent actor, float deltaTime) {

    }
}