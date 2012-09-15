using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataOutput {
    /// <summary>
    /// Class that stores each question response to be recorded
    /// </summary>
    [Serializable]
    public class OutputSurvey {
        public int playerID { get; set; }       // player who made the choice
        public int battleNum { get; set; }      // battle AFTER which player made choice
        public string q1 { get; set; }          // response value for question 1
        public string q2 { get; set; }          // response value for question 2

        public OutputSurvey(int playerID, int battleNum, string q1, string q2) {
            this.playerID = playerID;
            this.battleNum = battleNum;
            this.q1 = q1;
            this.q2 = q2;
        }

        public OutputSurvey() : this(-1, -1, "", "") { }

        public override string ToString() {
            return playerID + "," + battleNum + "," + q1 + "," + q2;
        }

    }
}
