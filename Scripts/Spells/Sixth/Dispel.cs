using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class DispelSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Dispel", "An Ort",
            218,
            9002,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public DispelSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Sixth;

        protected override Target CreateTarget() => new SpellTarget<DispelSpell, Mobile>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            if (!(o is Mobile)) return;

            Mobile m = (Mobile)o;
            BaseCreature bc = m as BaseCreature;

            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (bc == null || !bc.IsDispellable)
            {
                Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
            }
            else if (bc.SummonMaster == Caster || CheckHSequence(m))
            {
                SpellHelper.Turn(Caster, m);

                double dispelChance = (50.0 + ((100 * (Caster.Skills.Magery.Value - bc.GetDispelDifficulty())) / (bc.DispelFocus * 2))) / 100;

                //Skill Masteries
                dispelChance -= ((double)SkillMasteries.MasteryInfo.EnchantedSummoningBonus(bc) / 100);

                if (dispelChance > Utility.RandomDouble())
                {
                    Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                    Effects.PlaySound(m, m.Map, 0x201);

                    m.Delete();
                }
                else
                {
                    m.FixedEffect(0x3779, 10, 20);
                    Caster.SendLocalizedMessage(1010084); // The creature resisted the attempt to dispel it!
                }
            }
        }
    }
}
