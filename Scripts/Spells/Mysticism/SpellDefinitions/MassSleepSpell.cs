using Server.Targeting;
using System;
using System.Linq;

namespace Server.Spells.Mysticism
{
    public class MassSleepSpell : MysticSpell
    {
        public override SpellCircle Circle => SpellCircle.Fifth;

        private static readonly SpellInfo m_Info = new SpellInfo(
                "Mass Sleep", "Vas Zu",
                230,
                9022,
                Reagent.Ginseng,
                Reagent.Nightshade,
                Reagent.SpidersSilk
            );

        public MassSleepSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        protected override Target CreateTarget() => new SpellTarget<MassSleepSpell, IPoint3D>(this, TargetFlags.None);

        public override void Target(object o)
        {
            IPoint3D p = o as IPoint3D;
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                Map map = Caster.Map;

                if (map == null)
                    return;

                foreach (Mobile m in AcquireIndirectTargets(p, 3).OfType<Mobile>())
                {
                    double duration = ((Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value) / 20) + 3;
                    duration -= GetResistSkill(m) / 10;

                    if (duration > 0)
                    {
                        Caster.DoHarmful(m);

                        SleepSpell.DoSleep(Caster, m, TimeSpan.FromSeconds(duration));
                    }
                }
            }

            FinishSequence();
        }
    }
}
