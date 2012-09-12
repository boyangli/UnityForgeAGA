using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

using WorldEngine.Items;

public class SetMoveItem : Action
{
	public SetMoveItem()
	{
		actionName = "SetMoveItem";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the item gameobject.
        CharacterScript actor = this.actionContext.GetContextItem<CharacterScript>("character");
        Item item = actor.ActiveTask.Item;

        //Verify that the item is on the ground.
        if (item.Owner.GetComponent<ItemScript>() == null) return ActionResult.FAILURE;

        this.actionContext.SetContextItem<GameObject>("moveTarget", item.Owner);

        return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
