namespace StoryEngine {
    /// <summary>
    /// Simplistic Tuple for storing a speaker and their dialogue.  Too bad the
    /// Mono distribution doesn't include generic Tuples.
    /// </summary>
    public class Dialogue {
        /// <summary>
        /// The speaker uttering the dialogue.
        /// </summary>
        public CharacterScript Speaker { get; private set; }
        /// <summary>
        /// The specifc line being uttered.
        /// </summary>
        public string Line { get; private set; }
        /// <summary>
        /// Indicates that this dialogue has already been spoken ('executed').
        /// Used to coordinate task-flow for input and behavior trees;
        /// </summary>
        public bool Spoken { get; set; }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="speaker">The speaker uttering the line.</param>
        /// <param name="line">The specific line being uttered.</param>
        public Dialogue(CharacterScript speaker, string line) {
            this.Speaker = speaker;
            this.Line = line;
            this.Spoken = false;
        }
    }
}