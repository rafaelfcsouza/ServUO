using Server.Targeting;
using System;

namespace Server.Spells.Sixth
{
    public class EnergyBoltSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Energy Bolt", "Corp Por",
            230,
            9022,
            Reagent.BlackPearl,
            Reagent.Nightshade);
        public EnergyBoltSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Sixth;
        public override bool DelayedDamage => true;

        protected override Target CreateTarget() => new SpellTarget<EnergyBoltSpell, IDamageable>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            Mobile m = o as Mobile;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                IDamageable source = Caster;
                IDamageable target = m;

                SpellHelper.Turn(Caster, m);

                if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                    {
                        source.MovingParticles(target, 0x379F, 7, 0, false, true, 3043, 4043, 0x211);
                        source.PlaySound(0x20A);
                    });
                }

                double damage = GetNewAosDamage(40, 1, 5, m);

                // Do the effects
                Caster.MovingParticles(m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211);
                Caster.PlaySound(0x20A);

                if (damage > 0)
                {
                    // Deal the damage
                    SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 100);
                }
            }

            FinishSequence();
        }
    }
}
