using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class MagicTrapSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Trap", "In Jux",
            212,
            9001,
            Reagent.Garlic,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);

        public MagicTrapSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Second;

        protected override Target CreateTarget() => new MagicTrapTarget(this);

        public override void Target(object o)
        {
            var item = (TrapableContainer) o;
            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (item.TrapType != TrapType.None && item.TrapType != TrapType.MagicTrap)
            {
                base.DoFizzle();
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                item.TrapType = TrapType.MagicTrap;
                item.TrapPower = Utility.RandomMinMax(10, 50);
                item.TrapLevel = 0;

                Point3D loc = item.GetWorldLocation();

                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X + 1, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y - 1, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X - 1, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y + 1, loc.Z), item.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 9502);
                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc.X, loc.Y, loc.Z), item.Map, EffectItem.DefaultDuration), 0, 0, 0, 5014);

                Effects.PlaySound(loc, item.Map, 0x1EF);
            }

            FinishSequence();
        }

        private class MagicTrapTarget : SpellTarget<MagicTrapSpell, Mobile>
        {
            public MagicTrapTarget(MagicTrapSpell magicTrapSpell) : base(magicTrapSpell, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!(o is TrapableContainer))
                {
                    from.SendMessage("You can't trap that");
                    return;
                }

                if (Spell.Caster is PlayerMobile) Spell.Invoke(o);
                else Spell.Target((TrapableContainer) o);
            }
        }
    }
}
