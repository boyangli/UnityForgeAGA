using System;
using UnityEngine;
using RAIN.Core;
using RAIN.Motion;

/// <summary>
/// MODIFY THIS SCRIPT TO MAKE THIS COMPONENT AVAILABLE FOR MIXAMO ANIMATION SUPPORT
/// </summary>

public class MixamoAIAnimationExtender { } //DELETE THIS LINE AND UNCOMMENT THOSE BELOW

/* DELETE THIS LINE TO UNCOMMENT (THERE IS ANOTHER ONE AT THE BOTTOM)
[AddComponentMenu("RAIN/Mixamo/MixamoAIAnimationExtender")]
public class MixamoAIAnimationExtender : AIAnimationExtender
{
    private AnimationStateMachine _animationStateMachine;

    public MixamoAIAnimationExtender()
	{			
	}
	
	public void Start()
	{
		_animationStateMachine = gameObject.GetComponent<AnimationStateMachine>();
        _animationStateMachine.RootMotionMode = AnimationStateMachine.RootMotionModeType.Automatic;

    }
	
    public override void SetAnimationState(string state)
	{
		_animationStateMachine.ChangeState(state);
	}
	
    public override void SetAnimationSpeed(string state, float speed)
	{
		_animationStateMachine.ControlWeights[state + "_speed"] = speed;
	}
	
    public override void DoAnimate(float deltaTime) { }
	
    public override void GetMotionResults(out Vector3 translation, out Vector3 rotation)
    {
		AnimationStateMachine.RootMotionResult rmr = _animationStateMachine.GetRootMotion();
		if (rmr != null) translation = rmr.GlobalTranslation;
		else translation = Vector3.zero;
		rotation = Vector3.zero;
    }
}
DELETE THIS LINE TO UNCOMMENT (THERE IS ANOTHER ONE AT THE TOP)*/
