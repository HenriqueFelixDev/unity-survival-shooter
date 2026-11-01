using SurvivalShooter.Characters;
using UnityEngine;

namespace SurvivalShooter
{
    public abstract class Pickup : MonoBehaviour, IVisitor
    {
        private Transform m_collector;

        protected abstract void HandlePickup(Player player);

        public void Visit<T>(T visitable)
        {
            if (m_collector != null) return;

            if (visitable is Player player)
            {
                HandlePickup(player);
                m_collector = player.transform;
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IVisitable visitable))
            {
                visitable.Accept(this);
            }
        }
    }
}
