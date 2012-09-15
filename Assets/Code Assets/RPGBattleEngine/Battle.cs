using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using BattleEngine.Creatures;

namespace BattleEngine {
    /// <summary>
    /// Simplistic container for storing creatures in a battle.
    /// </summary>
    public class Battle {
        #region Attributes
        /// <summary>
        /// The set of creatures allied to the player.
        /// </summary>
        [XmlIgnore]
        public List<Creature> FriendlyTeam { get; private set; }
        /// <summary>
        /// The set of AI-controlled 'monster' creatures.
        /// </summary>
        [XmlIgnore]
        public List<Creature> HostileTeam { get; private set; }
        /// <summary>
        /// The list of friends, referenced by name.  Only used for serialization.
        /// </summary>
        public List<string> FriendlyNames { get; set; }
        /// <summary>
        /// The list of hostile, referenced by name.  Only used for serialization.
        /// </summary>
        public List<string> HostileNames { get; set; }

        /// <summary>
        /// A random number generator for selecting targets.
        /// </summary>
        private static readonly System.Random RNG = new System.Random();
        #endregion

        /// <summary>
        /// Creates a (rather boring) empty battle.
        /// </summary>
        public Battle() {
            this.FriendlyTeam = new List<Creature>();
            this.HostileTeam = new List<Creature>();
            this.FriendlyNames = new List<string>();
            this.HostileNames = new List<string>();
        }

        /// <summary>
        /// Gets a random target for a given creature.
        /// </summary>
        /// <param name="attacker">The creature requesting a target.</param>
        /// <returns>A random, valid target.</returns>
        public Creature GetRandomTarget(Creature attacker) {
            //Snag all the valid targets.
            List<Creature> otherTeam = (this.FriendlyTeam.Contains(attacker)) ? this.HostileTeam : this.FriendlyTeam;
            List<Creature> validTargets = new List<Creature>();

            foreach (Creature c in otherTeam) {
                if (c.Health > 0) validTargets.Add(c);
            }

            //Pick a random one.
            return validTargets[Battle.RNG.Next(validTargets.Count)];
        }

        /// <summary>
        /// Creates creatures from the list of prototypes and adds them to the
        /// friendly team.
        /// </summary>
        /// <param name="prototypes">The list of creature prototypes.</param>
        public void AddFriendlyPrototypes(List<CreaturePrototype> prototypes) {
            foreach (CreaturePrototype prototype in prototypes) this.FriendlyTeam.Add(new Creature(prototype));
        }

        /// <summary>
        /// Creates creatures from the list of prototypes and adds them to the
        /// hostile team.
        /// </summary>
        /// <param name="prototypes">The list of creature prototypes.</param>
        public void AddHostilePrototypes(List<CreaturePrototype> prototypes) {
            foreach (CreaturePrototype prototype in prototypes) this.HostileTeam.Add(new Creature(prototype));
        }

        /// <summary>
        /// Loads battle teams from a bestiary.
        /// </summary>
        /// <param name="bst">The bestiary containing the referenced creatures.</param>
        /// <returns>The number of loaded creatures.</returns>
        public int LoadCreaturesFromNames(Bestiary bst) {
            //Create lists.
            this.FriendlyTeam = new List<Creature>();
            this.HostileTeam = new List<Creature>();

            int referenced = 0;
            //Lookup friendly creatures.
            foreach (string name in this.FriendlyNames) {
                CreaturePrototype prototype = bst.GetCreature(name);
                if (prototype == null) {
                    Debug.LogWarning("Battle Creature not found in Bestiary. (" + name + ")");
                } else {
                    this.FriendlyTeam.Add(new Creature(prototype));
                    referenced++;
                }
            }

            //Lookup hostile creatures.
            foreach (string name in this.HostileNames) {
                CreaturePrototype prototype = bst.GetCreature(name);
                if (prototype == null) {
                    Debug.LogWarning("Battle Creature not found in Bestiary. (" + name + ")");
                } else {
                    this.HostileTeam.Add(new Creature(prototype));
                    referenced++;
                }
            }
            return referenced;
        }
    }
}