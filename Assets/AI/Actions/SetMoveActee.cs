using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class SetMoveActee : Action
{

	public SetMoveActee()
	{
		actionName = "SetMoveActee";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the actee gameobject.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        CharacterScript actee = actor.ActiveTask.Actee;

        this.actionContext.SetContextItem<GameObject>("moveTarget", actee.gameObject);

		return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
