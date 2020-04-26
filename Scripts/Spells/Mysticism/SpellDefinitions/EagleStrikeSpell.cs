using Server.Targeting;
using System;

namespace Server.Spells.Mysticism
{
    public class EagleStrikeSpell : MysticSpell
    {
        public override SpellCircle Circle => SpellCircle.Third;
        public override bool DelayedDamage => true;
        public override bool DelayedDamageStacking => false;

        private static readonly SpellInfo m_Info = new SpellInfo(
                "Eagle Strike", "Kal Por Xen",
                230,
                9022,
                Reagent.Bloodmoss,
                Reagent.Bone,
                Reagent.MandrakeRoot,
                Reagent.SpidersSilk
            );

        public EagleStrikeSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        protected override Target CreateTarget() => new SpellTarget<EagleStrikeSpell, IDamageable>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            if (!(o is IDamageable d))
            {
                return;
            }
            else if (CheckHSequence(d))
            {
                IDamageable target = d;
                IDamageable source = Caster;

                SpellHelper.Turn(Caster, target);

                if (HasDelayContext(target))
                {
                    DoHurtFizzle();
                    return;
                }

                if (SpellHelper.CheckReflect((int)Circle, ref source, ref target))
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                    {
                        source.MovingEffect(target, 0x407A, 8, 1, false, true, 0, 0);
                        source.PlaySound(0x2EE);
                    });
                }

                Caster.MovingEffect(d, 0x407A, 8, 1, false, true, 0, 0);
                Caster.PlaySound(0x2EE);

                Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                {
                    Caster.PlaySound(0x64D);
                });

                SpellHelper.Damage(this, target, GetNewAosDamage(19, 1, 5, target), 0, 0, 0, 0, 100);
            }

            FinishSequence();
        }
    }
}
