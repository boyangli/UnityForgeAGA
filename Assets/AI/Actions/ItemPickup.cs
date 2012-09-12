using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;
using WorldEngine.Items;

public class ItemPickup : Action
{
	public ItemPickup()
	{
		actionName = "ItemPickup";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the character and item.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        Item item = actor.ActiveTask.Item;

        //Get worldscript and pickup item.
        WorldScript ws = this.actionContext.GetContextItem<WorldScript>("world");
        bool success = ws.PickupItem(actor, item);

        if (success) return ActionResult.SUCCESS;
        else return ActionResult.FAILURE;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
