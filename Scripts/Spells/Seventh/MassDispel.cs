using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System.Collections.Generic;

namespace Server.Spells.Seventh
{
    public class MassDispelSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mass Dispel", "Vas An Ort",
            263,
            9002,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.BlackPearl,
            Reagent.SulfurousAsh);
        public MassDispelSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Seventh;

        protected override Target CreateTarget() => new SpellTarget<MassDispelSpell, IPoint3D>(this, TargetFlags.None);

        public override void Target(object o)
        {
            IPoint3D p = o as IPoint3D;
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, p);

                SpellHelper.GetSurfaceTop(ref p);

                List<Mobile> targets = new List<Mobile>();

                Map map = Caster.Map;

                if (map != null)
                {
                    IPooledEnumerable eable = map.GetMobilesInRange(new Point3D(p), 8);

                    foreach (Mobile m in eable)
                        if (m is BaseCreature && (m as BaseCreature).IsDispellable && (((BaseCreature)m).SummonMaster == Caster || Caster.CanBeHarmful(m, false)))
                            targets.Add(m);

                    eable.Free();
                }

                for (int i = 0; i < targets.Count; ++i)
                {
                    Mobile m = targets[i];

                    BaseCreature bc = m as BaseCreature;

                    if (bc == null)
                        continue;

                    double dispelChance = (50.0 + ((100 * (Caster.Skills.Magery.Value - bc.GetDispelDifficulty())) / (bc.DispelFocus * 2))) / 100;

                    // Skill Masteries
                    dispelChance -= ((double)SkillMasteries.MasteryInfo.EnchantedSummoningBonus(bc) / 100);

                    if (dispelChance > Utility.RandomDouble())
                    {
                        Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                        Effects.PlaySound(m, m.Map, 0x201);

                        m.Delete();
                    }
                    else
                    {
                        Caster.DoHarmful(m);

                        m.FixedEffect(0x3779, 10, 20);
                    }
                }
            }

            FinishSequence();
        }
    }
}
