using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using Ultima;

namespace Server.Spells.Third
{
    public class MagicLockSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Lock", "An Por",
            215,
            9001,
            Reagent.Garlic,
            Reagent.Bloodmoss,
            Reagent.SulfurousAsh);

        public MagicLockSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;

        protected override Target CreateTarget() => new MagicLockTarget(this);

        public override void Target(object o)
        {
            LockableContainer targ = o as LockableContainer;
            if (Multis.BaseHouse.CheckLockedDownOrSecured(targ))
            {
                // You cannot cast this on a locked down item.
                Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 501761);
            }
            else if (targ.Locked || targ.LockLevel == 0 || targ is ParagonChest)
            {
                // Target must be an unlocked chest.
                Caster.SendLocalizedMessage(501762);
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, targ);

                Point3D loc = targ.GetWorldLocation();

                Effects.SendLocationParticles(
                    EffectItem.Create(loc, targ.Map, EffectItem.DefaultDuration),
                    0x376A, 9, 32, 5020);

                Effects.PlaySound(loc, targ.Map, 0x1FA);

                // The chest is now locked!
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501763);

                targ.LockLevel = -255; // signal magic lock
                targ.Locked = true;
            }

            FinishSequence();
        }

        private class MagicLockTarget : SpellTarget<MagicLockSpell, LockableContainer>
        {
            public MagicLockTarget(MagicLockSpell spell) : base(spell, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!(o is LockableContainer)) from.SendLocalizedMessage(501762); // Target must be an unlocked chest.
                else if (from is PlayerMobile) Spell.Invoke(o);
                else if (o is LockableContainer container) Spell.Target(container);

            }
        }
    }
}
