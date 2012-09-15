using UnityEngine;

public class PlatformerController : MonoBehaviour {

    /// <summary>
    /// The controller's walking force.
    /// </summary>
    public float WalkForce = 1;
    /// <summary>
    /// The controller's maximum walking speed.
    /// </summary>
    public float WalkSpeed = 0.25f;
    /// <summary>
    /// The controller's 'aerial walking' force multiplier.  Whilst in the air,
    /// the player can still direct the character, but (normally) to a much
    /// lesser extent than when on the ground.
    /// </summary>
    public float AerialWalkMultiplier = 0.1f;
    /// <summary>
    /// The controller's jumping force. Gravity defaults to (0, -9.81, 0), meaning
    /// the value must be greater than 9.81 to actually lift the controller.
    /// </summary>
    public float JumpForce = 16;
    /// <summary>
    /// The maximum number of seconds to permit jump input.  This allows the
    /// player to perform variable height jumps by pressing the jump button for
    /// longer durations.
    /// </summary>
    public float JumpInputMaxDuration = 0.5f;
    /// <summary>
    /// The minimum number of seconds to permit jump input.  This allows the
    /// player to perform variable height jumps by pressing the jump button for
    /// breifer durations.
    /// </summary>
    public float JumpInputMinDuration = 0.25f;

    /// <summary>
    /// The layer mask to use for platforms when colliding things.
    /// </summary>
    public LayerMask PlatformLayerMask = new LayerMask();

    /// <summary>
    /// The current WalkStatus of the controlled object.
    /// </summary>
    public WalkStatus Walking { get; private set; }
    public enum WalkStatus { No, Left, Right }

    /// <summary>
    /// The current JumpStatus of the controlled object.
    /// </summary>
    public JumpStatus Jumping { get; private set; }
    public enum JumpStatus { No, Jumping, LongJumping, Airborne }

    /// <summary>
    /// The time at which the last jump was initiated.
    /// </summary>
    private float JumpTime { get; set; }

    /// <summary>
    /// Start is called just before the first call to Update().
    /// </summary>
    void Start() {
        this.JumpTime = float.NegativeInfinity;
        this.Jumping = JumpStatus.No;
        this.Walking = WalkStatus.No;
    }

    /// <summary>
    /// Update is called once per frame, if the MonoBehavior is enabled.
    /// </summary>
    void Update() {
        /* Note: These 'virtual' input devices are defined in the project's
         * input settings.  They can be edited using the following path within
         * Unity3D's editor:  Edit -> Project Settings -> Input.
         * 
         * Alternatively, you can check for specific inputs directly by using
         * any of the Input.GetKey or Input.GetMouseButton methods.
         */

        //Check inputs and move character if needed.
        float rawWalk = Input.GetAxis("Horizontal");
        float rawJump = Input.GetAxis("Jump");

        this.Walk(rawWalk);
        this.Jump(rawJump);

        //Temp shortcut in case I forget to set the layer on a platform.
        if (Input.GetKey(KeyCode.Tab)) this.Jumping = JumpStatus.No;
    }

    /// <summary>
    /// Converts raw axis input into usable a vector and moves the controlled
    /// object if appropriate.
    /// </summary>
    /// <param name="rawInput">The raw input, presumably parameterized [-1, 1].</param>
    private void Walk(float rawInput) {
        //Get the current and desired x-velocities.
        float currentVel = this.rigidbody.velocity.x;
        float desiredVel = rawInput * this.WalkSpeed;

        //Create desired force.
        float desiredImpulse = (desiredVel - currentVel) * this.rigidbody.mass;

        //Clamp force if over maximum.
        if (desiredImpulse > this.WalkForce) desiredImpulse = WalkForce;
        //Damp force if airborne.
        if (this.Jumping != JumpStatus.No) desiredImpulse *= this.AerialWalkMultiplier;

        Vector3 force = desiredImpulse * Vector3.right;
        rigidbody.AddForce(force, ForceMode.Force);

        Debug.Log("Input: " + rawInput + "WalkForce: " + force + ". WalkSpeed: " + this.rigidbody.velocity.x);

        ////Convert the input into a usuable force vector.
        //Vector3 walk = Vector3.right * rawInput * this.WalkForce;
        //if (this.Jumping != JumpStatus.No) walk *= this.AerialWalkMultiplier;

        //Debug.Log("Walk Force: " + walk);
        //rigidbody.AddForce(walk, ForceMode.Force);

        ////In the event we're over out speed limit, impose a counterforce.
        //float hVelocity = rigidbody.velocity.x;
        //float coefficient = Mathf.Abs(hVelocity / this.WalkSpeed);
        //Debug.Log("coefficient: " + coefficient);
        //if (coefficient >= 1) {
        //    Vector3 resistance = Vector3.left * (coefficient - 1) * Mathf.Sign(hVelocity) * this.WalkForce;
        //    rigidbody.AddForce(resistance, ForceMode.Force);
        //}

        //Set walking flag.
        if (0 < rawInput) this.Walking = WalkStatus.Right;
        else if (rawInput < 0) this.Walking = WalkStatus.Left;
        else this.Walking = WalkStatus.No;
    }

    /// <summary>
    /// Converts raw axis input into usable a vector and jumps the controlled
    /// object if appropriate.
    /// </summary>
    /// <param name="rawInput">The raw input, presumably parameterized [-1, 1].</param>
    private void Jump(float rawInput) {
        //(Default)Axes return values [-1, 1].  Negative values are meaningless here.
        if (rawInput < 0) {
            Debug.LogWarning("Detected negative input value for Jump axis.");
            return;
        }

        //Note: no pressure sensitive buttons - jump force is independent of input.
        Vector3 jump = Vector3.up * this.JumpForce;

        //If the jump button is pushed and Jumping.No, initiate the jump.
        if (this.Jumping == JumpStatus.No && rawInput > 0) {
            //Record jump time, advance state, and add force for this frame.
            this.JumpTime = Time.time;
            this.Jumping = JumpStatus.Jumping;
            rigidbody.AddForce(jump);
        } else if (this.Jumping == JumpStatus.Jumping) {
            //Regardless of input, force is applied for the minimum time.
            rigidbody.AddForce(jump);
            //If the minimum time is over, advance state.
            if (Time.time > this.JumpTime + this.JumpInputMinDuration) {
                this.Jumping = (rawInput > 0) ? JumpStatus.LongJumping : JumpStatus.Airborne;
            }
        } else if (this.Jumping == JumpStatus.LongJumping) {
            //Continue to add force as long as the button is pressed and time allows.
            if (Time.time < this.JumpTime + this.JumpInputMaxDuration && rawInput > 0) rigidbody.AddForce(jump);
            else this.Jumping = JumpStatus.Airborne;
        }
    }

    /// <summary>
    /// OnCollisionStay is called once per frame for every collider/rigidbody
    /// that is touching this rigidbody/collider, at least until the physics
    /// engine starts putting objects to sleep.
    /// 
    /// If you don't use the collision object the function, leave out the
    /// parameter as this avoids unneccessary calculations.
    /// </summary>
    /// <param name="collisionInfo">Collection of collision data.</param>
    public void OnCollisionStay(Collision collisionInfo) {
        foreach (ContactPoint contact in collisionInfo.contacts) {
            //Fuck bitwise operations. -_-
            int contactMask = (1 << contact.otherCollider.gameObject.layer);
            //If it's in the mask, we've landed on a platform.
            if ((this.PlatformLayerMask.value & contactMask) > 0) this.Jumping = JumpStatus.No;

            //Might as well draw these for now.
            Debug.DrawRay(contact.point, contact.normal * 2, Color.red);
        }
    }
}