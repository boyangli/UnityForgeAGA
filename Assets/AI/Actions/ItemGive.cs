using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;
using WorldEngine.Items;

public class ItemGive : Action
{
	public ItemGive()
	{
		actionName = "ItemGive";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the item from my inventory and add to the actee's.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        CharacterScript actee = actor.ActiveTask.Actee;
        Item item = actor.ActiveTask.Item;

        bool taken = actor.Inventory.Remove(item);
        actee.Inventory.Add(item);

        if (taken) return ActionResult.SUCCESS;
        return ActionResult.FAILURE;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
