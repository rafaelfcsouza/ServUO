using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class RemoveTrapSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Remove Trap", "An Jux",
            212,
            9001,
            Reagent.Bloodmoss,
            Reagent.SulfurousAsh);
        public RemoveTrapSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Second;

        protected override Target CreateTarget() => new RemoveTrapTarget(this);

        public void Target(TrapableContainer item)
        {
            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (item.TrapType != TrapType.None && item.TrapType != TrapType.MagicTrap)
            {
                base.DoFizzle();
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                Point3D loc = item.GetWorldLocation();

                Effects.SendLocationParticles(EffectItem.Create(loc, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5015);
                Effects.PlaySound(loc, item.Map, 0x1F0);

                item.TrapType = TrapType.None;
                item.TrapPower = 0;
                item.TrapLevel = 0;
            }

            FinishSequence();
        }

        private class RemoveTrapTarget : SpellTarget<RemoveTrapSpell, Mobile>
        {
            public RemoveTrapTarget(RemoveTrapSpell removeTrap) : base(removeTrap, TargetFlags.None) { }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!(o is TrapableContainer))
                {
                    from.SendLocalizedMessage(501856); // That isn't trapped.
                    return;
                }

                if (Spell.Caster is PlayerMobile) Spell.Invoke(o);
                else Spell.Target((TrapableContainer) o);
            }
        }
    }
}
