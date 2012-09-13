using System;

namespace StoryEngine.Trace
{
	public class Link
	{
		public int Source { get; set;}
		public int Target { get; set;}
		public Link (int s, int t)
		{
			Source = s;
			Target = t;
		}
		
		private Link()
		{
			Source = -1;
			Target = -1;
		}
	}
}

