using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using BattleEngine.Powers;

namespace BattleEngine.Creatures {
    /// <summary>
    /// Datastructure modelling a battle-capable character within the engine.
    /// A simplified version of the Character Sheets from 4th Edition Dungeons
    /// and Dragons.
    /// </summary>
    public class CreaturePrototype {
        #region Attributes
        /// <summary>
        /// The name of this character.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The maximum health of the character.
        /// </summary>
        public int MaxHealth { get; private set; }
        /// <summary>
        /// Powers that can be used without limitation.
        /// </summary>
        [XmlIgnore]
        public List<Power> AtWillPowers { get; private set; }
        /// <summary>
        /// The names of this creature's available powers, used to lookup powers
        /// from a PowerBook upon deserialization.
        /// </summary>
        public List<string> AtWillPowerNames { get; set; }
        /// <summary>
        /// The element of this creature.
        /// </summary>
        public Element Element { get; private set; }
        #endregion

        /// <summary>
        /// Private constructor for the purposes of serialization.
        /// </summary>
        private CreaturePrototype() {
            this.AtWillPowers = new List<Power>();
        }

        /// <summary>
        /// Creates a named default creature with no abilities or element.
        /// </summary>
        /// <param name="name"></param>
        public CreaturePrototype(string name) {
            this.MaxHealth = 100;

            this.AtWillPowers = new List<Power>();
            this.Element = Element.None;

            this.Name = name;

            //this.GeneratePowerNames();
        }

        /// <summary>
        /// Creates an array of the power names available to this creature.  Note
        /// that this array is populated from the AtWillPowers list, rather than 
        /// the names list - as not all of those may have been properly referenced
        /// from a PowerBook.
        /// </summary>
        /// <returns>String array of power names.</returns>
        public string[] GetPowerNames() {
            string[] names = new string[this.AtWillPowers.Count];

            for (int i = 0; i < this.AtWillPowers.Count; i++) {
                names[i] = this.AtWillPowers[i].Name;
            }

            return names;
        }

        /// <summary>
        /// Loads a creature's powers from a powerbook.
        /// </summary>
        /// <param name="pb">The powerbook containing the referenced powers.</param>
        /// <returns>The number of loaded powers.</returns>
        public int LoadPowersFromNames(PowerBook pb) {
            this.AtWillPowers = new List<Power>();
            foreach (string name in this.AtWillPowerNames) {
                Power power = pb.GetPower(name);
                if (power == null) {
                    Debug.LogWarning("Creature Power not found in PowerBook. (" + name + ")");
                } else {
                    this.AtWillPowers.Add(power);
                }
            }
            return this.AtWillPowers.Count;
        }
    }
}