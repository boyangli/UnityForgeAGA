using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class ConditionalTaskExists : Action
{
	public ConditionalTaskExists()
	{
		actionName = "ConditionalTaskExists";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the character's task, if it exists, we should start observing.
        CharacterScript character = this.actionContext.GetContextItem<CharacterScript>("character");

        if (character.ActiveTask != null) return ActionResult.SUCCESS;
		return ActionResult.FAILURE;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
