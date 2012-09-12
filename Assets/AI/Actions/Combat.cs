using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Belief;
using RAIN.Action;

using BattleEngine;
using BattleEngine.Creatures;
using StoryEngine;
using WorldEngine;

public class Combat : Action
{
	public Combat()
	{
		actionName = "Combat";
	}

	public override ActionResult Start(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}

	public override ActionResult Execute(Agent agent, float deltaTime)
	{
        //Get the worldscript and task.
        WorldScript ws = this.actionContext.GetContextItem<WorldScript>("world");
        Task task = this.actionContext.GetContextItem<CharacterScript>("character").ActiveTask;

        ////Create the new battle.
        //Battle battle = new Battle();
        //battle.AddFriendlyPrototypes(ws.PartyMembers);

        ////Get the enemy creatures.
        //string enemy = null;
        //string end = null;

        ////TODO: Make sure the actee is in the animation files and add him to the battle.
        //enemy = task.StoryEvent.GetValue("enemy1");
        //end = enemy.Remove(0, 1);
        //enemy = char.ToUpper(enemy[0]) + end;
        //battle.HostileTeam.Add(new Creature(Globals.Instance.Bestiary.GetCreature(enemy)));

        ////enemy = task.StoryEvent.GetValue("enemy2");
        ////end = enemy.Remove(0, 1);
        ////enemy = char.ToUpper(enemy[0]) + end;
        ////battle.HostileTeam.Add(new Creature(Globals.Instance.Bestiary.GetCreature(enemy)));

        ////enemy = task.StoryEvent.GetValue("enemy3");
        ////end = enemy.Remove(0, 1);
        ////enemy = char.ToUpper(enemy[0]) + end;
        ////battle.HostileTeam.Add(new Creature(Globals.Instance.Bestiary.GetCreature(enemy)));
        
        ////Register the battle.
        //Globals.Instance.Battle = battle;

        ws.LoadBattle(task);

        //Kill the actee.
        task.Actee.Dead = true;

        return ActionResult.SUCCESS;
	}

	public override ActionResult Stop(Agent agent, float deltaTime)
	{
		return ActionResult.SUCCESS;
	}
}
