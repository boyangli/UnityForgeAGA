using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StoryEngine.Trace;
using WorldEngine;
using WorldEngine.Items;

namespace StoryEngine {
    /// <summary>
    /// A class containing objective/goal/task (pick one) information for
    /// characters to complete.
    /// </summary>
    public class Task {
        /// <summary>
        /// The type of this task.  This is left as a string for better
        /// integration with RAIN{one} behavior trees.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// The parsed description of this task.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// The character that should be completing the task.
        /// </summary>
        public CharacterScript Actor { get; private set; }
        /// <summary>
        /// A character that the task should be enacted upon, if required.
        /// </summary>
        public CharacterScript Actee { get; private set; }
        /// <summary>
        /// An item involved in this task, if required.
        /// </summary>
        public Item Item { get; private set; }
        /// <summary>
        /// The location at which the task should occur.
        /// </summary>
        public LocaleScript Locale { get; private set; }
        /// <summary>
        /// Dialogue to be displayed prior to undertaking the task.
        /// </summary>
        public Dialogue PreDialogue { get; private set; }
        /// <summary>
        /// Dialogue to be displayed after the task is completed.
        /// </summary>
        public Dialogue PostDialogue { get; private set; }
        /// <summary>
        /// The raw StoryEvent from the deserialized XML.
        /// </summary>
        public StoryEvent StoryEvent { get; private set; }

        /// <summary>
        /// Creates a task datastructure with the given values.
        /// </summary>
        /// <param name="type">The type of this task.</param>
        /// <param name="actor">The active character completing this task.</param>
        /// <param name="actee">The passive character involved in this task.</param>
        /// <param name="item">An item involved in this task.  'The package.'</param>
        /// <param name="locale">The locale at which this task should occur.</param>
        [Obsolete("Use the Task(StoryEvent ev) constructor.")]
        public Task(string type, CharacterScript actor, CharacterScript actee, Item item, LocaleScript locale) {
            this.Type = type;
            this.Actor = actor;
            this.Actee = actee;
            this.Item = item;
            this.Locale = locale;
        }

        /// <summary>
        /// Creates a task datastructure with the given values.
        /// </summary>
        /// <param name="type">The type of this task.</param>
        /// <param name="actor">The active character completing this task.</param>
        /// <param name="actee">The passive character involved in this task.</param>
        /// <param name="locale">The locale at which this task should occur.</param>
        [Obsolete("Use the Task(StoryEvent ev) constructor.")]
        public Task(string type, CharacterScript actor, CharacterScript actee, LocaleScript locale) : this(type, actor, actee, null, locale) { }

        /// <summary>
        /// Creates a task datastructure with the given values.
        /// </summary>
        /// <param name="type">The type of this task.</param>
        /// <param name="actor">The active character completing this task.</param>
        /// <param name="item">An item involved in this task.  'The package.'</param>
        /// <param name="locale">The locale at which this task should occur.</param>
        [Obsolete("Use the Task(StoryEvent ev) constructor.")]
        public Task(string type, CharacterScript actor, Item item, LocaleScript locale) : this(type, actor, null, item, locale) { }

        /// <summary>
        /// Creates a task datastructure with the given values.
        /// </summary>
        /// <param name="type">The type of this task.</param>
        /// <param name="actor">The active character completing this task.</param>
        /// <param name="locale">The locale at which this task should occur.</param>
        [Obsolete("Use the Task(StoryEvent ev) constructor.")]
        public Task(string type, CharacterScript actor, LocaleScript locale) : this(type, actor, null, null, locale) { }

        /// <summary>
        /// Creates a task datastructure from a story event.
        /// </summary>
        /// <param name="ev">The event containing the relevant data.</param>
        public Task(StoryEvent ev) {
            //Save us some typing.
            WorldScript ws = Globals.Instance.WorldScript;

            this.Type = ev.Name;
            this.Actor = ws.GetCharacterByName(ev.GetValue("character1"));
            this.Actee = ws.GetCharacterByName(ev.GetValue("character2"));
            this.Item = ws.GetItemByName(ev.GetValue("item"));
            this.Description = ev.Description;
            
            //Because these aren't consistent.
            if (this.Type == "goto") {
                this.Locale = ws.GetLocaleByName(ev.GetValue("locationTo"));
            } else {
                this.Locale = ws.GetLocaleByName(ev.GetValue("location"));
            }

            this.Description = this.ParseDescription(ev.Description);
            this.PreDialogue = this.ParseDialogue(ev.GetValue("dialogPre"));
            this.PostDialogue = this.ParseDialogue(ev.GetValue("dialogPost"));

            this.StoryEvent = ev;
        }

        private Dialogue ParseDialogue(string rawDialogue) {
            //<Variable>
            //    <Name>dialogPre</Name>
            //    <Value>"{character1}I think I'll should give this {item} to {character2}."</Value>
            //</Variable>
            //<Variable>
            //    <Name>dialogPost</Name>
            //    <Value>"{character1}It's dangerous to go alone! Take this {item}, {character2}!"</Value>
            //</Variable>
			
			//parse out who is doing the speaking, save to theActor field, and rip the template out of the string			
			CharacterScript theActor = this.Actor;
			if( rawDialogue.StartsWith( "{character1}", StringComparison.OrdinalIgnoreCase ) )
			{
				theActor = this.Actor;
				rawDialogue = rawDialogue.Substring( "{character1}".Length );
			}
			else if( rawDialogue.StartsWith( "{character2}", StringComparison.OrdinalIgnoreCase ) )
			{
				theActor = this.Actee;
				rawDialogue = rawDialogue.Substring( "{character2}".Length );
			}
			
			rawDialogue = rawDialogue.Replace( "{character1}", this.Actor.Name );
			if (this.Actee != null) rawDialogue = rawDialogue.Replace( "{character2}", this.Actee.Name );
            if (this.Item != null) rawDialogue = rawDialogue.Replace("{item}", this.Item.Name);
			rawDialogue = rawDialogue.Replace( "{locationTo}", this.Locale.name );
			//workingStr = workingStr.Replace( "{locationFrom}", this.Actor.name );
			rawDialogue = rawDialogue.Replace( "{location}", this.Locale.name );
			
			
            return new Dialogue(theActor, rawDialogue);
        }

        private string ParseDescription(string rawDescription) {
            rawDescription = rawDescription.Replace("{character1}", this.Actor.Name);
            if (this.Actee != null) rawDescription = rawDescription.Replace("{character2}", this.Actee.Name);
            if (this.Item != null) rawDescription = rawDescription.Replace("{item}", this.Item.Name);
            rawDescription = rawDescription.Replace("{locationTo}", this.Locale.name);
            //workingStr = workingStr.Replace( "{locationFrom}", this.Actor.name );
            rawDescription = rawDescription.Replace("{location}", this.Locale.name);


            return rawDescription;
        }
    }
}