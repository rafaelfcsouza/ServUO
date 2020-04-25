using Server.Mobiles;
using Server.Spells.Chivalry;
using Server.Targeting;
using System;

namespace Server.Spells.Fifth
{
    public class ParalyzeSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Paralyze", "An Ex Por",
            218,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public ParalyzeSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

        protected override Target CreateTarget() => new SpellTarget<ParalyzeSpell, Mobile>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            Mobile m = o as Mobile;

            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (m.Frozen || m.Paralyzed || (m.Spell != null && m.Spell.IsCasting && !(m.Spell is PaladinSpell)))
            {
                Caster.SendLocalizedMessage(1061923); // The target is already frozen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)Circle, Caster, ref m);

                double duration;

                int secs = (int)((GetDamageSkill(Caster) / 10) - (GetResistSkill(m) / 10));

                if (!m.Player)
                    secs *= 3;

                if (secs < 0)
                    secs = 0;

                duration = secs;

                if (m is PlagueBeastLord)
                {
                    ((PlagueBeastLord)m).OnParalyzed(Caster);
                    duration = 120;
                }

                m.Paralyze(TimeSpan.FromSeconds(duration));

                m.PlaySound(0x204);
                m.FixedEffect(0x376A, 6, 1);

                HarmfulSpell(m);
            }

            FinishSequence();
        }
    }
}
