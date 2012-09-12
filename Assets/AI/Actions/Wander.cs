using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;
using RAIN.Motion.Instantaneous;

public class Wander : Action
{
    /// <summary>
    /// The specific wandering behavior to use.
    /// </summary>
    private SB_Wander WanderBehavior { get; set; }
    /// <summary>
    /// The amount of time to move in this wander action.  This time is chosen
    /// randomly.
    /// </summary>
    private float MoveTime { get; set; }
    /// <summary>
    /// The amount of time to wait in this wander action.  This time is chosen
    /// randomly.
    /// </summary>
    private float WaitTime { get; set; }

    /// <summary>
    /// The game object that the agent to which the agent is attached.
    /// </summary>
    private CharacterScript Character { get; set; }

	public Wander()
	{
		actionName = "Wander";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
        //Create move and look behaviors for wander to use.
        SB_Seek move = new SB_Seek();
        SB_LookWhereYoureGoing look = new SB_LookWhereYoureGoing(0, 0.1f, 0, 0);
        this.WanderBehavior = new SB_Wander(1.5f, 2f, 15 * Mathf.Deg2Rad, move, look);
        
        //Snag reference to character script so we don't have to lookup each frame.
        this.Character = this.actionContext.GetContextItem<CharacterScript>("character");

        //Set the randomized times.
        this.MoveTime = Time.time + Random.Range(4f, 8f);
        this.WaitTime = this.MoveTime + Random.Range(2f, 8f);

		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //If the character is dead, we don't want to wander.
        if (this.Character.Dead) return ActionResult.FAILURE;

		//Check for completion.
        if (Time.time >= this.WaitTime || this.Character.ActiveTask != null) return ActionResult.SUCCESS;
        
        //Continue wandering.
        if (this.MoveTime >= Time.time) {
            Steering steering = this.WanderBehavior.Steer(agent.Kinematic);
            agent.Kinematic.SetVelocity(0.25f * steering.Velocity, steering.AngularVelocity);
        }

        return ActionResult.RUNNING;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}