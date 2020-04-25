using Server.Spells.Fourth;
using Server.Targeting;
using System.Linq;

namespace Server.Spells.Sixth
{
    public class MassCurseSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Mass Curse", "Vas Des Sanct",
            218,
            9031,
            false,
            Reagent.Garlic,
            Reagent.Nightshade,
            Reagent.MandrakeRoot,
            Reagent.SulfurousAsh);
        public MassCurseSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Sixth;

        protected override Target CreateTarget() => new SpellTarget<MassCurseSpell, IPoint3D>(this, TargetFlags.None);

        public override void Target(object o)
        {
            IPoint3D p = o as IPoint3D;
            if (!Caster.CanSee(p))
            {
                Caster.SendLocalizedMessage(500237); // Target can not be seen.
            }
            else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
            {
                SpellHelper.Turn(Caster, p);
                SpellHelper.GetSurfaceTop(ref p);

                foreach (Mobile m in AcquireIndirectTargets(p, 2).OfType<Mobile>())
                {
                    CurseSpell.DoCurse(Caster, m, true);
                }
            }

            FinishSequence();
        }
    }
}
