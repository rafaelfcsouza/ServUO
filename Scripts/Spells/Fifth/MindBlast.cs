using Server.Targeting;
using System;

namespace Server.Spells.Fifth
{
    public class MindBlastSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mind Blast", "Por Corp Wis",
            218,
            9002,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade,
            Reagent.SulfurousAsh);
        public MindBlastSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
            m_Info.LeftHandEffect = m_Info.RightHandEffect = 9002;
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

        protected override Target CreateTarget() => new SpellTarget<MindBlastSpell, Mobile>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            Mobile m = o as Mobile;
            if (!Caster.CanSee(m))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else
            {
                if (Caster.CanBeHarmful(m) && CheckSequence())
                {
                    Mobile from = Caster, target = m;

                    SpellHelper.Turn(from, target);
                    SpellHelper.CheckReflect((int)Circle, ref from, ref target);

                    int intel = Math.Min(200, Caster.Int);

                    int damage = (int)((Caster.Skills[SkillName.Magery].Value + intel) / 5) + Utility.RandomMinMax(2, 6);

                    if (damage > 60)
                        damage = 60;

                    Timer.DelayCall(TimeSpan.FromSeconds(1.0),
                        new TimerStateCallback(AosDelay_Callback),
                        new object[] { Caster, target, m, damage });
                }
            }

            FinishSequence();
        }

        public override double GetSlayerDamageScalar(Mobile target)
        {
            return 1.0; //This spell isn't affected by slayer spellbooks
        }

        private void AosDelay_Callback(object state)
        {
            object[] states = (object[])state;
            Mobile caster = (Mobile)states[0];
            Mobile target = (Mobile)states[1];
            Mobile defender = (Mobile)states[2];
            int damage = (int)states[3];

            if (caster.HarmfulCheck(defender))
            {
                target.FixedParticles(0x374A, 10, 15, 5038, 1181, 2, EffectLayer.Head);
                target.PlaySound(0x213);

                SpellHelper.Damage(this, target, Utility.RandomMinMax(damage, damage + 4), 0, 0, 100, 0, 0);
            }
        }
    }
}
