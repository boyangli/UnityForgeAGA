using System.Collections.Generic;
using System.Xml.Serialization;

namespace StoryEngine.Trace {
    /// <summary>
    /// Class representing an instantiated instance of a predicate.
    /// </summary>
    public class Proposition {
        /// <summary>
        /// The list of variables for this proposition.
        /// </summary>
        public List<Variable> Variables { get; set; }
        /// <summary>
        /// The indexed variables contained in this event.
        /// </summary>
        [XmlIgnore]
        public Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Private constructor for the purposes of deserialization.
        /// </summary>
        private Proposition() { }

        /// <summary>
        /// A post processing step that converts the variable list into an 
        /// easier to index Dictionary.
        /// </summary>
        internal void MapVariables() {
            this.Properties = new Dictionary<string, string>();

            foreach (Variable v in this.Variables) {
                string name = v.Name;
                string value = v.Value;

                //Because the Planning Community is a bunch of twats, I have to do this.
                string[] words = value.Split('_');
                value = words[0];
                for (int i = 1; i < words.Length; i++) {
                    value += " " + words[i];
                }

                this.Properties.Add(name, value);
            }
        }

        /// <summary>
        /// Wrapper for accessing the Properties dictionary.
        /// </summary>
        /// <param name="variable">The variable to fetch.</param>
        /// <returns>The value of the variable or null.</returns>
        public string GetValue(string variable) {
            try {
                return this.Properties[variable];
            } catch (KeyNotFoundException) {
                return null;
            }
        }
    }
}