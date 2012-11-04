
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using StoryEngine;
using WorldEngine;
using WorldEngine.Items;


namespace StoryEngine.Trace
{
	public class TwitterData
	{
		public TwitterData ()
		{
		}
		
		public List<Character> Characters { get; set; }
		public List<Relationship> Relationships { get; set;}
		public List<PropObject> PropObjects { get; set;}
		public int Villain { get; set;}
		public int Sidekick { get; set;}
		
		public List<Character> goodFriends { get; set; }
		public List<Character> badFriends { get; set; }
		private Character VillainChar;
		private Character SidekickChar;
		
		public static TwitterData Deserialize(string xml) {
			
			XmlSerializer deserializer = new XmlSerializer(typeof(TwitterData));
			
			XmlReader reader = XmlReader.Create(new StringReader(xml));

			TwitterData td = (TwitterData)deserializer.Deserialize(reader);
			reader.Close();
			
			foreach(Relationship r in td.Relationships)
			{
				if (r.person1 == td.Villain)
				{
					foreach(Character cc in td.Characters)
					{
						if (cc.ID == r.person2)
						{
							td.badFriends.Add(cc);
							break;
						}
					}					
				}
				else if (r.person2 == td.Villain)
				{
					foreach(Character cc in td.Characters)
					{
						if (cc.ID == r.person1)
						{
							td.badFriends.Add(cc);
							break;
						}
					}
				}
				else if (r.person1 == td.Sidekick)
				{
					foreach(Character cc in td.Characters)
					{
						if (cc.ID == r.person2)
						{
							td.goodFriends.Add(cc);
							break;
						}
					}
				}
				else if (r.person2 == td.Sidekick)
				{
					foreach(Character cc in td.Characters)
					{
						if (cc.ID == r.person1)
						{
							td.goodFriends.Add(cc);
							break;
						}
					}
				}
			}
			
			foreach(Character c in td.Characters)
			{
				if (c.ID == td.Villain)
				{
					td.VillainChar = c;
				}
				else if (c.ID == td.Sidekick)
				{
					td.SidekickChar = c;
				}
			}
			
			return td;
		}
		
		// load the data into the game engine
		public void loadIntoEngine()
		{
			// rand = new UnityEngine.Random();
			WorldScript world = Globals.Instance.WorldScript;
			
			List<CharacterScript> chars = new List<CharacterScript>();
			
			foreach(CharacterScript cs in world.NPCs)
			{
				if (cs.Name != "Player")
					chars.Add(cs);	
			}
			
			Dictionary<CharacterScript, string> charLocales = new Dictionary<CharacterScript, string>();
			List<LocaleScript> locales = world.Locales;
			
			foreach (Character c in this.Characters)
			{
				// pick a random character
				int length = chars.Count;
				int idx = UnityEngine.Random.Range(0, length);
				
				CharacterScript cs = chars[idx];
				chars.Remove(cs);
				cs.Name = c.Name;
				Debug.Log("selected index = " + idx + " = " + c.Name);
				// pick a random location
				length = locales.Count;
				idx = UnityEngine.Random.Range(0, length);
				LocaleScript ls = locales[idx];
				charLocales.Add(cs, ls.name);
				cs.Location = ls.Location;
			}
			
			// find object needed
			List<PropObject> desirables = new List<PropObject>();
			Dictionary<string, string> likeDictionary = new Dictionary<string, string>();
			foreach(Character gc in this.goodFriends)
			{
				foreach(PropObject po in PropObjects)
				{
					if (po.ID == gc.Likes[0])
					{
						desirables.Add(po);
						likeDictionary.Add(po.Name, gc.Name);
					}
				}				
			}
			
			List<Character> bads = new List<Character>();
			bads.AddRange(badFriends);
			//bads.Add(this.Villain);
			
			List<Item> items = new List<Item>();
			foreach (Item i in world.Items)
			{
				if (!i.Name.Equals("Sword"))
					items.Add(i);
				else
				{
					// ** give the player the sword **
					world.PickupItem(world.GetCharacterByName("player"), i);
					Debug.Log("found sword");
				}
			}
			
			Dictionary<CharacterScript, string> ownership = new Dictionary<CharacterScript, string>();
			
			foreach(PropObject d in desirables)
			{							
				// take a random item and name it correctly
				
				int length = items.Count;
				int idx = UnityEngine.Random.Range(0, length);
				Item item = items[idx];
				items.Remove(item);
				item.Name = d.Name;
				item.Description = "This is random item obtained from a planet called Twitter. Seriously, who calls a planet Twitter?";
				
				// ** put the item into possession of a random bad friend **
								
				// find the random bad friend
				length = bads.Count;
				idx = UnityEngine.Random.Range(0, length);
				string badName = bads[idx].Name;
				bads.RemoveAt(idx);
				// find the npc 
				CharacterScript cs = world.GetCharacterByName(badName);
				if (cs == null)
					Debug.Log("cannot find character " + badName);
				else
				{
					bool result = world.PickupItem (cs, item);
					if (result)
					{
						Debug.Log("give " + badName + " " + item.Name);
						ownership.Add(cs, item.Name);
					}
					else
						Debug.Log("failed to give " + badName + " " + item.Name);			
				}			
				
			}
			
			Episode episode = generateEpisode(ownership, charLocales, likeDictionary);	
			Globals.Instance.DMScript.DramaManager.LoadTasks(episode.Events, episode.Links);
		}
		
		private Episode generateEpisode(Dictionary<CharacterScript, string> ownership, Dictionary<CharacterScript, string> charLocales, 
			Dictionary<string, string> likeDictionary)
		{
			Episode defaultEpi = Episode.Deserialize(File.ReadAllText("./Episodes/TwitterDefault.xml"));
			List<StoryEvent> events = defaultEpi.Events;
			
			WorldScript world = Globals.Instance.WorldScript;
			/** this is the initiating event */
			foreach(StoryEvent e in events)	
			{
				if (e.ID == 0)
				{					
					CharacterScript cs = world.GetCharacterByName(SidekickChar.Name);
					List<Variable> vars = new List<Variable>();
					vars.Add(new Variable("character1", "player"));
					vars.Add(new Variable("character2", SidekickChar.Name));
					vars.Add(new Variable("location", charLocales[cs]));
					vars.Add(new Variable("dialogPre", "{character1} Clouds are gathering. Something is happening. " +
						"I should talke to " + SidekickChar.Name + "."));
					vars.Add(new Variable("dialogPost", "{character2} The world is in danger. I need to assemble my team. " +
						"But they would listen until you give each of them something they like. You must act quickly!"));
					
					e.Variables = vars;
					e.MapVariables();
				}
			}

				
			/*** these are the killing and looting events ***/
			int i = 1;
			foreach(CharacterScript cs in ownership.Keys)
			{
				foreach(StoryEvent e in events)	
				{
					string item = ownership[cs];
					string location = charLocales[cs];
					if (e.ID == i)
					{
						
						List<Variable> vars = new List<Variable>();
						vars.Add(new Variable("character1", "player"));
						vars.Add(new Variable("character2", cs.Name));
						vars.Add(new Variable("location", location));
						vars.Add(new Variable("item", "Sword"));
						vars.Add(new Variable("character2_health", "healthy"));
						vars.Add(new Variable("dialogPre", "{character1} I must kill the {character2} " +
							"in order to obtain the " + item + "! Maybe I should search the " + location));
						vars.Add(new Variable("dialogPost", "{character2} You think you win, but " + VillainChar.Name +
							" will kill you! AHHhh..."));
						
						e.Variables = vars;
						e.MapVariables();
					}
					else if (e.ID == i + 1)
					{
						List<Variable> vars = new List<Variable>();
						vars.Add(new Variable("character1", "player"));
						vars.Add(new Variable("character2", cs.Name));
						vars.Add(new Variable("location", location));
						vars.Add(new Variable("item", item));
						vars.Add(new Variable("dialogPre", "{character1} I must search the body of {character2} " +
							"to find the " + item + "."));
						vars.Add(new Variable("dialogPost", "{character1} Yes! I found it!"));
						
						e.Variables = vars;
						e.MapVariables();
					}
					else if (e.ID == i + 2)
					{
						string goodGuy = likeDictionary[item];
						
						
						List<Variable> vars = new List<Variable>();
						vars.Add(new Variable("character1", "player"));
						vars.Add(new Variable("character2", goodGuy));
						vars.Add(new Variable("location", location));
						vars.Add(new Variable("item", item));
						vars.Add(new Variable("dialogPre", "{character1} Now I should give the " + item + " to " + goodGuy + "."));
						vars.Add(new Variable("dialogPost", "{character2} Thanks for the " + item + "! Now I need to go back to my base."));
						
						e.Variables = vars;
						e.MapVariables();
					}
				}
				i += 3;
			}
			
			/** these are the ending events */
			foreach(StoryEvent e in events)	
			{
				if (e.ID == 10)
				{
					CharacterScript cs = world.GetCharacterByName(SidekickChar.Name);
					List<Variable> vars = new List<Variable>();
					vars.Add(new Variable("character1", "player"));
					vars.Add(new Variable("character2", SidekickChar.Name));
					vars.Add(new Variable("location", charLocales[cs]));
					vars.Add(new Variable("dialogPre", "{character1} " +
						"I should talke to " + SidekickChar.Name + ". He should have assembled his team now."));
					vars.Add(new Variable("dialogPost", "{character2} Thank you for persuading my team members back to work. " +
						"While we fight the evil around the world, you should go directly to kill " + VillainChar.Name));
					
					e.Variables = vars;
					e.MapVariables();
				}
				
				if (e.ID == 11)
				{
					
					CharacterScript cs = world.GetCharacterByName(VillainChar.Name);
					List<Variable> vars = new List<Variable>();
					vars.Add(new Variable("character1", "player"));
					vars.Add(new Variable("character2", VillainChar.Name));
					vars.Add(new Variable("location", charLocales[cs]));
					vars.Add(new Variable("item", "Sword"));
					vars.Add(new Variable("character2_health", "healthy"));
					vars.Add(new Variable("dialogPre", "{character1} " +
						"Why am I always doing the heavylifting? I am going to do this again for the world. Now going to kill " + VillainChar.Name));
					vars.Add(new Variable("dialogPost", "{character2} MY EMPIRE FALLS!!!"));
					
					e.Variables = vars;
					e.MapVariables();
				}
			}
			
			return defaultEpi;
		}
		
	}
}

