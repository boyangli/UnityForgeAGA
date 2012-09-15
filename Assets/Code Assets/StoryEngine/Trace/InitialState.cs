using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryEngine.Trace {
    /// <summary>
    /// Datastructure containing the initial state configuration for an episode.
    /// </summary>
    public class InitialState {
        /// <summary>
        /// The predicate mapping characters to health statuses.
        /// </summary>
        public List<Proposition> HealthStatuses { get; private set; }
        /// <summary>
        /// The predicate mapping entities to locations.
        /// </summary>
        public List<Proposition> Locations { get; private set; } 
        /// <summary>
        /// The predicate mapping items to their owning characters.
        /// </summary>
        public List<Proposition> Possessions { get; private set; }

        /// <summary>
        /// Private constructor for the purposes of deserialization.
        /// </summary>
        private InitialState() { }
    }
}