using UnityEngine;

using StoryEngine;
using WorldEngine;

public class DramaManagerScript : MonoBehaviour {

    /// <summary>
    /// If true, the Drama Manager will also repair the script for the player.
    /// </summary>
    public bool PlayerIsNPC = false;

    /// <summary>
    /// The DramaManager for this world.
    /// </summary>
    public DramaManager DramaManager { get; private set; }

	// Use this for initialization
	void Start () {
        this.DramaManager = new DramaManager(Globals.Instance.WorldScript);
        this.DramaManager.RepairPlayer = this.PlayerIsNPC;

        Globals.Instance.DMScript = this;
	}
	
	// Update is called once per frame
	void Update () {
        //Let the DM observe the status of the world.
        this.DramaManager.Update();
	}
}