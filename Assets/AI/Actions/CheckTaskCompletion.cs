using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

using StoryEngine;

public class CheckTaskCompletion : Action {
    
    /// <summary>
    /// A reference to the player controller.
    /// </summary>
    private PlayerController controller;

    public CheckTaskCompletion() {
        actionName = "CheckTaskCompletion";
    }

    public override ActionResult Start(Agent agent, float deltaTime) {
//        //Get the current task and assign it to the player controller.
//        Task task = this.actionContext.GetContextItem<CharacterScript>("character").ActiveTask;
//        this.controller = this.actionContext.GetContextItem<PlayerController>("controller");
//        this.controller.Context = task;

        return ActionResult.SUCCESS;
    }

    public override ActionResult Execute(Agent agent, float deltaTime) {
        //If the controller has set a pathing request, allow the agent to path.
        agent.PathManager.Move(agent, deltaTime);

        //Check if the player has acted yet.
//        if (this.controller.Acted) 
			//return ActionResult.SUCCESS;
        return ActionResult.RUNNING;
    }

    public override ActionResult Stop(Agent agent, float deltaTime) {
        //Clear the player controller's contextual task.
//        this.actionContext.GetContextItem<PlayerController>("controller").Context = null;
        return ActionResult.SUCCESS;
    }
}
