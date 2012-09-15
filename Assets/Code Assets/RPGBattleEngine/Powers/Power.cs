using System;
using System.Collections.Generic;
using BattleEngine.Creatures;

namespace BattleEngine.Powers {
    /// <summary>
    /// A class responsible for containing and applying creature powers.
    /// </summary>
    public class Power {
        #region Attributes
        /// <summary>
        /// The name of this ability.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The unmodified minimum damage that this abilty deals.
        /// </summary>
        public int MinDamage { get; private set; }
        /// <summary>
        /// The unmodified maximum damage that this ability deals.
        /// </summary>
        public int MaxDamage { get; private set; }
        /// <summary>
        /// The source of this power.
        /// </summary>
        public Element Element { get; private set; }
        
        /// <summary>
        /// A random number generator for damage rolls.
        /// </summary>
        private static readonly Random RNG = new Random();
        #endregion

        /// <summary>
        /// Creates a power with given values.
        /// </summary>
        /// <param name="name">The name of the power.</param>
        /// <param name="minDamage">The minimum damage of the power.</param>
        /// <param name="maxDamage">The maximum damage of the power.</param>
        /// <param name="element">The element of the power.</param>
        public Power(string name, int minDamage, int maxDamage, Element element) {
            this.Name = name;
            this.MinDamage = minDamage;
            this.MaxDamage = maxDamage;
            this.Element = element;
        }

        /// <summary>
        /// Default constructor for the purposes of serialization.
        /// </summary>
        private Power() { }

        /// <summary>
        /// Uses the power on the given target, calculating the modified damage
        /// based on the results of Element comparisons.
        /// </summary>
        /// <param name="attacker">The attacking creature.</param>
        /// <param name="target">The target creature.</param>
        /// <returns>The damage report for the power usage.</returns>
        public DamageReport Use(Creature attacker, Creature target) {
            float baseDamage = this.MinDamage + (this.MaxDamage - this.MinDamage) * (float)Power.RNG.NextDouble();
            float multiplier = this.Resolve(target.Prototype.Element);
            int damage = (int)(baseDamage * multiplier);

            target.AlterHealth(-damage);

            return new DamageReport(attacker, target, this, damage, multiplier);
        }

        /// <summary>
        /// Resolves multipliers resulting from the comparison between different
        /// Powers.
        /// </summary>
        /// <param name="other">The element of the other object.</param>
        /// <returns>A multiplier for modifying damage.</returns>
        private float Resolve(Element other) {
            //TODO: Decide whether this table gets splatted right here, or if it should be somehow serialized.
            float multiplier = 1;
            switch (this.Element) {
                case Element.Acid:
                    if (other == Element.Water) multiplier = 2;
                    if (other == Element.Undeath) multiplier = 2;
                    if (other == Element.Ice) multiplier = 0;
                    if (other == Element.Earth) multiplier = 0;
                    break;
                case Element.Earth:
                    if (other == Element.Acid) multiplier = 2;
                    if (other == Element.Lightning) multiplier = 2;
                    if (other == Element.Fire) multiplier = 0;
                    if (other == Element.Force) multiplier = 0;
                    break;
                case Element.Fire:
                    if (other == Element.Ice) multiplier = 2;
                    if (other == Element.Earth) multiplier = 2;
                    if (other == Element.Water) multiplier = 0;
                    if (other == Element.Undeath) multiplier = 0;
                    break;
                case Element.Force:
                    if (other == Element.Ice) multiplier = 2;
                    if (other == Element.Earth) multiplier = 2;
                    if (other == Element.Water) multiplier = 0;
                    if (other == Element.Undeath) multiplier = 0;
                    break;
                case Element.Ice:
                    if (other == Element.Acid) multiplier = 2;
                    if (other == Element.Lightning) multiplier = 2;
                    if (other == Element.Fire) multiplier = 0;
                    if (other == Element.Force) multiplier = 0;
                    break;
                case Element.Lightning:
                    if (other == Element.Water) multiplier = 2;
                    if (other == Element.Undeath) multiplier = 2;
                    if (other == Element.Ice) multiplier = 0;
                    if (other == Element.Earth) multiplier = 0;
                    break;
                case Element.Undeath:
                    if (other == Element.Fire) multiplier = 2;
                    if (other == Element.Force) multiplier = 2;
                    if (other == Element.Acid) multiplier = 0;
                    if (other == Element.Lightning) multiplier = 0;
                    break;
                case Element.Water:
                    if (other == Element.Fire) multiplier = 2;
                    if (other == Element.Force) multiplier = 2;
                    if (other == Element.Acid) multiplier = 0;
                    if (other == Element.Lightning) multiplier = 0;
                    break;
                default:
                    break;
            }
            return multiplier;
        }
    }
}