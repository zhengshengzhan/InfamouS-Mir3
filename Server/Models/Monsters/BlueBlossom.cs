﻿using System;
using System.Collections.Generic;
using System.Linq;
using Library;
using Server.Envir;
using S = Library.Network.ServerPackets;

namespace Server.Models.Monsters
{
    public class BlueBlossom : MonsterObject
    {
        protected override bool InAttackRange()
        {
            if (Target.CurrentMap != CurrentMap) return false;
            if (Target.CurrentLocation == CurrentLocation) return false;

            return Functions.InRange(CurrentLocation, Target.CurrentLocation, 8);
        }

        public override bool ShouldAttackTarget(MapObject ob)
        {
            return CanAttackTarget(ob);
        }
        public override bool CanAttackTarget(MapObject ob)
        {
            return CanHelpTarget(ob);
        }
        public override bool CanHelpTarget(MapObject ob)
        {
            return base.CanHelpTarget(ob) && ob.CurrentHP < ob.MaximumHP && ob.Buffs.All(x => x.Type != BuffType.Heal);
        }

        public override void ProcessAction(DelayedAction action)
        {
            switch (action.Type)
            {
                case ActionType.DelayAttack:
                    Heal((MapObject)action.Data[0]);
                    return;
            }

            base.ProcessAction(action);
        }

        public override void ProcessSearch()
        {
            ProperSearch();
        }

        public void Heal(MapObject ob)
        {
            if (ob?.Node == null || ob.Dead) return;

            ob.BuffAdd(BuffType.Heal, TimeSpan.MaxValue, new Stats { [Stat.Healing] = Stats[Stat.Healing], [Stat.HealingCap] = Stats[Stat.HealingCap] }, false, false, TimeSpan.FromSeconds(1));
        }

        protected override void Attack()
        {
            Direction = Functions.DirectionFromPoint(CurrentLocation, Target.CurrentLocation);
            Broadcast(new S.ObjectRangeAttack { ObjectID = ObjectID, Direction = Direction, Location = CurrentLocation, Targets = new List<uint> { Target.ObjectID } });

            UpdateAttackTime();

            ActionList.Add(new DelayedAction(
                               SEnvir.Now.AddMilliseconds(400 + Functions.Distance(CurrentLocation, Target.CurrentLocation) * Globals.ProjectileSpeed),
                               ActionType.DelayAttack,
                               Target));
        }
    }
}
