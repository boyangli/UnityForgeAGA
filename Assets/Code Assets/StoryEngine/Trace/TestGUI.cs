using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using StoryEngine.Trace;
using System.Xml.Serialization;

public class TestGUI : MonoBehaviour
{
	bool done = false;
	public TestGUI ()
	{
	}
	
	private void drawSingle (int windowID)
	{		
		done = GUILayout.Button ("Test Serialization");
		
		
		if (done) {
			execute();
		}
	}
	
	private void drawFinish (int windowID)
	{		
		GUILayout.Button ("Finish");
		
	}
	
	public void OnGUI ()
	{
		Console.Out.WriteLine("23edc");
		Rect bounds = new Rect (10,10,300,300);
		
		if (!done)
			GUI.Window(0, bounds, this.drawSingle, "DDDD");
		//GUI.Window (0, bounds, this.drawSingle, "", GUI.skin.window);
	}
	
	private void execute()
	{
		//TwitterData td = TwitterData.Deserialize(@"./twitter1.xml");
		TwitterData td = buildData();
		
		XmlSerializer serializer = new XmlSerializer(typeof(TwitterData));
		TextWriter textWriter = new StreamWriter(@"./twitter2.xml");
		serializer.Serialize(textWriter, td);
		textWriter.Close();

		Rect bounds = new Rect (10,10,300,300);
		GUI.Window(0, bounds, this.drawFinish, "DDDD");
	}
	
	private TwitterData buildData()
	{
		Character c1 = new Character(1, "Mister Fantastic");
		Character c2 = new Character(2, "Invisible Woman");
		Character c3 = new Character(3, "The Human Torch");
		Character c4 = new Character(4, "The Thing");
		
		Character c5 = new Character(5, "Annihilus");
		Character c6 = new Character(6, "Doctor Doom");
		Character c7 = new Character(7, "Mad Thinker");
		Character c8 = new Character(8, "Terminus");
		
		List<Character> l1 = new List<Character>();
		l1.Add(c1);
		l1.Add(c2);
		l1.Add(c3);
		l1.Add(c4);
		l1.Add(c5);
		l1.Add(c6);
		l1.Add(c7);
		l1.Add(c8);
		
		Relationship r1 = new Relationship(1, 2);
		Relationship r2 = new Relationship(1, 3);
		Relationship r3 = new Relationship(1, 4);
		Relationship r4 = new Relationship(7, 5);
		Relationship r5 = new Relationship(7, 6);
		Relationship r6 = new Relationship(7, 8);
		
		List<Relationship> l2 = new List<Relationship>();
		l2.Add(r1);
		l2.Add(r2);
		l2.Add(r3);
		l2.Add(r4);
		l2.Add(r5);
		l2.Add(r6);
		
		PropObject o1 = new PropObject(1, "Chocolate");
		PropObject o2 = new PropObject(2, "Gun");		
		PropObject o3 = new PropObject(2, "Porsche");
		List<PropObject> lp = new List<PropObject>();
		lp.Add(o1);
		lp.Add(o2);
		lp.Add(o3);
			
		c2.Likes = new List<int>();						
		c2.Likes.Add(o1.ID);
		
		c3.Likes = new List<int>();						
		c3.Likes.Add(o2.ID);
		
		c4.Likes = new List<int>();						
		c4.Likes.Add(o3.ID);
		
		
		TwitterData td = new TwitterData();
		td.Characters = l1;
		td.Relationships = l2;
		td.PropObjects = lp;
		td.Villain = 7;
		td.Sidekick = 1;
		
		return td;
	}
}


