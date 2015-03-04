using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIM.Autoplay.Util;
using AIM.Autoplay.Util.Data;
using AIM.Autoplay.Util.Objects;
using BehaviorSharp;
using BehaviorSharp.Components.Actions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Orbwalking = AIM.Autoplay.Util.Orbwalking;

namespace AIM.Autoplay.Behaviors.Strategy
{
	internal class Actions
	{
		/// <summary>
		/// This Behavior Action will make the bot go all in without any consideration just to push the lane.
		/// </summary>
        internal BehaviorAction PushLane = new BehaviorAction(
            () =>
												{
													try
													{
														Console.WriteLine("pushlane");
														var objConstants = new Constants();
														var isInDanger = ObjectManager.Player.UnderTurret(true) && Modes.Base.InDangerUnderEnemyTurret();
														if (Heroes.Me.UnderTurret(true))
														{
															var turret = Turrets.EnemyTurrets.OrderBy(t => t.Distance(Heroes.Me)).FirstOrDefault();
															Modes.Base.OrbW.ForceTarget(turret);
														}

														if (isInDanger)
														{
															var orbwalkingPos = new Vector2
															{
																X = ObjectManager.Player.Position.X + (objConstants.DefensiveAdditioner),
																Y = ObjectManager.Player.Position.Y + (objConstants.DefensiveAdditioner)
															};
															ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, orbwalkingPos.To3D());
															Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.None;
															Modes.Base.OrbW.SetAttack(false);
															Modes.Base.OrbW.SetMovement(false);
															return BehaviorState.Success;
														}

														if (Modes.Base.LeadingMinion != null)
														{
															var orbwalkingPos = new Vector2
															{
																X =
                                Modes.Base.LeadingMinion.Position.X + (objConstants.DefensiveAdditioner / 8f) +
                                Randoms.Rand.Next(-100, 100),
                            Y =
                                Modes.Base.LeadingMinion.Position.Y + (objConstants.DefensiveAdditioner / 8f) +
                                Randoms.Rand.Next(-100, 100)
															};
															Utility.DelayAction.Add(new Random(Environment.TickCount).Next(500, 1500), () => Modes.Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D()));
															Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
															Modes.Base.OrbW.SetAttack(true);
															Modes.Base.OrbW.SetMovement(true);
															Orbwalking.SetMovementDelay(Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
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
		/// This Behavior Action will make the bot stay in the safe exp zone
		/// </summary>
        internal BehaviorAction StayWithinExpRange = new BehaviorAction(
            () =>
												{
													Console.WriteLine("stay within range");
													var objConstants = new Constants();
													var isInDanger = ObjectManager.Player.UnderTurret(true) && Modes.Base.InDangerUnderEnemyTurret();
													if (Heroes.Me.UnderTurret(true))
													{
														var turret = Turrets.EnemyTurrets.OrderBy(t => t.Distance(Heroes.Me)).FirstOrDefault();
														Modes.Base.OrbW.ForceTarget(turret);
													}

													if (isInDanger)
													{
														var orbwalkingPos = new Vector2
														{
															X = ObjectManager.Player.ServerPosition.X + objConstants.DefensiveAdditioner,
                        Y = ObjectManager.Player.ServerPosition.Y + objConstants.DefensiveAdditioner
														};
														ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, orbwalkingPos.To3D());
														Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.None;
														Modes.Base.OrbW.SetAttack(false);
														Modes.Base.OrbW.SetMovement(false);
														return BehaviorState.Success;
													}

													if (Modes.Base.ClosestEnemyMinion != null)
													{
														var orbwalkingPos = new Vector2
														{
															X =
                            Modes.Base.ClosestEnemyMinion.Position.X + objConstants.DefensiveAdditioner +
                            Randoms.Rand.Next(-150, 150),
                        Y =
                            Modes.Base.ClosestEnemyMinion.Position.Y + objConstants.DefensiveAdditioner +
                            Randoms.Rand.Next(-150, 150)
														};
														Utility.DelayAction.Add(new Random(Environment.TickCount).Next(500, 1500), () => Modes.Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D()));
														Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Mixed;
														Modes.Base.OrbW.SetAttack(true);
														Modes.Base.OrbW.SetMovement(true);
														Orbwalking.SetMovementDelay(Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
														return BehaviorState.Success;
													}

													return BehaviorState.Success;
												});

		/// <summary>
		/// This BehaviorAction will make the bot go all in for a kill, l0l bronze bot
		/// </summary>
        internal BehaviorAction KillEnemy = new BehaviorAction(
            () =>
												{
													var spells = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E };
													var heroes = new Heroes();
													var killableEnemy = heroes.EnemyHeroes.FirstOrDefault(h => h.Health < Heroes.Me.GetComboDamage(h, spells) + Heroes.Me.GetAutoAttackDamage(Heroes.Me));
													if (killableEnemy == null || killableEnemy.IsDead || !killableEnemy.IsValidTarget() ||
                    killableEnemy.IsInvulnerable || killableEnemy.UnderTurret(true) || Heroes.Me.IsDead)
													{
														return BehaviorState.Success;
													}

													Modes.Base.OrbW.ForceTarget(killableEnemy);
													Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Combo;
													var orbwalkingPos = new Vector3
													{
														X =
                        killableEnemy.Position.X +
                        (Heroes.Me.AttackRange - 0.2f * Heroes.Me.AttackRange) *
                        Modes.Base.ObjConstants.DefensiveMultiplier,
                    Y =
                        killableEnemy.Position.Y +
                        (Heroes.Me.AttackRange - 0.2f * Heroes.Me.AttackRange) *
                        Modes.Base.ObjConstants.DefensiveMultiplier
													};
													Modes.Base.OrbW.SetOrbwalkingPoint(orbwalkingPos);
													Orbwalking.SetMovementDelay(Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
													return BehaviorState.Success;
												});

		/// <summary>
		/// This behavior action makes the bot collect a health relic
		/// </summary>
        internal BehaviorAction CollectHealthRelic = new BehaviorAction(
            () =>
												{
													Console.WriteLine("healthrelic");

													if (Heroes.Me.Position != Relics.ClosestRelic().Position)
													{
														Heroes.Me.IssueOrder(GameObjectOrder.MoveTo, Relics.ClosestRelic().Position);
														Modes.Base.OrbW.SetAttack(false);
														Modes.Base.OrbW.SetMovement(false);
														return BehaviorState.Running;
													}

													Modes.Base.OrbW.SetAttack(true);
													Modes.Base.OrbW.SetMovement(true);
													Orbwalking.SetMovementDelay(Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
													return BehaviorState.Success;
												});

		/// <summary>
		/// This Behavior action makes the bot walk to the farthest turret and orbwalk there spurdo
		/// </summary>
        internal BehaviorAction ProtectFarthestTurret = new BehaviorAction(
            () =>
												{
													Console.WriteLine("ProtectFarthestTurret");

													var farthestTurret = Turrets.AllyTurrets.OrderByDescending(t => t.Distance(HQ.AllyHQ)).FirstOrDefault();
													var objConstants = new Constants();
													var orbwalkingPos = new Vector2();
													if (farthestTurret != null)
													{
														orbwalkingPos.X = farthestTurret.Position.X;
														orbwalkingPos.Y = farthestTurret.Position.Y;
													}
													else
													{
														orbwalkingPos.X = HQ.AllyHQ.Position.X;
														orbwalkingPos.Y = HQ.AllyHQ.Position.Y;
													}

													if (Heroes.Me.Team == GameObjectTeam.Order)
													{
														orbwalkingPos.X = orbwalkingPos.X - Randoms.Rand.Next(-100, 200);
														orbwalkingPos.Y = orbwalkingPos.Y - Randoms.Rand.Next(-100, 200);
													}

													if (Heroes.Me.Team == GameObjectTeam.Chaos)
													{
														//	Game.PrintChat("Team Chaos");
														orbwalkingPos.X = orbwalkingPos.X + Randoms.Rand.Next(-100, 200);
														orbwalkingPos.Y = orbwalkingPos.Y + Randoms.Rand.Next(-100, 200);
													}

													Modes.Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D());
													int mvmtDelay = Randoms.Rand.Next(100, Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
													Orbwalking.SetMovementDelay(mvmtDelay);
													return BehaviorState.Success;
												});

		/// <summary>
		/// This is the Teamfight Behavior, pretty self explainatory
		/// </summary>
        internal BehaviorAction Teamfight = new BehaviorAction(
            () =>
												{
													Console.WriteLine("teamfight");

													var orbwalkingPos = Positioning.Teamfight.GetPos();
													Positioning.Util Utils = new Positioning.Util();

													if (Game.MapId == GameMapId.HowlingAbyss)
													{
														var allyZonePathList = Positioning.Util.AllyZone().OrderBy(p => Randoms.Rand.Next()).FirstOrDefault();
														var allyZoneVectorList = new List<Vector2>();

														//create vectors from points and remove walls
                    foreach (var point in allyZonePathList)
														{
															var v2 = new Vector2(point.X, point.Y);
															if (!v2.IsWall())
															{
																allyZoneVectorList.Add(v2);
															}
														}

														var pointClosestToEnemyHQ =
                        allyZoneVectorList.OrderBy(p => p.Distance(HQ.EnemyHQ.Position)).FirstOrDefault();
														var zz = new Constants();
														//Console.WriteLine(Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
														int minNum = Modes.Base.Menu.Item("MinDist").GetValue<Slider>().Value;
														int maxNum = Modes.Base.Menu.Item("MaxDist").GetValue<Slider>().Value;
														//Console.WriteLine(minNum);
														//Console.WriteLine(maxNum);

														var closestEnemy = Utils.GetEnemyPosList().OrderByDescending(b => b.Distance(HQ.AllyHQ.Position)).FirstOrDefault();
														pointClosestToEnemyHQ = Utils.GetAllyPosList().OrderByDescending(a => a.Distance(HQ.AllyHQ.Position)).FirstOrDefault();
														if (pointClosestToEnemyHQ == null &&closestEnemy.X == null)
														{
															Console.WriteLine("going to middle");
															pointClosestToEnemyHQ.X = 5000;
															pointClosestToEnemyHQ.Y = 5000;
														}
														else if (pointClosestToEnemyHQ.X == null)
														{
															Console.WriteLine("going to enemy");
															pointClosestToEnemyHQ = closestEnemy;
														}

														/*	else if(pointClosestToEnemyHQ!= null &&closestEnemy !=null){
															Console.WriteLine("going to median");
															pointClosestToEnemyHQ.X = (pointClosestToEnemyHQ.X +closestEnemy.X)/2;
															pointClosestToEnemyHQ.Y = (pointClosestToEnemyHQ.Y +closestEnemy.Y)/2;
														}*/

														Console.WriteLine(pointClosestToEnemyHQ.X);
														Console.WriteLine(pointClosestToEnemyHQ.Y);

														if (Heroes.Me.Team == GameObjectTeam.Order)
														{
															pointClosestToEnemyHQ.X = pointClosestToEnemyHQ.X - Randoms.Rand.Next(minNum, maxNum);
															pointClosestToEnemyHQ.Y = pointClosestToEnemyHQ.Y - Randoms.Rand.Next(minNum, maxNum);
														}

														if (Heroes.Me.Team == GameObjectTeam.Chaos)
														{
															pointClosestToEnemyHQ.X = pointClosestToEnemyHQ.X + Randoms.Rand.Next(minNum, maxNum);
															pointClosestToEnemyHQ.Y = pointClosestToEnemyHQ.Y + Randoms.Rand.Next(minNum, maxNum);
														}

														orbwalkingPos = pointClosestToEnemyHQ;
													}

													Modes.Base.OrbW.SetOrbwalkingPoint(orbwalkingPos.To3D());
													Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.LaneClear;
													Modes.Base.OrbW.ActiveMode = Orbwalking.OrbwalkingMode.Combo;
													int mvmtDelay = Randoms.Rand.Next(100, Modes.Base.Menu.Item("MovementDelay").GetValue<Slider>().Value);
													Console.WriteLine(mvmtDelay);
													Orbwalking.SetMovementDelay(mvmtDelay);
													return BehaviorState.Success;
												});
	}
}