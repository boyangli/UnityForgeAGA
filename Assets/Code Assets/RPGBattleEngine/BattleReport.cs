using System.Collections.Generic;

using BattleEngine.Creatures;
using BattleEngine.Powers;

namespace BattleEngine {
    /// <summary>
    /// Stores data collected during the course of a battle.
    /// </summary>
    public class BattleReport {
        /// <summary>
        /// The collection of all damage reports generated during a battle.
        /// </summary>
        public List<DamageReport> DamageReports { get; private set; }
        /// <summary>
        /// The results from the after-battle questionairre.
        /// </summary>
        public int QuestionairreData = 0;

        /// <summary>
        /// Creates an empty BattleReport.
        /// </summary>
        public BattleReport() {
            this.DamageReports = new List<DamageReport>();
        }
    }
}