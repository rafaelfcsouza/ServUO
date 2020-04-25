using Server.Targeting;

namespace Server.Spells.Seventh
{
    public class ManaVampireSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mana Vampire", "Ort Sanct",
            221,
            9032,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);
        public ManaVampireSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Seventh;

        protected override Target CreateTarget() => new SpellTarget<ManaVampireSpell, Mobile>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            Mobile m = o as Mobile;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                SpellHelper.CheckReflect((int)Circle, Caster, ref m);

                if (m.Spell != null)
                    m.Spell.OnCasterHurt();

                m.Paralyzed = false;

                int toDrain = (int)(GetDamageSkill(Caster) - GetResistSkill(m));

                if (!m.Player)
                    toDrain /= 2;

                if (toDrain < 0)
                    toDrain = 0;
                else if (toDrain > m.Mana)
                    toDrain = m.Mana;

                if (toDrain > (Caster.ManaMax - Caster.Mana))
                    toDrain = Caster.ManaMax - Caster.Mana;

                m.Mana -= toDrain;
                Caster.Mana += toDrain;

                m.FixedParticles(0x374A, 1, 15, 5054, 23, 7, EffectLayer.Head);
                m.PlaySound(0x1F9);

                Caster.FixedParticles(0x0000, 10, 5, 2054, EffectLayer.Head);

                HarmfulSpell(m);
            }

            FinishSequence();
        }

        public override double GetResistPercent(Mobile target)
        {
            return 98.0;
        }
    }
}
