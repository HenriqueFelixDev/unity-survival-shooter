using SurvivalShooter.Stats.StatModifier;
using System;
using System.Collections.Generic;

namespace SurvivalShooter.Stats
{
    public class StatMediator
    {
        private readonly LinkedList<IStatModifier> m_modifiers = new();
        private readonly Dictionary<int, LinkedListNode<IStatModifier>> m_lastNodeByPriority = new();

        public event Action<IStatModifier> OnChange;

        public void AddModifier(IStatModifier modifier)
        {
            LinkedListNode<IStatModifier> addedNode = InsertNodeInOrder(modifier);

            m_lastNodeByPriority[modifier.GetPriority()] = addedNode;
            OnChange?.Invoke(addedNode.Value);
        }

        private void RemoveNode(LinkedListNode<IStatModifier> node)
        {
            var priority = node.Value.GetPriority();

            UpdateLastNodeOfPriority(priority, node);
            m_modifiers.Remove(node);

            node.Value.Dispose();
            OnChange?.Invoke(node.Value);
        }

        public void PerformQuery(object sender, Query query)
        {
            for (var node = m_modifiers.First; node != null; node = node.Next)
            {
                if (node.Value.CanHandle(sender, query))
                    node.Value.Handle(sender, query);
            }
        }

        public void Update(float deltaTime)
        {
            var node = m_modifiers.First;

            while (node != null)
            {
                var next = node.Next;

                UpdateModifier(node, deltaTime);

                node = next;
            }
        }

        private LinkedListNode<IStatModifier> InsertNodeInOrder(IStatModifier modifier)
        {
            int priority = modifier.GetPriority();

            var next = FirstNodeWithHigherPriority(priority, modifier);

            if (next != null)
            {
                return m_modifiers.AddBefore(next, modifier);
            }

            return m_modifiers.AddLast(modifier);
        }

        private LinkedListNode<IStatModifier> FirstNodeWithHigherPriority(int priority, IStatModifier modifier)
        {
            if (m_lastNodeByPriority.TryGetValue(priority, out var lastPriorityNode))
            {
                return lastPriorityNode.Next;
            }

            var node = m_modifiers.First;

            while (node != null && node.Value.GetPriority() <= priority)
            {
                node = node.Next;
            }

            return node;
        }

        private void UpdateLastNodeOfPriority(int priority, LinkedListNode<IStatModifier> node)
        {
            if (!IsLastNodeOfPriority(priority, node))
                return;

            var previous = node.Previous;

            if (previous != null && previous.Value.GetPriority() == priority)
            {
                m_lastNodeByPriority[priority] = previous;
                return;
            }

            m_lastNodeByPriority.Remove(priority);
        }

        private void UpdateModifier(LinkedListNode<IStatModifier> node, float deltaTime)
        {
            if (!node.Value.MarkToRemoval)
            {
                node.Value.Update(deltaTime);
            }

            if (node.Value.MarkToRemoval)
            {
                RemoveNode(node);
            }
        }

        private bool IsLastNodeOfPriority(int priority, LinkedListNode<IStatModifier> node)
        {
            return m_lastNodeByPriority.TryGetValue(priority, out var lastPriorityNode)
                && lastPriorityNode == node;
        }
    }
}
