using System;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.First
{
    public class ReactiveArmorSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Reactive Armor", "Flam Sanct",
            236,
            9011,
            Reagent.Garlic,
            Reagent.SpidersSilk,
            Reagent.SulfurousAsh);

        private static readonly Dictionary<Mobile, ResistanceMod[]> _ResistanceTable = new Dictionary<Mobile, ResistanceMod[]>();
        private static readonly Dictionary<Mobile, InternalTimer> _TimerTable = new Dictionary<Mobile, InternalTimer>();
        public ReactiveArmorSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.First;

        public static void EndArmor(Mobile m)
        {
            if (!_ResistanceTable.ContainsKey(m)) return;

            ResistanceMod[] mods = _ResistanceTable[m];

            if (mods != null)
            {
                foreach (var t in mods)
                    m.RemoveResistanceMod(t);
            }

            _ResistanceTable.Remove(m);
            _TimerTable.Remove(m);
            BuffInfo.RemoveBuff(m, BuffIcon.ReactiveArmor);
        }

        protected override Target CreateTarget() => new SpellTarget<ReactiveArmorSpell, Mobile>(this, TargetFlags.Beneficial);

        public override void Target(object o)
        {
            /* The reactive armor spell increases the caster's physical resistance, while lowering the caster's elemental resistances.
            * 15 + (Inscription/20) Physcial bonus
            * -5 Elemental
            * The reactive armor spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
            * Reactive Armor, Protection, and Magic Reflection will stay on�even after logging out, even after dying�until you �turn them off� by casting them again.
            * (+20 physical -5 elemental at 100 Inscription)
            */
            if (CheckSequence())
            {
                Mobile targ = (Mobile) o;

                targ.PlaySound(0x1E9);
                targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

                var mods = new[]
                {
                    new ResistanceMod(ResistanceType.Physical, 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20)),
                    new ResistanceMod(ResistanceType.Fire, -5),
                    new ResistanceMod(ResistanceType.Cold, -5),
                    new ResistanceMod(ResistanceType.Poison, -5),
                    new ResistanceMod(ResistanceType.Energy, -5)
                };

                _ResistanceTable[targ] = mods;

                foreach (var t in mods)
                    targ.AddResistanceMod(t);

                int physresist = 15 + (int)(targ.Skills[SkillName.Inscribe].Value / 20);
                string args = String.Format("{0}\t{1}\t{2}\t{3}\t{4}", physresist, 5, 5, 5, 5);

                BuffInfo.AddBuff(targ, new BuffInfo(BuffIcon.ReactiveArmor, 1075812, 1075813, args.ToString()));

                TimeSpan length = SpellHelper.GetDuration(Caster, targ);
                _TimerTable[targ] = new InternalTimer(targ, length + TimeSpan.FromMilliseconds(50));
            }

            FinishSequence();

        }

        public static bool HasArmor(Mobile m)
        {
            return _TimerTable.ContainsKey(m);
        }

        private class InternalTimer : Timer
        {
            private Mobile Mobile { get; }

            public InternalTimer(Mobile m, TimeSpan duration)
                : base(duration)
            {
                Mobile = m;
                Start();
            }

            protected override void OnTick()
            {
                EndArmor(Mobile);
            }
        }
    }
}
