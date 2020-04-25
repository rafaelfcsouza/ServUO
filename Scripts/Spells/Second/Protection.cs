using System;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Second
{
    public class ProtectionSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Protection", "Uus Sanct",
            236,
            9011,
            Reagent.Garlic,
            Reagent.Ginseng,
            Reagent.SulfurousAsh);

        public static Dictionary<Mobile, double> Registry { get; } = new Dictionary<Mobile, double>();

        private static readonly Dictionary<Mobile, object> _ModsTable = new Dictionary<Mobile, object>();
        private static readonly Dictionary<Mobile, InternalTimer> _TimerTable = new Dictionary<Mobile, InternalTimer>();

        public override SpellCircle Circle => SpellCircle.Second;

        public ProtectionSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public static void StartProtection(Mobile caster, Mobile target, bool archprotection)
        {
            /* Players under the protection spell effect can no longer have their spells "disrupted" when hit.
            * Players under the protection spell have decreased physical resistance stat value (-15 + (Inscription/20),
            * a decreased "resisting spells" skill value by -35 + (Inscription/20),
            * and a slower casting speed modifier (technically, a negative "faster cast speed") of 2 points.
            * The protection spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
            * Reactive Armor, Protection, and Magic Reflection will stay on—even after logging out,
            * even after dying—until you “turn them off” by casting them again.
            */

            target.PlaySound(0x1E9);
            target.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

            var mods = new object[]
            {
                new ResistanceMod(ResistanceType.Physical, -15 + Math.Min((int) (caster.Skills[SkillName.Inscribe].Value / 20), 15)),
                new DefaultSkillMod(SkillName.MagicResist, true, -35 + Math.Min((int) (caster.Skills[SkillName.Inscribe].Value / 20), 35))
            };

            _ModsTable[target] = mods;
            Registry[target] = 100.0;

            target.AddResistanceMod((ResistanceMod) mods[0]);
            target.AddSkillMod((SkillMod) mods[1]);

            int physloss = -15 + (int) (caster.Skills[SkillName.Inscribe].Value / 20);
            int resistloss = -35 + (int) (caster.Skills[SkillName.Inscribe].Value / 20);
            string args = String.Format("{0}\t{1}", physloss, resistloss);
            BuffInfo.AddBuff(target, new BuffInfo(archprotection ? BuffIcon.ArchProtection : BuffIcon.Protection, archprotection ? 1075816 : 1075814, 1075815, args.ToString()));

            _TimerTable[target] = new InternalTimer(caster);
        }

        public static void EndProtection(Mobile m)
        {
            if (_TimerTable.ContainsKey(m))
            {
                // m.PlaySound(0x1ED);
                // m.FixedParticles(0x375A, 9, 20, 5016, EffectLayer.Waist);

                object[] mods = (object[]) _ModsTable[m];

                Registry.Remove(m);

                _ModsTable.Remove(m);
                _TimerTable.Remove(m);

                m.RemoveResistanceMod((ResistanceMod) mods[0]);
                m.RemoveSkillMod((SkillMod) mods[1]);

                BuffInfo.RemoveBuff(m, BuffIcon.Protection);
                BuffInfo.RemoveBuff(m, BuffIcon.ArchProtection);
            }
        }

        public override bool CheckCast()
        {
            return true;
        }

        public override void OnCast()
        {
            if (Caster is PlayerMobile)
            {
                if (PreTarget != null)
                {
                    if (CheckSequence())
                        StartProtection(Caster, (Mobile) PreTarget, false);

                    FinishSequence();
                }
                else Caster.Target = new SpellTarget<ProtectionSpell, Mobile>(this, TargetFlags.Beneficial);
            }
            else
            {
                if (CheckSequence())
                    StartProtection(Caster, Caster, false);

                FinishSequence();
            }
        }

        #region SA

        public static bool HasProtection(Mobile m)
        {
            return _ModsTable.ContainsKey(m);
        }

        #endregion

        private class InternalTimer : Timer
        {
            private readonly Mobile m_Caster;

            public InternalTimer(Mobile caster)
                : base(TimeSpan.Zero)
            {
                double val = caster.Skills[SkillName.Magery].Value * 2.0;
                if (val < 15)
                    val = 15;
                else if (val > 240)
                    val = 240;

                m_Caster = caster;
                Delay = TimeSpan.FromSeconds(val);
                Priority = TimerPriority.OneSecond;
                Start();
            }

            protected override void OnTick()
            {
                EndProtection(m_Caster);
            }
        }
    }
}
