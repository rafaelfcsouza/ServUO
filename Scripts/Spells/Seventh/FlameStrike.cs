using Server.Targeting;

namespace Server.Spells.Seventh
{
    public class FlameStrikeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Flame Strike", "Kal Vas Flam",
            245,
            9042,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);
        public FlameStrikeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Seventh;
        public override bool DelayedDamage => true;

        protected override Target CreateTarget() => new SpellTarget<FlameStrikeSpell, IDamageable>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            IDamageable m = o as IDamageable;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                Mobile source = Caster;

                SpellHelper.CheckReflect((int)Circle, ref source, ref m);

                double damage = GetNewAosDamage(48, 1, 5, m);

                if (m != null)
                {
                    m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
                    m.PlaySound(0x208);
                }

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 100, 0, 0, 0);
                }
            }

            FinishSequence();
        }
    }
}
