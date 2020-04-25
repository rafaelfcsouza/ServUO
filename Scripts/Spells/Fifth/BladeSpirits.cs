using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Fifth
{
    public class BladeSpiritsSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Blade Spirits", "In Jux Hur Ylem",
            266,
            9040,
            false,
            Reagent.BlackPearl,
            Reagent.MandrakeRoot,
            Reagent.Nightshade);
        public BladeSpiritsSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;
        public override TimeSpan GetCastDelay()
        {
            return TimeSpan.FromTicks(base.GetCastDelay().Ticks * 3);
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if ((Caster.Followers + 2) > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        protected override Target CreateTarget() => new BladeSpiritsSpellTarget(this);

        public override void Target(object o)
        {
            IPoint3D p = o as IPoint3D;

            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                BaseCreature.Summon(new BladeSpirits(true), false, Caster, new Point3D(p), 0x212, TimeSpan.FromSeconds(120));
            }

            FinishSequence();
        }

        public class BladeSpiritsSpellTarget : SpellTarget<BladeSpiritsSpell, IPoint3D>
        {
            public BladeSpiritsSpellTarget(BladeSpiritsSpell owner)
                : base(owner, TargetFlags.None)
            {
            }

            protected override void OnTargetOutOfLOS(Mobile from, object o)
            {
                from.SendLocalizedMessage(501943); // Target cannot be seen. Try again.
                from.Target = new BladeSpiritsSpellTarget(Spell);
                from.Target.BeginTimeout(from, TimeoutTime - DateTime.UtcNow);
                Spell = null;
            }

        }
    }
}
