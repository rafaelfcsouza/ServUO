using Server.Targeting;
using System;

namespace Server.Spells.Mysticism
{
    public class BombardSpell : MysticSpell
    {
        public override SpellCircle Circle => SpellCircle.Sixth;
        public override bool DelayedDamage => true;
        public override bool DelayedDamageStacking => false;

        private static readonly SpellInfo m_Info = new SpellInfo(
                "Bombard", "Corp Por Ylem",
                230,
                9022,
                Reagent.Bloodmoss,
                Reagent.Garlic,
                Reagent.SulfurousAsh,
                Reagent.DragonBlood
            );

        public BombardSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        protected override Target CreateTarget() => new SpellTarget<BombardSpell, Mobile>(this, TargetFlags.Harmful);

        public override void Target(object o)
        {
            IDamageable d = o as IDamageable;

            if (d == null)
            {
                return;
            }

            if (CheckHSequence(d))
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
                    Server.Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
                    {
                        source.MovingEffect(target, 0x1363, 12, 1, false, true, 0, 0);
                        source.PlaySound(0x64B);
                    });
                }

                Caster.MovingEffect(d, 0x1363, 12, 1, false, true, 0, 0);
                Caster.PlaySound(0x64B);

                SpellHelper.Damage(this, target, GetNewAosDamage(40, 1, 5, target), 100, 0, 0, 0, 0);

                if (target is Mobile)
                {
                    Timer.DelayCall(TimeSpan.FromMilliseconds(1200), () =>
                    {
                        if (!CheckResisted((Mobile)target))
                        {
                            int secs = (int)((GetDamageSkill(Caster) / 10) - (GetResistSkill((Mobile)target) / 10));

                            if (secs < 0)
                                secs = 0;

                            ((Mobile)target).Paralyze(TimeSpan.FromSeconds(secs));
                        }
                    });
                }
            }

            FinishSequence();
        }
    }
}
