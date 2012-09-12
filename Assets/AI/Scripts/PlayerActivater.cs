using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.BehaviorTrees;

public class PlayerActivater : BTActivationManager {
    public override void InitBehavior(Agent actor) {
        CharacterScript cs = this.transform.parent.GetComponent<CharacterScript>();
        PlayerController pc = this.transform.parent.GetComponent<PlayerController>();

        if (cs == null || pc == null) Debug.LogError("Error finding attached scripts during activation.");

        actor.actionContext.AddContextItem<CharacterScript>("character", cs);
        actor.actionContext.AddContextItem<PlayerController>("controller", pc);
    }

    protected override void PreAction(Agent actor, float deltaTime) {
    }
}