using NUnit.Framework;
using SurvivalShooter;
using SurvivalShooter.Characters;
using SurvivalShooter.Core.Timers;
using SurvivalShooter.Stats;
using SurvivalShooter.Stats.ModifierStrategy;
using SurvivalShooter.Stats.StatModifier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlayModeTests.Pickups
{
    public class PickupTests
    {
        private readonly List<GameObject> m_gameObjects = new();
        private readonly List<Object> m_assets = new();
        private PlayerBaseStats m_baseStats;
        private Player m_player;

        [SetUp]
        public void SetUp()
        {
            m_baseStats = ScriptableObject.CreateInstance<PlayerBaseStats>();
            m_baseStats.MaxHealth = 100f;
            m_baseStats.AttackDamage = 10f;
            m_baseStats.MovementSpeed = 10f;
            m_assets.Add(m_baseStats);

            m_player = CreatePlayer(m_baseStats);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = m_gameObjects.Count - 1; i >= 0; i--)
            {
                if (m_gameObjects[i] != null)
                    Object.DestroyImmediate(m_gameObjects[i]);
            }

            foreach (Object asset in m_assets)
            {
                if (asset != null)
                    Object.DestroyImmediate(asset);
            }

            m_gameObjects.Clear();
            m_assets.Clear();
        }

        [Test]
        public void PlayerAccept_DelegatesToVisitorWithPlayerInstance()
        {
            RecordingVisitor visitor = new();

            m_player.Accept(visitor);

            Assert.That(visitor.VisitedObject, Is.SameAs(m_player));
            Assert.That(visitor.VisitedType, Is.EqualTo(typeof(Player)));
        }

        [UnityTest]
        public IEnumerator HealPickup_IgnoresNonPlayerCollider()
        {
            HealPickup pickup = CreatePickup<HealPickup>();
            Collider unrelatedCollider = CreateCollider("Unrelated object");

            pickup.Initialize(10f);
            m_player.Health.ApplyDamage(20f);

            pickup.SendMessage("OnTriggerEnter", unrelatedCollider);

            Assert.That(m_player.Health.CurrentHealth, Is.EqualTo(80f));

            yield return null;
        }

        [UnityTest]
        public IEnumerator HealPickup_OnlyAppliesOnceToPlayer()
        {
            HealPickup pickup = CreatePickup<HealPickup>();

            pickup.Initialize(10f);
            m_player.Health.ApplyDamage(20f);

            Assert.That(m_player.Health.CurrentHealth, Is.EqualTo(80f));

            pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>());
            pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>());

            Assert.That(m_player.Health.CurrentHealth, Is.EqualTo(90f));

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatModifierPickup_AddOperationIsPermanentAndScopedToCollector()
        {
            Player otherPlayer = CreatePlayer(m_baseStats);
            StatModifierPickup pickup = CreatePickup<StatModifierPickup>();
            pickup.Initialize(
                StatType.MovementSpeed,
                OperationType.Sum,
                ModifierType.Permanent,
                value: 4f,
                durationInSeconds: 0f);

            pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>());

            Assert.That(m_player.MovementSpeed, Is.EqualTo(14f));
            Assert.That(otherPlayer.MovementSpeed, Is.EqualTo(10f));
            Assert.That(m_baseStats.MovementSpeed, Is.EqualTo(10f));

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatModifierPickup_MultiplyOperationUsesConfiguredFactor()
        {
            StatModifierPickup pickup = CreatePickup<StatModifierPickup>();
            pickup.Initialize(
                StatType.AttackDamage,
                OperationType.Multiply,
                ModifierType.Permanent,
                value: 2f,
                durationInSeconds: 0f);

            pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>());

            Assert.That(m_player.AttackDamage, Is.EqualTo(20f));
            Assert.That(m_baseStats.AttackDamage, Is.EqualTo(10f));

            yield return null;
        }

        [UnityTest]
        public IEnumerator StatModifierPickup_TemporaryEffectExpiresWithoutRemovingOtherModifiers()
        {
            TimerManagerComponent timerManager = CreateTimerManager();
            StatModifierPickup pickup = CreatePickup<StatModifierPickup>();
            pickup.Initialize(
                StatType.MovementSpeed,
                OperationType.Sum,
                ModifierType.Temporary,
                value: 5f,
                durationInSeconds: 1f);

            pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>());
            Assert.That(m_player.MovementSpeed, Is.EqualTo(15f));

            // A separate system adds a valid modifier while the pickup effect is active.
            m_player.Mediator.AddModifier(new FixedStatModifier(
                StatType.MovementSpeed,
                new AddModifierStrategy(3f)));
            Assert.That(m_player.MovementSpeed, Is.EqualTo(18f));

            timerManager.TimerManager.Update(1.01f);

            Assert.That(m_player.MovementSpeed, Is.EqualTo(13f));
            Assert.That(m_baseStats.MovementSpeed, Is.EqualTo(10f));
            Assert.That(timerManager.TimerManager.TimersCount, Is.Zero);

            yield return null;
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void StatModifierPickup_NonPositiveTemporaryDurationExpiresOnFirstTimerUpdate(float duration)
        {
            TimerManagerComponent timerManager = CreateTimerManager();
            StatModifierPickup pickup = CreatePickup<StatModifierPickup>();
            pickup.Initialize(
                StatType.MovementSpeed,
                OperationType.Sum,
                ModifierType.Temporary,
                value: 5f,
                durationInSeconds: duration);

            Assert.DoesNotThrow(() => pickup.SendMessage("OnTriggerEnter", m_player.GetComponent<Collider>()));
            Assert.That(m_player.MovementSpeed, Is.EqualTo(15f));

            timerManager.TimerManager.Update(0f);
            m_player.Mediator.Update(0f);

            Assert.That(m_player.MovementSpeed, Is.EqualTo(10f));
            Assert.That(timerManager.TimerManager.TimersCount, Is.Zero);
        }

        private Player CreatePlayer(PlayerBaseStats baseStats)
        {
            GameObject gameObject = new("Test player");
            gameObject.SetActive(false);
            m_gameObjects.Add(gameObject);

            Player player = gameObject.AddComponent<Player>();
            player.Initialize(baseStats);
            gameObject.SetActive(true);

            gameObject.AddComponent<BoxCollider>();

            return player;
        }

        private T CreatePickup<T>() where T : Pickup
        {
            GameObject gameObject = new(typeof(T).Name);
            m_gameObjects.Add(gameObject);

            T pickup = gameObject.AddComponent<T>();
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            return pickup;
        }

        private Collider CreateCollider(string name)
        {
            GameObject gameObject = new(name);
            m_gameObjects.Add(gameObject);

            return gameObject.AddComponent<BoxCollider>();
        }

        private TimerManagerComponent CreateTimerManager()
        {
            GameObject gameObject = new("Test timer manager");
            gameObject.SetActive(false);
            m_gameObjects.Add(gameObject);
            TimerManagerComponent timerManager = gameObject.AddComponent<TimerManagerComponent>();
            gameObject.SetActive(true);

            return timerManager;
        }

        private sealed class RecordingVisitor : IVisitor
        {
            public object VisitedObject { get; private set; }
            public System.Type VisitedType { get; private set; }

            public void Visit<T>(T visitable)
            {
                VisitedObject = visitable;
                VisitedType = typeof(T);
            }
        }
    }
}
