using System;
using System.Collections;
using System.Collections.Generic;
using Server.Targeting;

namespace Server.Spells.Fifth
{
    public class MagicReflectSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Magic Reflection", "In Jux Sanct",
            242,
            9012,
            Reagent.Garlic,
            Reagent.MandrakeRoot,
            Reagent.SpidersSilk);

        public static readonly Dictionary<Mobile, MagicReflectionTimer> _TimerTable = new Dictionary<Mobile, MagicReflectionTimer>();
        private static readonly Dictionary<Mobile, ResistanceMod> _ModTable = new Dictionary<Mobile, ResistanceMod>();

        private static Func<Mobile, int> CalcularePhysicalModification = m => (int) (-25 + m.Skills[SkillName.Inscribe].Value / 20);
        private static Func<Mobile, TimeSpan> CalculateTimer = m => {
            var val = m.Skills[SkillName.Magery].Value * 2.0;
            if (val < 15) val = 15;
            else if (val > 240) val = 240;
            return TimeSpan.FromSeconds(val);
        };

        protected override bool UsesTarget => false;

        public MagicReflectSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Fifth;
        public static void EndReflect(Mobile m)
        {
            if (_TimerTable.ContainsKey(m))
            {
                m.RemoveResistanceMod(_ModTable[m]);
                m.MagicDamageAbsorb = 0;

                _TimerTable.Remove(m);
                _ModTable.Remove(m);
                BuffInfo.RemoveBuff(m, BuffIcon.MagicReflection);
            }
        }

        public override void OnCast()
        {
            /* The magic reflection spell reflects first spell and decreases the caster's physical resistance
            * Physical decrease = 25 - (Inscription/20).
            * Reactive Armor, Protection, and Magic Reflection will stay on�even after logging out, even after dying�until you �turn them off� by casting them again.
            */
            if (CheckSequence())
            {
                Mobile targ = Caster;
                targ.PlaySound(0x1E9);
                targ.FixedParticles(0x375A, 10, 15, 5037, EffectLayer.Waist);


                var timer = CalculateTimer(Caster);

                targ.MagicDamageAbsorb = 15;

                ResistanceMod physiMod = new ResistanceMod(ResistanceType.Physical, CalcularePhysicalModification(targ));

                _ModTable[targ] = physiMod;
                _TimerTable[targ] = new MagicReflectionTimer(targ, timer);

                string buffFormat = $"{physiMod}";

                BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.MagicReflection, 1075817, timer, targ, buffFormat));
            }

            FinishSequence();

        }

        public static bool HasReflect(Mobile m)
        {
            return _TimerTable.ContainsKey(m);
        }

        public class MagicReflectionTimer : Timer
        {
            private readonly Mobile _Target;

            public MagicReflectionTimer(Mobile target, TimeSpan delay)
                : base(TimeSpan.Zero)
            {

                _Target = target;
                Delay = delay;
                Priority = TimerPriority.OneSecond;
                Start();
            }

            protected override void OnTick()
            {
                EndReflect(_Target);
            }
        }
    }
}
