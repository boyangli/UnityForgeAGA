using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using BattleEngine.Powers;

namespace BattleEngine.Creatures {
    /// <summary>
    /// A compendium of creatures.
    /// </summary>
    public class Bestiary {
        /// <summary>
        /// The creatures available in this compendium.
        /// </summary>
        public List<CreaturePrototype> Prototypes { get; private set; }

        /// <summary>
        /// Creates an empty Bestiary.
        /// </summary>
        public Bestiary() {
            this.Prototypes = new List<CreaturePrototype>();
        }

        /// <summary>
        /// Creates a Bestiary from a collection.
        /// </summary>
        /// <param name="collection">The compendium to copy.</param>
        public Bestiary(IEnumerable<CreaturePrototype> collection) {
            this.Prototypes = new List<CreaturePrototype>(collection);
        }

        /// <summary>
        /// Looks up the first creature within the compendium referenced by name.
        /// </summary>
        /// <param name="creatureName">The name to look for.</param>
        /// <returns>The creature of the given name, or null.</returns>
        public CreaturePrototype GetCreature(string creatureName) {
            foreach (CreaturePrototype c in this.Prototypes) {
                if (c.Name == creatureName) return c;
            }
            return null;
        }

        /// <summary>
        /// Gets the entire family of creatures within an element.
        /// </summary>
        /// <param name="element">The element of creatures to collect.</param>
        /// <returns>A list of creatures.</returns>
        public List<CreaturePrototype> GetFamily(Element element) {
            List<CreaturePrototype> family = new List<CreaturePrototype>();

            foreach (CreaturePrototype c in this.Prototypes) {
                if (c.Element == element) family.Add(c);
            }

            return family;
        }

        /// <summary>
        /// Serializes the bestiary to an XML string.
        /// </summary>
        /// <returns>A XML data representation of the bestiary.</returns>
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
        /// Deserializes a bestiary from an XML string.
        /// </summary>
        /// <param name="xml">The XML content.</param>
        /// <param name="powerBook">The PowerBook containing the creatures' powers.</param>
        /// <returns>The bestiary from the string data.</returns>
        public static Bestiary Deserialize(string xml, PowerBook powerBook) {
            XmlSerializer serializer = new XmlSerializer(typeof(Bestiary));

            StringReader reader = new StringReader(xml);
            Bestiary bst =  (Bestiary)serializer.Deserialize(reader);

            Debug.Log("Loaded " + bst.Prototypes.Count + " creatures into Bestiary.");

            //Initialize the creature powers.
            int referenced = 0;
            foreach (CreaturePrototype c in bst.Prototypes) {
                referenced += c.LoadPowersFromNames(powerBook);
            }
            Debug.Log("Successfully referenced " + referenced + " creature powers.");

            return bst;
        }
    }
}