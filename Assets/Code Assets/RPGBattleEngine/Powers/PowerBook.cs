using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BattleEngine.Powers {
    /// <summary>
    /// A compendium of powers.
    /// </summary>
    public class PowerBook {

        /// <summary>
        /// The powers available in this compendium.
        /// </summary>
        public List<Power> Powers { get; private set; }

        /// <summary>
        /// Creates an empty PowerBook.
        /// </summary>
        public PowerBook() {
            this.Powers = new List<Power>();
        }

        /// <summary>
        /// Creates a PowerBook from a collection.
        /// </summary>
        /// <param name="collection">The compendium to copy.</param>
        public PowerBook(IEnumerable<Power> collection) {
            this.Powers = new List<Power>(collection);
        }

        /// <summary>
        /// Looks up the first power within the compendium referenced by name.
        /// </summary>
        /// <param name="powerName">The name to look for.</param>
        /// <returns>The power of the given name, or null.</returns>
        public Power GetPower(string powerName) {
            foreach (Power p in this.Powers) {
                if (p.Name == powerName) return p;
            }
            return null;
        }

        /// <summary>
        /// Gets the entire school of powers within an element.
        /// </summary>
        /// <param name="element">The element of powers to collect.</param>
        /// <returns>A list of powers.</returns>
        public List<Power> GetSchool(Element element) {
            List<Power> school = new List<Power>();

            foreach (Power p in this.Powers) {
                if (p.Element == element) school.Add(p);
            }

            return school;
        }

        /// <summary>
        /// Serializes the powerbook to an XML string.
        /// </summary>
        /// <returns>A XML data representation of the powerbook.</returns>
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
        /// Deserializes a powerbook from an XML string.
        /// </summary>
        /// <param name="xml">The XML content.</param>
        /// <returns>The powerbook from the string data.</returns>
        public static PowerBook Deserialize(string xml) {
            XmlSerializer serializer = new XmlSerializer(typeof(PowerBook));

            StringReader reader = new StringReader(xml);
            PowerBook pb = (PowerBook)serializer.Deserialize(reader);

            Debug.Log("Loaded " + pb.Powers.Count + " powers into PowerBook.");
            
            return pb;
        }
    }
}