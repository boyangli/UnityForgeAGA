using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class KillRevive : Action
{
	public KillRevive()
	{
		actionName = "KillRevive";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the actee.  Items are not consumed when used to kill or revive, so we don't need those.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        CharacterScript actee = actor.ActiveTask.Actee;
		
        //I was too lazy to make two separate Sequencers.
        actee.Dead = actor.ActiveTask.Type == "kill-by-item";

        return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}