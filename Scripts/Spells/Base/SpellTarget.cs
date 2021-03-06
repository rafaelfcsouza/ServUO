using System;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells
{
    public class SpellTarget<TSpell, TTarget> : Target where TSpell : Spell
    {
        protected TSpell Spell;

        public SpellTarget(TSpell spell, TargetFlags flag)
            : base(10, false, flag)
        {
            Spell = spell;
        }

        public SpellTarget(TSpell spell, TargetFlags flag, bool allowTargetGround)
            : base(10, allowTargetGround, flag)
        {
            Spell = spell;
        }

        protected override void OnTarget(Mobile from, object o)
        {
            if (!(o is TTarget)) return;
            if (Spell.Caster is PlayerMobile) Spell.Invoke(o);
            else Spell.Target(o);
        }

        protected override void OnTargetFinish(Mobile from)
        {
            if (!(Spell.Caster is PlayerMobile))
            {
                Spell.FinishSequence();
            }
        }
    }
}
