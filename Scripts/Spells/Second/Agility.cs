using Server.Targeting;
using System;
using Server.Mobiles;

namespace Server.Spells.Second
{
    public class AgilitySpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Agility", "Ex Uus",
            212,
            9061,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public AgilitySpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Second;

        protected override Target CreateTarget() => new SpellTarget<AgilitySpell, Mobile>(this, TargetFlags.Beneficial);

        public override void Target(object o)
        {
            var m = (Mobile) o;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckBSequence(m))
            {
                int oldDex = SpellHelper.GetBuffOffset(m, StatType.Dex);
                int newDex = SpellHelper.GetOffset(Caster, m, StatType.Dex, false, true);

                if (newDex < oldDex || newDex == 0)
                {
                    DoHurtFizzle();
                }
                else
                {
                    SpellHelper.Turn(Caster, m);

                    SpellHelper.AddStatBonus(Caster, m, false, StatType.Dex);
                    int percentage = (int)(SpellHelper.GetOffsetScalar(Caster, m, false) * 100);
                    TimeSpan length = SpellHelper.GetDuration(Caster, m);
                    BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.Agility, 1075841, length, m, percentage.ToString()));

                    m.FixedParticles(0x375A, 10, 15, 5010, EffectLayer.Waist);
                    m.PlaySound(0x1e7);
                }
            }

            FinishSequence();
        }
    }
}
