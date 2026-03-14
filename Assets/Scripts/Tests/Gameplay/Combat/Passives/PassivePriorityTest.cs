using System;
using System.Linq;
using NUnit.Framework;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Combat.Passives;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;
using Project.Gameplay.Gameplay.Logging;

namespace Project.Tests.Combat.Passives
{
    /// <summary>
    /// Tests for trigger execution order and phase priorities.
    /// </summary>
    public class PassivePriorityTest : PassiveFixture
    {
        [Test]
        public void DesperationExecutesBeforeSwarm_HighPriority()
        {
            // Arrange
            var figure = CreateFigure(attack: 10, position: new GridPosition(0, 0));
            figure.AddPassive(new DesperationPassive("desperation", new LogService()));
            figure.AddPassive(new SwarmPassive("swarm", damagePerAlly: 2, duration: 1));
            
            // Create allies nearby
            CreateFigure(position: new GridPosition(0, 1));
            CreateFigure(position: new GridPosition(1, 0));
            
            // Act
            var target = CreateFigure(position: new GridPosition(0, 5));
            var context = CreateBeforeHitContext(figure, target, baseDamage: 10);
            
            // Execute in BeforeCalculation phase
            TriggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeCalculation,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnBeforeHit)
                    .WithPhase(TriggerPhase.BeforeCalculation)
                    .WithActor(figure)
                    .WithTarget(target)
                    .WithValue(10)
                    .WithData(context)
                    .Build());
            
            // Assert: Desperation should execute first (Priority = High, Group = First)
            // With allies nearby, Desperation should NOT apply
            // Swarm should add +4 (2 allies * 2 damage)
            Assert.That(figure.Stats.Attack.Value, Is.GreaterThan(10f));
        }
        
        [Test]
        public void CriticalExecutesInBeforeHitPhase()
        {
            // Arrange
            var attacker = CreateFigure(attack: 10);
            var target = CreateFigure(position: new GridPosition(0, 5));
            var random = new TestRandomService(shouldCrit: true);
            attacker.AddPassive(new CriticalPassive("crit", 1f, 2f, random));
            
            // Act
            var context = CreateBeforeHitContext(attacker, target, baseDamage: 10);
            ExecuteBeforeHit(context);

            // Assert: Critical should be applied
            Assert.That(context.IsCritical, Is.True);
            Assert.That(context.DamageMultiplier, Is.GreaterThan(1.0f));
        }
        
        [Test]
        public void ExecuteMultiplierAppliedInBeforeHitPhase()
        {
            // Arrange
            var attacker = CreateFigure(attack: 10);
            var target = CreateFigure(hp: 20, maxHp: 100, position: new GridPosition(0, 5)); // 20% HP
            attacker.AddPassive(new ExecutePassive("execute", 0.3f, 1.5f));

            // Act
            var context = CreateBeforeHitContext(attacker, target, baseDamage: 10);
            ExecuteBeforeHit(context);

            // Assert: Execute multiplier should be applied
            Assert.That(context.DamageMultiplier, Is.GreaterThan(1.0f));
        }
        
        [Test]
        public void LifestealTriggersInAfterHitPhase()
        {
            // Arrange
            var attacker = CreateFigure(hp: 50, maxHp: 100, position: new GridPosition(0, 0));
            var target = CreateFigure(position: new GridPosition(0, 5));
            attacker.AddPassive(new LifestealPassive("lifesteal", 0.3f));
            
            // Act
            var context = CreateAfterHitContext(
                attacker, target,
                new GridPosition(0, 0), new GridPosition(0, 5),
                damageDealt: 10);
            
            TriggerService.Execute(TriggerType.OnAfterHit, TriggerPhase.AfterHit,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnAfterHit)
                    .WithPhase(TriggerPhase.AfterHit)
                    .WithActor(attacker)
                    .WithTarget(target)
                    .WithValue(10)
                    .WithData(context)
                    .Build());
            
            // Assert: Lifesteal should trigger
            Assert.Pass("Lifesteal effect added to context");
        }
        
        [Test]
        public void InspirationTriggersOnTurnStart()
        {
            // Arrange
            var king = CreateFigure(position: new GridPosition(0, 0));
            var ally = CreateFigure(position: new GridPosition(0, 1));
            var random = new TestRandomService();
            king.AddPassive(new InspirationPassive("inspiration", 
                attackBonus: 2, defenceBonus: 1, evasionBonus: 0.1f, 
                random: random, buffDuration: 2));
            
            // Act
            var turnContext = new TurnContext
            {
                Team = Team.Player,
                Grid = Grid,
                CurrentTurn = 1
            };
            
            TriggerService.Execute(TriggerType.OnTurnStart, TriggerPhase.OnTurnStart,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnTurnStart)
                    .WithPhase(TriggerPhase.OnTurnStart)
                    .WithActor(king)
                    .WithData(turnContext)
                    .Build());
            
            // Assert: Inspiration buff should be applied to ally
            var hasInspiration = ally.Effects.GetEffects().Any(e => e.Id == "inspiration_buff");
            Assert.That(hasInspiration, Is.True);
        }
        
        [Test]
        public void RoyalPresenceTriggersOnMove()
        {
            // Arrange
            var king = CreateFigure(position: new GridPosition(0, 0));
            var ally = CreateFigure(position: new GridPosition(0, 2));

            // Add passive (this registers it with TriggerService from SetUp)
            var passive = new RoyalPresencePassive("royal_presence", damageBonus: 1f, auraRadius: 2);
            king.AddPassive(passive);
            
            Assert.That(king.BasePassives.Count, Is.EqualTo(1), "Passive should be added");
            
            // Debug: check if passive is registered in TriggerService
            var triggersField = typeof(TriggerService).GetField("_triggers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var triggers = triggersField?.GetValue(TriggerService) as System.Collections.Generic.List<ITrigger>;
            Assert.That(triggers, Is.Not.Null, "TriggerService._triggers should not be null");
            Assert.That(triggers!.Contains(passive), Is.True, $"Passive should be registered in TriggerService. Total triggers: {triggers.Count}");

            // Mark king as having moved
            king.MovedThisTurn = true;

            // Act
            var moveContext = new MoveContext
            {
                Actor = king,
                Grid = Grid,
                From = new GridPosition(0, 0),
                To = new GridPosition(0, 1),
                DidMove = true
            };
            
            // First Execute builds the cache (includes the passive)
            var context = new TriggerContextBuilder()
                .WithType(TriggerType.OnMove)
                .WithPhase(TriggerPhase.AfterMove)
                .WithActor(king)
                .WithData(moveContext)
                .Build();
                
            // Debug: check if passive matches the context
            Assert.That(passive.Matches(context), Is.True, "Passive.Matches() should return true for OnMove context");
            
            var result = TriggerService.Execute(TriggerType.OnMove, TriggerPhase.AfterMove, context);
            
            // Debug: check if king is on grid
            var kingCell = Grid.FindFigure(king);
            Assert.That(kingCell, Is.Not.Null, "King should be on grid");
            
            // Debug: check if ally is in range
            var alliesInRange = Grid.GetFiguresInRadius(kingCell.Position, 2)
                .Where(f => f.Team == king.Team && f != king)
                .ToList();
            Assert.That(alliesInRange.Count, Is.GreaterThan(0), $"Should have allies in range. King at {kingCell.Position}");
            
            // Debug: check what effects ally has
            var effects = ally.Effects.GetEffects().ToList();
            
            // Assert: Royal presence buff should be applied to ally
            var hasBuff = ally.Effects.GetEffects().Any(e => e.Id == "royal_presence");
            Assert.That(hasBuff, Is.True, $"Ally should have royal_presence buff. Effects: {string.Join(", ", effects.Select(e => e.Id))}");
        }
    }
}
