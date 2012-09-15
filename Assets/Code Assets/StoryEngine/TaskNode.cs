using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StoryEngine {
	public class TaskNode{
		
		public List<TaskNode> children { get; set; }
		public List<TaskNode> parents { get; set; }
		public Task data { get; set; }
		
		public bool done { get; set; }
		
	
		public TaskNode(Task task)
		{
			this.data = task;
			done = false;
			children = new List<TaskNode>();
			parents = new List<TaskNode>();
		}
	}
}
