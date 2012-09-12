using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

using StoryEngine;
using WorldEngine;

public class SayPostDialogue : Action
{
    public SayPostDialogue()
	{
        actionName = "SayPostDialogue";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        //Get the gui script and task.
        WorldGUI wgui = Globals.Instance.WorldGUI;
        Task task = this.actionContext.GetContextItem<CharacterScript>("character").ActiveTask;

        //Set the dialogue and flag the GUI to display it.
        wgui.Dialogue = task.PostDialogue;
        wgui.DisplayDialogue = true;

        return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the gui script and check for dialogue close.
        Task task = this.actionContext.GetContextItem<CharacterScript>("character").ActiveTask;

        if (task.PostDialogue.Spoken) return ActionResult.SUCCESS;
        else return ActionResult.RUNNING;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
