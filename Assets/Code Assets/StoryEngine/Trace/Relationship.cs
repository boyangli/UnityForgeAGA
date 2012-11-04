using System;

namespace StoryEngine.Trace
{
	public class Relationship
	{
		public int person1 { get; private set; }
		public int person2 { get; private set; }
		
		public Relationship ()
		{
		}
		
		public Relationship (int p1, int p2)
		{
			person1 = p1;
			person2 = p2;
		}
	}
}

