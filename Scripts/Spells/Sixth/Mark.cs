using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Sixth
{
    public class MarkSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mark", "Kal Por Ylem",
            218,
            9002,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);

        public MarkSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Sixth;

        protected override Target CreateTarget() => new MarkSpellTarget(this);

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            return SpellHelper.CheckTravel(Caster, TravelCheckType.Mark);
        }

        public override void Target(object o)
        {
            RecallRune rune = o as RecallRune;
            BaseBoat boat = BaseBoat.FindBoatAt(Caster.Location, Caster.Map);

            if (!Caster.CanSee(rune))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.Mark))
            {
            }
            else if (boat == null && SpellHelper.CheckMulti(Caster.Location, Caster.Map, false))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (boat != null && !(boat is BaseGalleon))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501800); // You cannot mark an object at that location.
            }
            else if (!rune.IsChildOf(Caster.Backpack))
            {
                Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062422); // You must have this rune in your backpack in order to mark it.
            }
            else if (CheckSequence())
            {
                rune.Mark(Caster);

                Caster.PlaySound(0x1FA);
                Effects.SendLocationEffect(Caster, Caster.Map, 14201, 16);
            }

            FinishSequence();
        }

        private class MarkSpellTarget : SpellTarget<MarkSpell, RecallRune>
        {
            public MarkSpellTarget(MarkSpell spell)
                : base(spell, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (Spell.Caster is PlayerMobile) Spell.Invoke(o);
                else if (o is RecallRune rune)
                {
                    Spell.Target(rune);
                }
                else
                {
                    from.Send(new MessageLocalized(from.Serial, from.Body, MessageType.Regular, 0x3B2, 3, 501797, from.Name, "")); // I cannot mark that object.
                }
            }
        }
    }
}
