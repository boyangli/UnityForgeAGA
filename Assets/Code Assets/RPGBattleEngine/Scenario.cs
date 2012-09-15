using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using BattleEngine.Creatures;

namespace BattleEngine
{
    /// <summary>
    /// A collection of sequential battles.
    /// </summary>
	public class Scenario
	{
        /// <summary>
        /// The sequential list of battles.
        /// </summary>
        public List<Battle> Battles { get; private set; }

        /// <summary>
        /// Creates a new scenario containing no battles.
        /// </summary>
        public Scenario() {
            this.Battles = new List<Battle>();
        }

        /// <summary>
        /// Serializes the scenario to an XML string.
        /// </summary>
        /// <returns>A XML data representation of the scenario.</returns>
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

        /// <summary>
        /// Deserializes a scenario from an XML string.
        /// </summary>
        /// <param name="xml">The XML content.</param>
        /// <param name="powerBook">The Bestiary containing the battles' creatures.</param>
        /// <returns>The scenario from the string data.</returns>
        public static Scenario Deserialize(string xml, Bestiary bestiary) {
            XmlSerializer serializer = new XmlSerializer(typeof(Scenario));

            StringReader reader = new StringReader(xml);
            Scenario scen = (Scenario)serializer.Deserialize(reader);

            Debug.Log("Loaded " + scen.Battles.Count + " battles into Scenario.");

            //Initialize the creature powers.
            int referenced = 0;
            foreach (Battle battle in scen.Battles) {
                referenced += battle.LoadCreaturesFromNames(bestiary);
            }
            Debug.Log("Successfully referenced " + referenced + " battle creatures.");

            return scen;
        }
	}
}
