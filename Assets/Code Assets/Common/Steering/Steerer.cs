using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class Steerer : MonoBehaviour {

    /// <summary>
    /// The maximum force that be imparted on this script's RigidBody.
    /// </summary>
    public float MaxForce = 16;
    /// <summary>
    /// The maximum speed that can be attained as a result of steering.
    /// External forces can cause greater speeds to occur.
    /// </summary>
    public float MaxSpeed = 2f;
    /// <summary>
    /// The distance at which the steering force will begin to lerp to 0 when
    /// arrival forces are applied.
    /// </summary>
    public float BrakingDistance = 2;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Steers the rigidbody towards a specified position in space. This steering
    /// behavior applies force to the rigidbody to procude a velocity that is
    /// radially aligned towards the target location.
    /// </summary>
    /// <param name="location">The location to seek towards.</param>
    public void Seek(Vector3 location) {
        this.rigidbody.AddForce(this.SeekForce(location), ForceMode.Force);
    }

    /// <summary>
    /// Steers the rigidbody away from a specified position in space.
    /// </summary>
    /// <param name="location"></param>
    public void Flee(Vector3 location) {
        this.rigidbody.AddForce(-this.SeekForce(location), ForceMode.Force);
    }

    /// <summary>
    /// Steers the rigid body towards a specified position in space such that
    /// it has no velocity when it arrives.
    /// </summary>
    /// <param name="location"></param>
    public void Arrive(Vector3 location) {
        //Almost the same as Seek, except the desired velocity is scaled back.
        Vector3 toTarget = location - this.transform.position;
        float distance = toTarget.magnitude;

        float rampedSpeed = this.MaxSpeed * distance / this.BrakingDistance;
        float speed = Mathf.Min(rampedSpeed, this.MaxSpeed);
        
        Vector3 desiredVelocity = speed * Vector3.Normalize(toTarget);

        Vector3 desiredImpulse = (desiredVelocity - this.rigidbody.velocity) * this.rigidbody.mass;
        //Clamp force within maximum.
        if (desiredImpulse.sqrMagnitude > this.MaxForce * this.MaxForce) {
            desiredImpulse = this.MaxForce * desiredImpulse.normalized;
        }

        this.rigidbody.AddForce(desiredImpulse, ForceMode.Force);
    }

    /// <summary>
    /// Helper method for calculating seek force, since it's used in various functions.
    /// </summary>
    /// <param name="location">The location to seek towards.</param>
    /// <returns>The seeking force vector.</returns>
    private Vector3 SeekForce(Vector3 location) {
        Vector3 desiredVelocity = this.MaxSpeed * Vector3.Normalize(location - this.transform.position);
        Vector3 desiredImpulse = (desiredVelocity - this.rigidbody.velocity) * this.rigidbody.mass;

        //Clamp force within maximum.
        if (desiredImpulse.sqrMagnitude > this.MaxForce * this.MaxForce) {
            desiredImpulse = this.MaxForce * desiredImpulse.normalized;
        }
        return desiredImpulse;
    }
}