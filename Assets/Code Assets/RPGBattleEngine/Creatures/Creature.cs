using BattleEngine.Powers;

namespace BattleEngine.Creatures {
    /// <summary>
    /// An instantiated CreaturePrototype.
    /// </summary>
    public class Creature {
        /// <summary>
        /// The name of this creatures.  Defaults to the prototype name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The current health of the character, clamped [0, MaxHealth].
        /// </summary>
        public int Health {
            get { return this.health; }
            set {
                this.health = value;
                if (this.health > this.Prototype.MaxHealth) health = this.Prototype.MaxHealth;
                if (this.health < 0) health = 0;
            }
        }
        private int health;
        /// <summary>
        /// Public variable used for marking dead creatures externally.
        /// </summary>
        public bool Dead { get; set; }

        /// <summary>
        /// A simple flag set by battle controlling classes.  This serves as an
        /// easy accessor for this information, rather than searching the contents
        /// of various lists.
        /// </summary>
        public bool AIControlled { get; set; }
        /// <summary>
        /// Public variable for marking friendly / hostile creatures (to the player).
        /// </summary>
        public bool Hostile { get; set; }
        /// <summary>
        /// The prototype this creature instantiates.
        /// </summary>
        public CreaturePrototype Prototype { get; private set; }

        /// <summary>
        /// A random number generator for selecting abilities.
        /// </summary>
        private static readonly System.Random RNG = new System.Random();

        /// <summary>
        /// Creates a new creature from a given prototype.
        /// </summary>
        /// <param name="prototype">The prototype to instantiate.</param>
        public Creature(CreaturePrototype prototype) {
            this.Prototype = prototype;
            this.Health = prototype.MaxHealth;
            this.Name = prototype.Name;
        }

        /// <summary>
        /// Changes the creatures health by the given amount, allowing it to
        /// temporarily store the value for display.
        /// </summary>
        /// <param name="amount">The delta change in creture health to apply.</param>
        public void AlterHealth(int amount) {
            this.Health += amount;
            //TODO: Display the heatlh in the game somewhere.
        }

        /// <summary>
        /// Uses a random power on another creature.
        /// </summary>
        /// <param name="other">The target creature.</param>
        /// <returns>The power's damage report.</returns>
        public DamageReport UseRandomPower(Creature other) {
            //Nothing to do if no powers.
            if (this.Prototype.AtWillPowers.Count == 0) return new DamageReport(this, other, null, 0, 1);

            int index = Creature.RNG.Next(this.Prototype.AtWillPowers.Count);
            return this.Prototype.AtWillPowers[index].Use(this, other);
        }
    }
}
