using UnityEngine;
using BattleEngine.Creatures;

namespace BattleEngine.Powers {
    public class DamageReport {

        public int Damage { get; set; }

        public float Multiplier { get; set; }

        public Creature Attacker { get; set; }

        public Creature Defender { get; set; }

        public Power Power { get; set; }

        public DamageReport(Creature attacker, Creature defender, Power power, int damage, float multiplier) {
            this.Attacker = attacker;
            this.Defender = defender;
            this.Power = power;
            this.Damage = damage;
            this.Multiplier = multiplier;
        }

        public void Log() {
            Debug.Log(this.ToString());
        }

        public override string ToString() {
            string s = "";
            s += this.Attacker.Name + " used " + this.Power.Name + " on " + this.Defender.Name;
            s += " for " + this.Damage + " damage.";

            if (this.Multiplier > 1) {
                s += " (Super-Effective)";
            }
            if (this.Multiplier < 1) {
                s += " (Ineffective)";
            }

            return s;
        }
    }
}