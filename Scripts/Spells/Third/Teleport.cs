using Server.Items;
using Server.Regions;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class TeleportSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Teleport", "Rel Por",
            215,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);
        public TeleportSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;
        public override bool CheckCast()
        {
            if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom);
        }

        protected override Target CreateTarget() => new SpellTarget<TeleportSpell, IPoint3D>(this, TargetFlags.None);

        public override void Target(object o)
        {
            IPoint3D p = o as IPoint3D;
            IPoint3D orig = p;
            Map map = Caster.Map;

            SpellHelper.GetSurfaceTop(ref p);

            Point3D from = Caster.Location;
            Point3D to = new Point3D(p);

            if (Server.Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.TeleportFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, to, TravelCheckType.TeleportTo))
            {
            }
            else if (map == null || !map.CanSpawnMobile(p.X, p.Y, p.Z))
            {
                Caster.SendLocalizedMessage(501942); // That location is blocked.
            }
            else if (SpellHelper.CheckMulti(to, map))
            {
                Caster.SendLocalizedMessage(502831); // Cannot teleport to that spot.
            }
            else if (Region.Find(to, map).GetRegion(typeof(HouseRegion)) != null)
            {
                Caster.SendLocalizedMessage(502829); // Cannot teleport to that spot.
            }
            else if (CheckSequence())
            {
                SpellHelper.Turn(Caster, orig);

                Mobile m = Caster;

                m.Location = to;
                m.ProcessDelta();

                if (m.Player)
                {
                    Effects.SendLocationParticles(EffectItem.Create(from, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
                    Effects.SendLocationParticles(EffectItem.Create(to, m.Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
                }
                else
                {
                    m.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                }

                m.PlaySound(0x1FE);

                IPooledEnumerable eable = m.GetItemsInRange(0);

                foreach (Item item in eable)
                {
                    if (item is Server.Spells.Fifth.PoisonFieldSpell.InternalItem || item is Server.Spells.Fourth.FireFieldSpell.FireFieldItem)
                        item.OnMoveOver(m);
                }

                eable.Free();
            }

            FinishSequence();
        }
    }
}
