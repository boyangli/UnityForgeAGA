using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WorldEngine.Items {
    /// <summary>
    /// A compendium of items.
    /// </summary>
    public class Catalog {
        /// <summary>
        /// The items available in this compendium.
        /// </summary>
        public List<Item> Items { get; private set; }

        /// <summary>
        /// Creates an empty Catalog.
        /// </summary>
        public Catalog() {
            this.Items = new List<Item>();
        }

        /// <summary>
        /// Creates a Catalog from a collection.
        /// </summary>
        /// <param name="collection">The compendium to copy.</param>
        public Catalog(IEnumerable<Item> collection) {
            this.Items = new List<Item>(collection);
        }

        /// <summary>
        /// Looks up the first item within the compendium referenced by name.
        /// </summary>
        /// <param name="itemName">The name to look for.</param>
        /// <returns>The item of the given name, or null.</returns>
        public Item GetItem(string itemName) {
            foreach (Item i in this.Items) {
                if (i.Name == itemName) return i;
            }
            return null;
        }

        public Item GetItem(int id) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the Catalog to an XML string.
        /// </summary>
        /// <returns>A XML data representation of the Catalog.</returns>
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
        /// Deserializes a Catalog from an XML string.
        /// </summary>
        /// <param name="xml">The XML content.</param>
        /// <returns>The Catalog from the string data.</returns>
        public static Catalog Deserialize(string xml) {
            XmlSerializer serializer = new XmlSerializer(typeof(Catalog));

            StringReader reader = new StringReader(xml);
            Catalog inv = (Catalog)serializer.Deserialize(reader);

            Debug.Log("Loaded " + inv.Items.Count + " items into Inventory.");

            return inv;
        }
    }
}