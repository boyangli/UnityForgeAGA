using System.Collections.Generic;

using BattleEngine;
using BattleEngine.Creatures;
using BattleEngine.Powers;
using WorldEngine.Items;
using UnityEngine;

namespace WorldEngine {
    /// <summary>
    /// Singleton class for allowing the World and Battle systems to share
    /// variables across scene loading transitions.
    /// </summary>
    public class Globals {

        #region Instancing
        /// <summary>
        /// The shared, static instance of this class.
        /// </summary>
        public static Globals Instance {
            get {
                if (Globals.instance == null) instance = new Globals();
                return instance;
            }
        }
        private static Globals instance;
        #endregion

        #region Attributes
        /// <summary>
        /// The current battle that should be used by the combat engine.
        /// </summary>
        public Battle Battle { get; set; }
        /// <summary>
        /// The persistent root object for the world.
        /// </summary>
        public GameObject WorldRoot { get;  set; }
        /// <summary>
        /// The WorldScript attached to the persistent root object for the world.
        /// </summary>
        public WorldScript WorldScript {
            get { return this.WorldRoot.GetComponent<WorldScript>(); }
        }
        /// <summary>
        /// The WorldGUI attached to the persistent root object for the world.
        /// </summary>
        public WorldGUI WorldGUI {
            get { return this.WorldRoot.GetComponent<WorldGUI>(); }
        }
        /// <summary>
        /// The Drmama Manager Script running the generated stories.
        /// </summary>
        public DramaManagerScript DMScript { get; set; }

        /// <summary>
        /// The deserialized Bestiary.
        /// </summary>
        public Bestiary Bestiary { get; set; }
        /// <summary>
        /// The desserialized catalog.
        /// </summary>
        public Catalog Catalog { get; set; }
        /// <summary>
        /// The deserialized PowerBook.
        /// </summary>
        public PowerBook PowerBook { get; set; }

        /// <summary>
        /// The list of battle reports for each combat engaged.
        /// </summary>
        public List<BattleReport> Reports { get; private set; }
        /// <summary>
        /// The cumulative score for all battles thus far.
        /// </summary>
        public int Score { get; set; }
        #endregion

        /// <summary>
        /// Private constructor.
        /// </summary>
        private Globals() {
            this.Reports = new List<BattleReport>();
        }

        /// <summary>
        /// Ractivates the world root and flags a cleanup.
        /// </summary>
        public void ActivateRoot() {
            this.WorldScript.ActivateAll(true);
        }
    }
}
