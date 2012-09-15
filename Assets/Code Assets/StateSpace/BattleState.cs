using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

using BattleEngine.Creatures;

namespace StateSpace {
    /// <summary>
    /// Datastructure for converting raw battle information into the domain
    /// specific statespace used by the AFABL agents.
    /// </summary>
    public class BattleState {
        /// <summary>
        /// Flag indicating the friendly team won.
        /// </summary>
        public bool FriendliesWon { get; set; }
        /// <summary>
        /// Flag indicating the hostile team won.
        /// </summary>
        public bool HostilesWon { get; set; }
        /// <summary>
        /// The turn order, in simplified creature states.
        /// </summary>
        public List<CreatureState> TurnOrder { get; set; }

        /// <summary>
        /// Private constructor for the purposes of serialization.
        /// </summary>
        private BattleState() { }

        public BattleState(List<Creature> turnOrder) {
            this.TurnOrder = new List<CreatureState>();

            foreach (Creature creature in turnOrder) {
                this.TurnOrder.Add(new CreatureState(creature));
            }
        }

        /// <summary>
        /// Serializes the state to an XML string.
        /// </summary>
        /// <returns></returns>
        public string Serialize() {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\n";
            settings.NewLineOnAttributes = true;
            settings.OmitXmlDeclaration = true;

            StringBuilder sB = new StringBuilder();
            XmlWriter xW = XmlTextWriter.Create(sB, settings);

            XmlSerializer xS = new XmlSerializer(this.GetType());

            xS.Serialize(xW, this);

            return sB.ToString();
        }
    }
}