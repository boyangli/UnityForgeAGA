using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class ConditionalTaskPickup : Action
{
	public ConditionalTaskPickup()
	{
		actionName = "ConditionalTaskPickup";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the current task.
        CharacterScript cs = this.actionContext.GetContextItem<CharacterScript>("character");
        //Eh, null tasks mean to wander.
        if (cs.ActiveTask == null) return ActionResult.FAILURE;
        //Test task.
        if (cs.ActiveTask.Type == "pickup") return ActionResult.SUCCESS;
        return ActionResult.FAILURE;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
