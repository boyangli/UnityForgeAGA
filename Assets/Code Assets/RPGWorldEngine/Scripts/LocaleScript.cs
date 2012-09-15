using UnityEngine;
using RAIN.Sensors;
using RAIN.Path;
using WorldEngine;

/// <summary>
/// Simplistic trigger script that alerts the system when the player leaves a
/// locale.  The system assumes that locale colliders don't overlap.
/// </summary>
public class LocaleScript : MonoBehaviour {

    /// <summary>
    /// The locale's x-z position in the world.
    /// </summary>
    public Vector2 Location {
        get {
            Vector3 worldPos = this.transform.position;
            return new Vector2(worldPos.x, worldPos.z);
        }
    }

    ///// <summary>
    ///// Called whenver another colliders exits the trigger collider.
    ///// </summary>
    ///// <param name="other"></param>
    //public void OnTriggerExit(Collider other) {
    //    //If the player is an NPC, ignore these events.
    //    if (Globals.Instance.DMScript.PlayerIsNPC) return;

    //    //Ignore anything that isn't an ObstacleAvoidanceCollider
    //    if (other.GetComponent<ObstacleAvoidanceCollider>()) {
    //        CharacterScript agent = other.transform.parent.gameObject.GetComponent<CharacterScript>();
    //        //If other is the player character, popup the fast travel GUI widget.
    //        if (agent == Globals.Instance.WorldScript.PartyCharacter) {
    //            Debug.Log(agent.name + " leaving locale: " + this.name);
    //            //Display the fast travel menu.
    //            Globals.Instance.WorldGUI.DisplayLocales = true;
    //            agent.Stop();
    //        }
    //    }
    //}
}
