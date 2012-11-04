using System.Xml.Serialization;
using UnityEngine;

namespace WorldEngine.Items {
    /// <summary>
    /// Basic datastructure for an item.
    /// </summary>
    public class Item {
        /// <summary>
        /// The name of this item.
        /// </summary>
        /// 
        /// 
        public string Name { 
			get {return this.name;}
			set { 
				this.name = value;
//				if (this.Owner != null)
//					this.Owner.name = this.name; 
			}
		}
		
		private string name;
        /// <summary>
        /// A description of the item.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The art asset used to represent this item.
        /// </summary>
        [XmlIgnore]
        public Texture2D Icon { get; set; }
        /// <summary>
        /// The current game object holding the item.
        /// </summary>
        [XmlIgnore]
        public GameObject Owner { get; set; }

        //public static int 

        /// <summary>
        /// Creates a new item with the given fields.
        /// </summary>
        /// <param name="name">The name of the item.</param>
        /// <param name="description">A description of the item.</param>
        public Item(string name, string description) {
            this.Name = name;
            this.Description = description;
        }
        /// <summary>
        /// Private constructor for the purposes of XML serialization.
        /// </summary>
        private Item() { }
    }
}