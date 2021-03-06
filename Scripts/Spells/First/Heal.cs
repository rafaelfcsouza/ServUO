using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.First
{
    public class HealSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Heal", "In Mani",
            224,
            9061,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.SpidersSilk);
        public HealSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.First;

        protected override Target CreateTarget() => new SpellTarget<HealSpell, Mobile>(this, TargetFlags.Beneficial);

        public override void Target(object o)
        {
            var m = (Mobile) o;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m.IsDeadBondedPet)
            {
                Caster.SendLocalizedMessage(1060177); // You cannot heal a creature that is already dead!
            }
            else if (m is BaseCreature && ((BaseCreature)m).IsAnimatedDead)
            {
                Caster.SendLocalizedMessage(1061654); // You cannot heal that which is not alive.
            }
            else if (m is IRepairableMobile)
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500951); // You cannot heal that.
            }
            else if (m.Poisoned || Server.Items.MortalStrike.IsWounded(m))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x22, (Caster == m) ? 1005000 : 1010398);
            }
            else if (CheckBSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                int toHeal;

                toHeal = Caster.Skills.Magery.Fixed / 120;
                toHeal += Utility.RandomMinMax(1, 4);

                if (Caster != m)
                    toHeal = (int)(toHeal * 1.5);

                //m.Heal( toHeal, Caster );
                SpellHelper.Heal(toHeal, m, Caster);

                m.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
                m.PlaySound(0x1F2);
            }

            FinishSequence();
        }
    }
}
