﻿using System;
using System.Collections.Generic;
using System.Linq;
using AIM.Autoplay.Modes;
using AIM.Autoplay.Util.Data;
using AIM.Autoplay.Util.Objects;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Orbwalking = AIM.Autoplay.Util.Orbwalking;

namespace AIM.Autoplay.Behaviors.Strategy
{
    internal class Actions
    {
        /// <summary>
        ///     This behavior action makes the bot collect a health relic
        /// </summary>
        internal BehaviorAction CollectHealthRelic = new BehaviorAction(
            () =>
            {
                if (Heroes.Me.Position != Relics.ClosestRelic().Position)
                {
                    Heroes.Me.IssueOrder(GameObjectOrder.MoveTo, Relics.ClosestRelic().Position);
                    Base.OrbW.SetAttack(false);
                    Base.OrbW.SetMovement(false);
                    return BehaviorState.Running;
                }
                Base.OrbW.SetAttack(true);
                Base.OrbW.SetMovement(true);
                Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                return BehaviorState.Success;
            });

        /// <summary>
        ///     This BehaviorAction will make the bot go all in for a kill, l0l bronze bot
        /// </summary>
        internal BehaviorAction KillEnemy = new BehaviorAction(
            () =>
            {
                var spells = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E };
                var heroes = new Heroes();
                var killableEnemy =
                    heroes.EnemyHeroes.FirstOrDefault(
                        h => h.Health < Heroes.Me.GetComboDamage(h, spells) + Heroes.Me.GetAutoAttackDamage(Heroes.Me));
                if (killableEnemy == null || killableEnemy.IsDead || !killableEnemy.IsValidTarget() ||
                    killableEnemy.IsInvulnerable || killableEnemy.UnderTurret(true) || Heroes.Me.IsDead)
                {
                    return BehaviorState.Success;
                }


                Base.OrbW.ForceTarget(killableEnemy);
                Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Combo;
                var orbwalkingPos = new Vector3
                {
                    X =
                        killableEnemy.Position.X +
                        (Heroes.Me.AttackRange - 0.2f * Heroes.Me.AttackRange) * Base.ObjConstants.DefensiveMultiplier,
                    Y =
                        killableEnemy.Position.Y +
                        (Heroes.Me.AttackRange - 0.2f * Heroes.Me.AttackRange) * Base.ObjConstants.DefensiveMultiplier
                };
                Base.OrbW.SetOrbwalkingPoint(orbwalkingPos);
                Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                return BehaviorState.Success;
            });

        /// <summary>
        ///     This Behavior action makes the bot walk to the farthest turret and orbwalk there spurdo
        /// </summary>
        internal BehaviorAction ProtectFarthestTurret = new BehaviorAction(
            () =>
            {
                var farthestTurret = Turrets.AllyTurrets.OrderByDescending(t => t.Distance(HQ.AllyHQ)).FirstOrDefault();
                var objConstants = new Constants();
                var orbwalkingPos = new Vector2();
                if (farthestTurret != null)
                {
                    orbwalkingPos.X = farthestTurret.Position.X + (objConstants.DefensiveAdditioner / 8f) +
                                      Randoms.Rand.Next(-100, 100);
                    orbwalkingPos.Y = farthestTurret.Position.Y + (objConstants.DefensiveAdditioner / 8f) +
                                      Randoms.Rand.Next(-100, 100);
                }
                else
                {
                    orbwalkingPos.X = HQ.AllyHQ.Position.X + (objConstants.DefensiveAdditioner / 8f) +
                                      Randoms.Rand.Next(-100, 100);
                    orbwalkingPos.Y = HQ.AllyHQ.Position.Y + (objConstants.DefensiveAdditioner / 8f) +
                                      Randoms.Rand.Next(-100, 100);
                }

                Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D());
                Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
                Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                return BehaviorState.Success;
            });

        /// <summary>
        ///     This Behavior Action will make the bot go all in without any consideration just to push the lane.
        /// </summary>
        internal BehaviorAction PushLane = new BehaviorAction(
            () =>
            {
                try
                {
                    var objConstants = new Constants();
                    var isInDanger = ObjectHandler.Player.UnderTurret(true) && Base.InDangerUnderEnemyTurret();
                    if (Heroes.Me.UnderTurret(true))
                    {
                        var turret = Turrets.EnemyTurrets.OrderBy(t => t.Distance(Heroes.Me)).FirstOrDefault();
                        Base.OrbW.ForceTarget(turret);
                    }
                    if (isInDanger)
                    {
                        var orbwalkingPos = new Vector2
                        {
                            X = ObjectHandler.Player.Position.X + (objConstants.DefensiveAdditioner),
                            Y = ObjectHandler.Player.Position.Y + (objConstants.DefensiveAdditioner)
                        };
                        ObjectHandler.Player.IssueOrder(GameObjectOrder.MoveTo, orbwalkingPos.To3D());
                        Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.None;
                        Base.OrbW.SetAttack(false);
                        Base.OrbW.SetMovement(false);
                        return BehaviorState.Success;
                    }
                    if (Base.LeadingMinion != null)
                    {
                        var orbwalkingPos = new Vector2
                        {
                            X =
                                Base.LeadingMinion.Position.X + (objConstants.DefensiveAdditioner / 8f) +
                                Randoms.Rand.Next(-100, 100),
                            Y =
                                Base.LeadingMinion.Position.Y + (objConstants.DefensiveAdditioner / 8f) +
                                Randoms.Rand.Next(-100, 100)
                        };
                        Utility.DelayAction.Add(
                            new Random(Environment.TickCount).Next(500, 1500),
                            () => Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D()));
                        Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
                        Base.OrbW.SetAttack(true);
                        Base.OrbW.SetMovement(true);
                        Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                        return BehaviorState.Success;
                    }
                    return BehaviorState.Failure;
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                }
                return BehaviorState.Failure;
            });

        /// <summary>
        ///     This Behavior Action will make the bot stay in the safe exp zone
        /// </summary>
        internal BehaviorAction StayWithinExpRange = new BehaviorAction(
            () =>
            {
                var objConstants = new Constants();
                var isInDanger = ObjectHandler.Player.UnderTurret(true) && Base.InDangerUnderEnemyTurret();
                if (Heroes.Me.UnderTurret(true))
                {
                    var turret = Turrets.EnemyTurrets.OrderBy(t => t.Distance(Heroes.Me)).FirstOrDefault();
                    Base.OrbW.ForceTarget(turret);
                }
                if (isInDanger)
                {
                    var orbwalkingPos = new Vector2
                    {
                        X = ObjectHandler.Player.ServerPosition.X + objConstants.DefensiveAdditioner,
                        Y = ObjectHandler.Player.ServerPosition.Y + objConstants.DefensiveAdditioner
                    };
                    ObjectHandler.Player.IssueOrder(GameObjectOrder.MoveTo, orbwalkingPos.To3D());
                    Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.None;
                    Base.OrbW.SetAttack(false);
                    Base.OrbW.SetMovement(false);
                    return BehaviorState.Success;
                }

                if (Base.ClosestEnemyMinion != null)
                {
                    var orbwalkingPos = new Vector2
                    {
                        X =
                            Base.ClosestEnemyMinion.Position.X + objConstants.DefensiveAdditioner +
                            Randoms.Rand.Next(-150, 150),
                        Y =
                            Base.ClosestEnemyMinion.Position.Y + objConstants.DefensiveAdditioner +
                            Randoms.Rand.Next(-150, 150)
                    };
                    Utility.DelayAction.Add(
                        new Random(Environment.TickCount).Next(500, 1500),
                        () => Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D()));
                    Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
                    Base.OrbW.SetAttack(true);
                    Base.OrbW.SetMovement(true);
                    Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                    return BehaviorState.Success;
                }
                return BehaviorState.Success;
            });

        /// <summary>
        ///     This is the Teamfight Behavior, pretty self explainatory
        /// </summary>
        internal BehaviorAction Teamfight = new BehaviorAction(
            () =>
            {
                var orbwalkingPos = Positioning.Teamfight.GetPos();
                Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D());
                Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
                Orbwalking.SetMovementDelay(Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
                return BehaviorState.Success;
            });
    }
}