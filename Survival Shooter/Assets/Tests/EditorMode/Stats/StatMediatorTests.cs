using NUnit.Framework;
using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats;
using UnityEngine.TestTools.Utils;
using SurvivalShooter.Stats.ModifierStrategy;
using SurvivalShooter.Stats.StatModifier;

namespace EditorModeTest.Stats
{
    public class StatMediatorTests
    {
        private readonly FloatEqualityComparer floatEqualityComparer = new(0.001f);

        [Test]
        public void AddModifierStrategy_Execute_AddsConfiguredValue()
        {
            AddModifierStrategy strategy = new(4.5f);

            Assert.That(strategy.Execute(2f), Is.EqualTo(6.5f).Using(floatEqualityComparer));
            Assert.That(strategy.Value, Is.EqualTo(4.5f).Using(floatEqualityComparer));
        }

        [Test]
        public void MultiplyModifierStrategy_Execute_MultipliesByConfiguredValue()
        {
            MultiplyModifierStrategy strategy = new(2.5f);

            Assert.That(strategy.Execute(4f), Is.EqualTo(10f).Using(floatEqualityComparer));
            Assert.That(strategy.Execute(-2f), Is.EqualTo(-5f).Using(floatEqualityComparer));
        }

        [Test]
        public void PerformQuery_AppliesMatchingModifiersInPrecedenceOrder()
        {
            StatMediator mediator = new();
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(3f)));
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new MultiplyModifierStrategy(2f)));
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(-1f)));

            Query query = new Query(StatType.MovementSpeed, 5f);
            mediator.PerformQuery(this, query);

            // Order
            // 1. Add
            // 2. Multiply
            Assert.That(query.Value, Is.EqualTo(14f).Using(floatEqualityComparer));
        }

        [Test]
        public void AddModifier_InsertsMissingLowerPriorityBeforeExistingModifiers()
        {
            StatMediator mediator = new();
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new MultiplyModifierStrategy(2f)));
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(3f)));

            Query query = new Query(StatType.MovementSpeed, 5f);
            mediator.PerformQuery(this, query);

            Assert.That(query.Value, Is.EqualTo(16f).Using(floatEqualityComparer));
        }

        [Test]
        public void Update_RemovingLastModifierForPriorityRestoresPreviousPriorityNode()
        {
            StatMediator mediator = new();
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(1f)));
            TestFixedStatModifier removedLastAdd = new(
                StatType.MovementSpeed, new AddModifierStrategy(100f));
            removedLastAdd.Remove();
            mediator.AddModifier(removedLastAdd);
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new MultiplyModifierStrategy(2f)));

            mediator.Update(0f);
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(3f)));

            Query query = new Query(StatType.MovementSpeed, 5f);
            mediator.PerformQuery(this, query);

            Assert.That(query.Value, Is.EqualTo(18f).Using(floatEqualityComparer));
        }

        [Test]
        public void Update_RemovingOnlyModifierForPriorityAllowsThatPriorityToBeAddedAgain()
        {
            StatMediator mediator = new();
            TestFixedStatModifier removedAdd = new(
                StatType.MovementSpeed, new AddModifierStrategy(100f));
            removedAdd.Remove();
            mediator.AddModifier(removedAdd);
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new MultiplyModifierStrategy(2f)));

            mediator.Update(0f);
            mediator.AddModifier(new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(3f)));

            Query query = new Query(StatType.MovementSpeed, 5f);
            mediator.PerformQuery(this, query);

            Assert.That(query.Value, Is.EqualTo(16f).Using(floatEqualityComparer));
        }

        [Test]
        public void PerformQuery_IgnoresModifiersForDifferentStatTypes()
        {
            StatMediator mediator = new();

            var damageModifier = new TestFixedStatModifier(
                StatType.AttackDamage, new MultiplyModifierStrategy(3f));

            var movementSpeedModifier = new TestFixedStatModifier(
                StatType.MovementSpeed, new AddModifierStrategy(10f));

            mediator.AddModifier(damageModifier);
            mediator.AddModifier(movementSpeedModifier);

            Query query = new Query(StatType.AttackSpeed, 7f);
            mediator.PerformQuery(this, query);

            Assert.That(query.Value, Is.EqualTo(7f).Using(floatEqualityComparer));
            Assert.That(damageModifier.HandleCalls, Is.EqualTo(1));
            Assert.Zero(damageModifier.UpdateCalls);
            Assert.That(movementSpeedModifier.HandleCalls, Is.EqualTo(1));
            Assert.Zero(movementSpeedModifier.UpdateCalls);
        }

        [Test]
        public void Update_RemovesMarkedModifiers_UnsubscribesAndDisposesThem()
        {
            StatMediator mediator = new();
            TestFixedStatModifier first = new(StatType.Health, new AddModifierStrategy(1f));
            TestFixedStatModifier removedA = new(StatType.Health, new AddModifierStrategy(100f));
            TestFixedStatModifier removedB = new(StatType.Health, new AddModifierStrategy(200f));
            TestFixedStatModifier last = new(StatType.Health, new AddModifierStrategy(5f));

            removedA.Remove();
            removedB.Remove();

            mediator.AddModifier(first);
            mediator.AddModifier(removedA);
            mediator.AddModifier(removedB);
            mediator.AddModifier(last);

            mediator.Update(0.25f);

            Assert.That(first.UpdateCalls, Is.EqualTo(1));
            Assert.That(last.UpdateCalls, Is.EqualTo(1));
            Assert.That(removedA.UpdateCalls, Is.Zero);
            Assert.That(removedB.UpdateCalls, Is.Zero);
            Assert.Zero(first.DisposeCalls);
            Assert.Zero(last.DisposeCalls);
            Assert.That(removedA.DisposeCalls, Is.EqualTo(1));
            Assert.That(removedB.DisposeCalls, Is.EqualTo(1));

            Query query = new Query(StatType.Health, 10f);
            mediator.PerformQuery(this, query);
            Assert.That(query.Value, Is.EqualTo(16f).Using(floatEqualityComparer));
            Assert.That(first.HandleCalls, Is.EqualTo(1));
            Assert.That(last.HandleCalls, Is.EqualTo(1));
            Assert.Zero(removedA.HandleCalls);
            Assert.Zero(removedB.HandleCalls);
            Assert.That(first.UpdateCalls, Is.EqualTo(1));
            Assert.That(last.UpdateCalls, Is.EqualTo(1));
            Assert.Zero(removedA.UpdateCalls);
            Assert.Zero(removedB.UpdateCalls);


            first.Remove();
            last.Remove();
            mediator.Update(0f);

            Assert.That(first.UpdateCalls, Is.EqualTo(1));
            Assert.That(last.UpdateCalls, Is.EqualTo(1));
            Assert.That(first.DisposeCalls, Is.EqualTo(1));
            Assert.That(last.DisposeCalls, Is.EqualTo(1));

            query = new Query(StatType.Health, 10f);
            mediator.PerformQuery(this, query);
            Assert.That(query.Value, Is.EqualTo(10f).Using(floatEqualityComparer));
            Assert.That(first.HandleCalls, Is.EqualTo(1));
            Assert.That(last.HandleCalls, Is.EqualTo(1));
        }

        [Test]
        public void TemporaryModifier_ExpiresAndIsRemovedFromMediator()
        {
            TimerManager timerManager = new();
            StatMediator mediator = new();
            TestTemporaryStatModifier modifier = new(
                timerManager, 1f, StatType.Health, new AddModifierStrategy(5f));
            mediator.AddModifier(modifier);

            Assert.That(timerManager.TimersCount, Is.EqualTo(1));
            AssertQueryValue(mediator, StatType.Health, 20f, 25f);

            mediator.Update(0.5f);
            Assert.IsFalse(modifier.MarkToRemoval);
            AssertQueryValue(mediator, StatType.Health, 20f, 25f);

            mediator.Update(0.51f);
            Assert.IsTrue(modifier.MarkToRemoval);
            Assert.Zero(timerManager.TimersCount);

            AssertQueryValue(mediator, StatType.Health, 20f, 20f);
        }

        private void AssertQueryValue(
            StatMediator mediator, StatType statType, float initialValue, float expectedValue)
        {
            Query query = new Query(statType, initialValue);
            mediator.PerformQuery(null, query);

            Assert.That(query.Value, Is.EqualTo(expectedValue).Using(floatEqualityComparer));
        }

        private sealed class TestTemporaryStatModifier : TemporaryStatModifier
        {
            public int HandleCalls { get; private set; }
            public int UpdateCalls { get; private set; }
            public int DisposedCalls { get; private set; }

            public TestTemporaryStatModifier(
                TimerManager timerManager,
                float duration,
                StatType statType,
                IModifierStrategy strategy)
                : base(timerManager, duration, statType, strategy) { }

            public override void Handle(object sender, Query query)
            {
                base.Handle(sender, query);
                HandleCalls++;
            }

            public override void Update(float deltaTime)
            {
                base.Update(deltaTime);
                UpdateCalls++;
            }

            public override void Dispose()
            {
                base.Dispose();
                DisposedCalls++;
            }
        }

        private sealed class TestFixedStatModifier : FixedStatModifier
        {
            public int HandleCalls { get; private set; }
            public int UpdateCalls { get; private set; }
            public int DisposeCalls { get; private set; }

            public void Remove()
            {
                MarkToRemoval = true;
            }

            public TestFixedStatModifier(StatType statType, IModifierStrategy strategy) 
                : base(statType, strategy) { }

            public override void Handle(object sender, Query query)
            {
                base.Handle(sender, query);
                HandleCalls++;
            }

            public override void Update(float deltaTime)
            {
                base.Update(deltaTime);
                UpdateCalls++;
            }

            public override void Dispose()
            {
                base.Dispose();
                DisposeCalls++;
            }
        }
    }
}
