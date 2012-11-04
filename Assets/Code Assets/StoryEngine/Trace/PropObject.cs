using System;

namespace StoryEngine.Trace
{
	public class PropObject
	{
		public int ID { get; set; }
		public string Name { get; set; }
		
		public PropObject ()
		{
		}
		
		public PropObject (int id, string name)
		{
			ID = id;
			Name = name;
		}
	}
}

