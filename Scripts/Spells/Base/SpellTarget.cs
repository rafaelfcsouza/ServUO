using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells
{
    public class SpellTarget<S, T> : Target where S: Spell where T: IEntity
    {
        private readonly S _Spell;
        public SpellTarget(S spell, TargetFlags flag)
            : base(10, false, flag)
        {
            _Spell = spell;
        }

        protected override void OnTarget(Mobile from, object o)
        {
            if (!(o is T)) return;
            if (_Spell.Caster is PlayerMobile) _Spell.Invoke(o);
            else _Spell.Target((T) o);
        }

        protected override void OnTargetFinish(Mobile from)
        {
            if (!(_Spell.Caster is PlayerMobile))
            {
                _Spell.FinishSequence();
            }
        }
    }
}
