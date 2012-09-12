using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

public class Teleport : Action
{

    /// <summary>
    /// Class for teleporting an agent.
    /// </summary>
	public Teleport()
	{
		actionName = "Teleport";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the character and the location to go to.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        LocaleScript locale = actor.ActiveTask.Locale;

        //Teleport the actor.
        agent.Kinematic.Position = locale.transform.position;

		return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}