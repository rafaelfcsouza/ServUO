using Server.Items;
using Server.Misc;
using Server.Targeting;
using System;
using Server.Mobiles;

namespace Server.Spells.Fifth
{
    public class DispelFieldSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Dispel Field", "An Grav",
            206,
            9002,
            Reagent.BlackPearl,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh,
            Reagent.Garlic);

        public DispelFieldSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;

        protected override Target CreateTarget() => new DispelFieldSpellTarget(this);

        public override void Target(object o)
        {
            Item item = o as Item;

            Type t = item?.GetType();

            if (!Caster.CanSee(item))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (!t.IsDefined(typeof(DispellableFieldAttribute), false))
            {
                Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
            }
            else if (item is Moongate moongate && !moongate.Dispellable)
            {
                Caster.SendLocalizedMessage(1005047); // That magic is too chaotic
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 20, 5042);
                Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x201);

                item.Delete();
            }

            FinishSequence();
        }

        public class DispelFieldSpellTarget : SpellTarget<DispelFieldSpell, Item>
        {
            public DispelFieldSpellTarget(DispelFieldSpell owner)
                : base(owner, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (!(o is Item)) Spell.Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
                else if (Spell.Caster is PlayerMobile) Spell.Invoke(o);
                else if (o is Item item)
                {
                    Spell.Target(item);
                }
            }
        }
    }
}
