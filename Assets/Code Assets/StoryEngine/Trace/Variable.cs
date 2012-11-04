namespace StoryEngine.Trace {
    /// <summary>
    /// Storage for a single variable contained within StoryEvents.
    /// </summary>
    public class Variable {
        /// <summary>
        /// The name of this variable.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The value of this variable.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Private constructor for the purposes of serialization.
        /// </summary>
        private Variable() { }
		
		public Variable(string name, string val) {
			Name = name;
			Value = val;
		}
    }
}
