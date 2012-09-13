using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskNode{
	
	List<TaskNode> children {get; set;}
	List<TaskNode> parents {get; set;}
	
	public bool done {get; set;}
}
