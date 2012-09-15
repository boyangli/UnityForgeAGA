using System;
using System.Collections.Generic;
using BattleEngine.Creatures;
using BattleEngine.Powers;

namespace StateSpace {
    /// <summary>
    /// A streamlined creature datastructure for sending to the learner server.
    /// </summary>
    public class CreatureState {
        /// <summary>
        /// The name of the creature.  Should only really be used 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The current health of the creature.
        /// </summary>
        public int Health { get; set; }
        /// <summary>
        /// The type enumeration of the creature.
        /// </summary>
        public Element Element { get; set; }
        /// <summary>
        /// True, if the creature belongs to the hostile team.
        /// </summary>
        public bool IsHostile { get; set; }
        /// <summary>
        /// The list of powers available to the creature.
        /// </summary>
        public List<Power> Powers { get; set; }

        /// <summary>
        /// Private parameterless constructor for the purposes of serialization.
        /// </summary>
        private CreatureState() { }

        /// <summary>
        /// Creates a creature state from the attributes of a creature.
        /// </summary>
        /// <param name="creature"></param>
        public CreatureState(Creature creature) {
            this.Name = creature.Name;
            this.Health = creature.Health;
            this.Element = creature.Prototype.Element;
            this.IsHostile = creature.Hostile;
            this.Powers = creature.Prototype.AtWillPowers;
        }
    }
}