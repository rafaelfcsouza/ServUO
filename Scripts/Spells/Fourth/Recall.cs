using Server.Engines.NewMagincia;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells.Necromancy;
using Server.Targeting;

namespace Server.Spells.Fourth
{
    public class RecallSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Recall", "Kal Ort Por",
            239,
            9031,
            Reagent.BlackPearl,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);

        private readonly RunebookEntry m_Entry;
        private readonly Runebook m_Book;
        private readonly VendorSearchMap m_SearchMap;
        private readonly AuctionMap m_AuctionMap;
        private bool _Casted;

        public bool NoSkillRequirement => (m_Book != null || m_AuctionMap != null || m_SearchMap != null) ||
                                          TransformationSpellHelper.UnderTransformation(Caster, typeof(WraithFormSpell));

        public RecallSpell(Mobile caster, Item scroll)
            : this(caster, scroll, null, null)
        {
        }

        public RecallSpell(Mobile caster, Item scroll, RunebookEntry entry, Runebook book)
            : base(caster, scroll, m_Info)
        {
            m_Entry = entry;
            m_Book = book;
        }

        public RecallSpell(Mobile caster, Item scroll, VendorSearchMap map)
            : base(caster, scroll, m_Info)
        {
            m_SearchMap = map;
        }

        public RecallSpell(Mobile caster, Item scroll, AuctionMap map)
            : base(caster, scroll, m_Info)
        {
            m_AuctionMap = map;
        }

        public override SpellCircle Circle => SpellCircle.Fourth;

        public override void GetCastSkills(out double min, out double max)
        {
            if (NoSkillRequirement) //recall using Runebook charge, wraith Caster or using vendor search map
                min = max = 0;
            else
                base.GetCastSkills(out min, out max);
        }

        protected override Target CreateTarget() => new RecallSpellTarget(this);

        public override void OnCast()
        {
            if (m_Entry == null && m_SearchMap == null && m_AuctionMap == null)
            {
                base.OnCast();
            }
            else if (!_Casted && Caster is PlayerMobile)
            {
                _Casted = true;
                Invoke();
            }
            else
            {
                Point3D loc;
                Map map;

                if (m_Entry != null)
                {
                    if (m_Entry.Type != RecallRuneType.Ship)
                    {
                        loc = m_Entry.Location;
                        map = m_Entry.Map;
                    }
                    else
                    {
                        Effect(m_Entry.Galleon);
                        return;
                    }
                }
                else if (m_SearchMap != null)
                {
                    loc = m_SearchMap.GetLocation(Caster);
                    map = m_SearchMap.GetMap();
                }
                else
                {
                    loc = m_AuctionMap.GetLocation(Caster);
                    map = m_AuctionMap.GetMap();
                }

                Effect(loc, map, true, false);
            }
        }

        public override bool CheckCast()
        {
            if (Server.Engines.VvV.VvVSigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                return false;
            }
            else if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                return false;
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                return false;
            }
            else if (Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                return false;
            }

            return SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom);
        }

        public void Effect(BaseGalleon galleon)
        {
            if (galleon == null)
            {
                Caster.SendLocalizedMessage(1116767); // The ship could not be located.
            }
            else if (galleon.Map == Map.Internal)
            {
                Caster.SendLocalizedMessage(1149569); // That ship is in dry dock.
            }
            else if (!galleon.HasAccess(Caster))
            {
                Caster.SendLocalizedMessage(1116617); // You do not have permission to board this ship.
            }
            else
            {
                Effect(galleon.GetMarkedLocation(), galleon.Map, false, true);
            }
        }

        public override void Target(object o)
        {
            if (o is RecallRune rune)
            {
                if (rune.Marked)
                {
                    if (rune.Type == RecallRuneType.Ship)
                    {
                        Effect(rune.Galleon);
                    }
                    else
                    {
                        Effect(rune.Target, rune.TargetMap, true);
                    }
                }
                else
                {
                    Caster.SendLocalizedMessage(501805); // That rune is not yet marked.
                }
            }
            else if (o is Runebook runebook)
            {
                RunebookEntry e = runebook.Default;

                if (e != null)
                {
                    if (e.Type == RecallRuneType.Ship)
                    {
                        Effect(e.Galleon);
                    }
                    else
                    {
                        Effect(e.Location, e.Map, true);
                    }
                }
                else
                {
                    Caster.SendLocalizedMessage(502354); // Target is not marked.
                }
            }
            else if (o is Key key && key.KeyValue != 0 && key.Link is BaseBoat)
            {
                BaseBoat boat = key.Link as BaseBoat;

                if (!boat.Deleted && boat.CheckKey(key.KeyValue))
                    Effect(boat.GetMarkedLocation(), boat.Map, false, true);
                else
                    Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall Caster that object.
            }
            else if (o is WritOfLease lease)
            {
                if (lease.RecallLoc != Point3D.Zero && lease.Facet != null && lease.Facet != Map.Internal)
                    Effect(lease.RecallLoc, lease.Facet, false);
                else
                    Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall Caster that object.
            }

            else
            {
                Caster.Send(new MessageLocalized(Caster.Serial, Caster.Body, MessageType.Regular, 0x3B2, 3, 502357, Caster.Name, "")); // I can not recall Caster that object.
            }
        }

        public void Effect(Point3D loc, Map map, bool checkMulti, bool isboatkey = false)
        {
            if (Server.Engines.VvV.VvVSigil.ExistsOn(Caster))
            {
                Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
            }
            else if (map == null)
            {
                Caster.SendLocalizedMessage(1005569); // You can not recall to another facet.
            }
            else if (!SpellHelper.CheckTravel(Caster, TravelCheckType.RecallFrom))
            {
            }
            else if (!SpellHelper.CheckTravel(Caster, map, loc, TravelCheckType.RecallTo))
            {
            }
            else if (map == Map.Felucca && Caster is PlayerMobile && ((PlayerMobile) Caster).Young)
            {
                Caster.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
            }
            else if (SpellHelper.RestrictRedTravel && Caster.Murderer && map.Rules != MapRules.FeluccaRules)
            {
                Caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
            }
            else if (Caster.Criminal)
            {
                Caster.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
            }
            else if (SpellHelper.CheckCombat(Caster))
            {
                Caster.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
            }
            else if (Misc.WeightOverloading.IsOverloaded(Caster))
            {
                Caster.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
            }
            else if (!map.CanSpawnMobile(loc.X, loc.Y, loc.Z) && !isboatkey)
            {
                Caster.SendLocalizedMessage(501025); // Something is blocking the location.
            }
            else if (checkMulti && SpellHelper.CheckMulti(loc, map) && !isboatkey)
            {
                Caster.SendLocalizedMessage(501025); // Something is blocking the location.
            }
            else if (m_Book != null && m_Book.CurCharges <= 0)
            {
                Caster.SendLocalizedMessage(502412); // There are no charges left on that item.
            }
            else if (Caster.Holding != null)
            {
                Caster.SendLocalizedMessage(1071955); // You cannot teleport while dragging an object.
            }
            else if (Engines.CityLoyalty.CityTradeSystem.HasTrade(Caster))
            {
                Caster.SendLocalizedMessage(1151733); // You cannot do that while carrying a Trade Order.
            }
            else if (CheckSequence())
            {
                BaseCreature.TeleportPets(Caster, loc, map, true);

                if (m_Book != null)
                    --m_Book.CurCharges;

                if (m_SearchMap != null)
                    m_SearchMap.OnBeforeTravel(Caster);

                if (m_AuctionMap != null)
                    m_AuctionMap.OnBeforeTravel(Caster);

                Caster.PlaySound(0x1FC);
                Caster.MoveToWorld(loc, map);
                Caster.PlaySound(0x1FC);
            }

            FinishSequence();
        }

        private class RecallSpellTarget : SpellTarget<RecallSpell, object>
        {
            public RecallSpellTarget(RecallSpell spell)
                : base(spell, TargetFlags.None)
            {
                spell.Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501029); // Select Marked item.
            }

            protected override void OnNonlocalTarget(Mobile from, object o)
            {
            }
        }
    }
}
