using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class LightningSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Lightning", "Por Ort Grav",
            239,
            9021,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public LightningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fourth;
        public override bool DelayedDamage => false;

        protected override Target CreateTarget() => new SpellTarget<LightningSpell, IDamageable>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            IDamageable m = o as IDamageable;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                Mobile source = Caster;
                SpellHelper.Turn(Caster, m.Location);

                SpellHelper.CheckReflect((int)Circle, ref source, ref m);

                double damage = GetNewAosDamage(23, 1, 4, m);

                if (m is Mobile)
                {
                    Effects.SendBoltEffect(m, true, 0, false);
                }
                else
                {
                    Effects.SendBoltEffect(EffectMobile.Create(m.Location, m.Map, EffectMobile.DefaultDuration), true, 0, false);
                }

                if (damage > 0)
                {
                    SpellHelper.Damage(this, m, damage, 0, 0, 0, 0, 100);
                }
            }

            FinishSequence();
        }
    }
}
