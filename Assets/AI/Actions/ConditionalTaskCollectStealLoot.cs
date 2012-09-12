using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class ConditionalTaskCollectStealLoot : Action
{
	public ConditionalTaskCollectStealLoot()
	{
		actionName = "ConditionalTaskCollectSteal";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
		//Get the current task.
        CharacterScript character = this.actionContext.GetContextItem<CharacterScript>("character");
        //Eh, null tasks mean to wander.
        if (character.ActiveTask == null) return ActionResult.FAILURE;
        //Test task.
        if (character.ActiveTask.Type == "collect" ||
            character.ActiveTask.Type == "steal" ||
            character.ActiveTask.Type == "loot-corpse")
            return ActionResult.SUCCESS;

        return ActionResult.FAILURE;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
