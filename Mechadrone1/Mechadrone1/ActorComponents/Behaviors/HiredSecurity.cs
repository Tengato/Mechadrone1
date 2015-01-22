namespace Mechadrone1
{
    class HiredSecurity : Behavior, IDamagable
    {
        private int mHitPoints;

        public HiredSecurity(Actor owner)
            : base(owner)
        {
            mHitPoints = 100;
        }

        public void TakeDamage(int amount)
        {
            mHitPoints -= amount;
            if (mHitPoints < 1)
            {
                BipedControllerComponent control = Owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
                control.KnockDown();
            }
        }

        public override void Release()
        {
        }
    }
}
