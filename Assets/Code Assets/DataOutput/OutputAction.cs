using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataOutput {

    /// <summary>
    /// Class that stores each action event to be recorded
    /// </summary>
    [Serializable]
    public class OutputAction {

        public int playerID { get; set; }       // player who executed the action
        public int turnNum { get; set; }        // turn number in total play trace
        public int battleNum { get; set; }      // current battle in the trace
        public string spell { get; set; }       // name of spell being cast on the enemy
        public string spellType { get; set; }   // type of spell being cast on the enemy
        public double performance { get; set; } // performance of the player on the task
        public string enemy { get; set; }       // name of enemy being attacked
        public string enemyType { get; set; }   // type of enemy being attacked
        public int enemyFigthNum { get; set; }  // number of times fighting this enemy (including this battle)
        public string task { get; set; }        // task currently being performed

        public OutputAction(int playerID, int turnNum, int battleNum, string spell, string spellType, double performance, string enemy, string enemyType, int enemyFightNum, string task) {
            this.playerID = playerID;
            this.turnNum = turnNum;
            this.battleNum = battleNum;
            this.spell = spell;
            this.spellType = spellType;
            this.performance = performance;
            this.enemy = enemy;
            this.enemyType = enemyType;
            this.enemyFigthNum = enemyFigthNum;
            this.task = task;
        }

        public OutputAction(int playerID, int turnNum, int battleNum, string spell, string spellType, double performance, string enemy, string enemyType, int enemyFightNum)
            : this(playerID, turnNum, battleNum, spell, spellType, performance, enemy, enemyType, enemyFightNum, (spell + "-" + enemy + "-" + enemyFightNum + "-" + battleNum)) { }

        // constructor for no spellType or enemyType
        public OutputAction(int playerID, int turnNum, int battleNum, string spell, double performance, string enemy, int enemyFightNum)
            : this(playerID, turnNum, battleNum, spell, "", performance, enemy, "", enemyFightNum, (spell + "-" + enemy + "-" + enemyFightNum + "-" + battleNum)) { }

        public OutputAction() : this(-1, -1, -1, "", "", -1, "", "", -1) { }

        public override string ToString() {
            return playerID + "," + turnNum + "," + battleNum + "," + spell + "," + spellType + "," + performance + "," + enemy + "," + enemyType + "," + enemyFigthNum + "," + task;
        }
    }
}
