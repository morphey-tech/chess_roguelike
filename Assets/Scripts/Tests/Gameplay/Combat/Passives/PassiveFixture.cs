using System.Collections.Generic;
using NUnit.Framework;
using Project.Core.Core.Combat;
using Project.Core.Core.Grid;
using Project.Core.Core.Logging;
using Project.Core.Core.Random;
using Project.Core.Core.Triggers;
using Project.Gameplay.Gameplay.Attack;
using Project.Gameplay.Gameplay.Combat.Contexts;
using Project.Gameplay.Gameplay.Figures;
using Project.Gameplay.Gameplay.Grid;

namespace Project.Tests.Combat.Passives
{
    /// <summary>
    /// Test fixture providing helper methods and test data for passive ability tests.
    /// </summary>
    public class PassiveFixture
    {
        protected TriggerService TriggerService { get; private set; } = null!;
        protected BoardGrid Grid { get; private set; } = null!;
        protected FigureFactory FigureFactory { get; private set; } = null!;
        
        [SetUp]
        public virtual void SetUp()
        {
            var logService = new NullLogService();
            TriggerService = new TriggerService(logService);
            Grid = new BoardGrid(10, 10);
            FigureFactory = new FigureFactory(TriggerService);
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            TriggerService.Dispose();
        }
        
        protected Figure CreateFigure(
            string typeId = "test_figure",
            Team team = Team.Player,
            int hp = 100,
            int maxHp = 100,
            float attack = 10f,
            float defence = 0f,
            GridPosition? position = null)
        {
            FigureStats stats = new(maxHp, System.Array.Empty<AttackProfile>(), attack,
                defence, 0)
                {
                    CurrentHp =
                    {
                        Value = hp
                    }
                };

            var figure = new Figure(
                id: GetHashCode(),
                typeId: typeId,
                movementId: "default",
                attackId: "default",
                turnPatternsId: "default",
                stats: stats,
                team: team,
                triggerService: TriggerService);
            
            if (position.HasValue)
            {
                Grid.PlaceFigure(figure, position.Value);
            }
            
            return figure;
        }
        
        protected BeforeHitContext CreateBeforeHitContext(
            Figure attacker,
            Figure target,
            float baseDamage = 10f)
        {
            return new BeforeHitContext
            {
                Attacker = attacker,
                Target = target,
                Grid = Grid,
                BaseDamage = baseDamage
            };
        }
        
        protected AfterHitContext CreateAfterHitContext(
            Figure attacker,
            Figure target,
            GridPosition attackerPos,
            GridPosition targetPos,
            int damageDealt = 10,
            bool targetDied = false)
        {
            return new AfterHitContext
            {
                Attacker = attacker,
                Target = target,
                Grid = Grid,
                AttackerPosition = attackerPos,
                TargetPosition = targetPos,
                DamageDealt = damageDealt,
                TargetDied = targetDied
            };
        }
        
        public void RebuildCache()
        {
            // Re-register all passives from figures in the test
            // This is needed because cache is built at first Execute call
            // and doesn't include newly added passives
            // For simplicity, just dispose and create new service
            // Tests should add passives AFTER calling this method
            TriggerService?.Dispose();
            TriggerService = new TriggerService(new NullLogService());
        }
        
        protected void ExecuteBeforeHit(BeforeHitContext context)
        {
            // Execute in BeforeHit phase where crit/execute passives run
            TriggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeHit,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnBeforeHit)
                    .WithPhase(TriggerPhase.BeforeHit)
                    .WithActor(context.Attacker)
                    .WithTarget(context.Target)
                    .WithValue(context.BaseDamage)
                    .WithData(context)
                    .Build());
        }
        
        protected void ExecuteBeforeCalculation(BeforeHitContext context)
        {
            // Execute in BeforeCalculation phase where damage calculation passives run
            TriggerService.Execute(TriggerType.OnBeforeHit, TriggerPhase.BeforeCalculation,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnBeforeHit)
                    .WithPhase(TriggerPhase.BeforeCalculation)
                    .WithActor(context.Attacker)
                    .WithTarget(context.Target)
                    .WithValue(context.BaseDamage)
                    .WithData(context)
                    .Build());
        }

        protected void ExecuteAfterHit(AfterHitContext context)
        {
            TriggerService.Execute(TriggerType.OnAfterHit, TriggerPhase.AfterHit,
                new TriggerContextBuilder()
                    .WithType(TriggerType.OnAfterHit)
                    .WithPhase(TriggerPhase.AfterHit)
                    .WithActor(context.Attacker)
                    .WithTarget(context.Target)
                    .WithValue(context.DamageDealt)
                    .WithData(context)
                    .Build());
        }
    }
    
    /// <summary>
    /// Null implementation of ILogService for tests.
    /// </summary>
    internal sealed class NullLogService : ILogService
    {
        public LogLevel MinLevel { get; set; } = LogLevel.None;
        
        public void Log(LogLevel level, string category, string message) { }
        public void Log(LogLevel level, string category, string message, System.Exception exception) { }
        public ILogger<T> CreateLogger<T>() => new NullLogger<T>();
        public ILogger CreateLogger(string category) => new NullLogger();
        
        private sealed class NullLogger : ILogger
        {
            public string Category => string.Empty;
            public void Trace(string message) { }
            public void Debug(string message) { }
            public void Info(string message) { }
            public void Warning(string message) { }
            public void Error(string message) { }
            public void Error(string message, System.Exception ex) { }
            public void Fatal(string message) { }
            public void Fatal(string message, System.Exception ex) { }
        }
        
        private sealed class NullLogger<T> : ILogger<T>
        {
            public string Category => string.Empty;
            public void Trace(string message) { }
            public void Debug(string message) { }
            public void Info(string message) { }
            public void Warning(string message) { }
            public void Error(string message) { }
            public void Error(string message, System.Exception ex) { }
            public void Fatal(string message) { }
            public void Fatal(string message, System.Exception ex) { }
        }
    }
    
    /// <summary>
    /// Simple random service for tests with deterministic results.
    /// </summary>
    internal sealed class TestRandomService : IRandomService
    {
        private readonly float _critChance;
        private readonly bool _shouldCrit;
        
        public TestRandomService(bool shouldCrit = true, float critChance = 1f)
        {
            _shouldCrit = shouldCrit;
            _critChance = critChance;
        }
        
        public bool Chance(float chance) => _shouldCrit;
        public float Value { get; }
        public int Range(int min, int max) => min;
        public float Range(float min, float max) => min;
        public T RandomElement<T>(IList<T> list) => list[0];
        public T RandomElement<T>(ICollection<T> collection) => collection.GetEnumerator().Current!;
    }

    public sealed class FigureConfig
    {
        public string Id { get; set; } = "test";
    }
}
