using System;
using System.Collections.Generic;
using System.Collections;

namespace StoryEngine.Trace
{
	public class Character
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public List<int> Likes {get; set;}
		public List<int> Hates {get; set;}
		
		public Character ()
		{
			
		}
		
		public Character (int id, string name)
		{
			ID = id;
			Name = name;
		}
	}
}

