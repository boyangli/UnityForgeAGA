using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;
using WorldEngine.Items;


public class ItemTake : Action
{
	public ItemTake()
	{
		actionName = "ItemTake";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the item from the actee inventory and add to mine.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        CharacterScript actee = actor.ActiveTask.Actee;
        Item item = actor.ActiveTask.Item;

        bool taken = actee.Inventory.Remove(item);
        if (!taken) return ActionResult.FAILURE;
        
        actor.Inventory.Add(item);
        return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
