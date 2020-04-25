using Server.Targeting;
using System;
using Server.Mobiles;

namespace Server.Spells.Second
{
    public class CunningSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Cunning", "Uus Wis",
            212,
            9061,
            Reagent.MandrakeRoot,
            Reagent.Nightshade);
        public CunningSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Second;

        protected override Target CreateTarget() => new SpellTarget<CunningSpell, Mobile>(this, TargetFlags.Beneficial);

        public override void Target(object o)
        {
            var m = (Mobile) o;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                int oldInt = SpellHelper.GetBuffOffset(m, StatType.Int);
                int newInt = SpellHelper.GetOffset(Caster, m, StatType.Int, false, true);

                if (newInt < oldInt || newInt == 0)
                {
                    DoHurtFizzle();
                }
                else
                {
                    SpellHelper.Turn(Caster, m);

                    SpellHelper.AddStatBonus(Caster, m, false, StatType.Int);
                    int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                    TimeSpan length = SpellHelper.GetDuration(Caster, m);
                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Cunning, 1075843, length, m, percentage.ToString()));

                    m.FixedParticles(0x375A, 10, 15, 5011, EffectLayer.Head);
                    m.PlaySound(0x1EB);
                }
            }

            FinishSequence();
        }
    }
}
