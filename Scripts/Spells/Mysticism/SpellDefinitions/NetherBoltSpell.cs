using Server.Targeting;
using System;

namespace Server.Spells.Mysticism
{
    public class NetherBoltSpell : MysticSpell
    {
        public override SpellCircle Circle => SpellCircle.First;

        private static readonly SpellInfo m_Info = new SpellInfo(
                "Nether Bolt", "In Corp Ylem",
                230,
                9022,
                Reagent.BlackPearl,
                Reagent.SulfurousAsh
            );

        public NetherBoltSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        public override bool DelayedDamage => true;
        public override bool DelayedDamageStacking => false;
        public override Type[] DelayDamageFamily => new Type[] { typeof(Server.Spells.First.MagicArrowSpell) };

        protected override Target CreateTarget() => new SpellTarget<NetherBoltSpell, IDamageable>(this, TargetFlags.Harmful);

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
                        source.MovingParticles(target, 0x36D4, 7, 0, false, true, 0x49A, 0, 0, 9502, 4019, 0x160);
                        source.PlaySound(0x211);
                    });
                }

                double damage = GetNewAosDamage(10, 1, 4, target);

                SpellHelper.Damage(this, target, damage, 0, 0, 0, 0, 0, 100, 0);

                Caster.MovingParticles(d, 0x36D4, 7, 0, false, true, 0x49A, 0, 0, 9502, 4019, 0x160);
                Caster.PlaySound(0x211);
            }

            FinishSequence();
        }
    }
}
