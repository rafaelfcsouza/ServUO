using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class Beacon : DamageableItem
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public BeaconItem Component { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DoBlast
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                    DoEffects();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool DoArea
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                    DoAreaAttack();
            }
        }

        public List<Item> Rubble { get; set; }

        public override bool DeleteOnDestroy => false;
        public override double IDChange => 0.50;

        public Beacon()
            : base(18212, 39299, 1)
        {
            Component = new BeaconItem(this);

            ResistBasePhys = 50;
            ResistBaseFire = 85;
            ResistBaseCold = 99;
            ResistBasePoison = 99;
            ResistBaseEnergy = 70;

            Level = ItemLevel.Easy; // Hard
        }

        public override void OnLocationChange(Point3D oldlocation)
        {
            base.OnLocationChange(oldlocation);

            if (Component != null)
                Component.Location = new Point3D(X - 1, Y, Z);
        }

        public override void OnMapChange()
        {
            base.OnMapChange();

            if (Component != null)
                Component.Map = Map;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            if (Component != null && !Component.Deleted)
                Component.Delete();
        }

        public override bool OnBeforeDestroyed()
        {
            List<Item> delete = new List<Item>();

            if (Rubble != null)
            {
                foreach (Item i in Rubble.Where(item => item.Z > Z))
                {
                    i.Delete();
                    delete.Add(i);
                }

                delete.ForEach(i => Rubble.Remove(i));
            }

            DoEffects();

            if (Component != null)
            {
                Component.ItemID = 1;
                Component.Visible = false;
            }

            Visible = false;

            delete.Clear();
            delete.TrimExcess();

            AddRubble(new Static(634), new Point3D(X - 2, Y, Z));
            AddRubble(new Static(633), new Point3D(X - 2, Y + 1, Z));

            AddRubble(new Static(635), new Point3D(X + 2, Y - 2, Z));
            AddRubble(new Static(632), new Point3D(X + 3, Y - 2, Z));

            AddRubble(new Static(634), new Point3D(X + 2, Y, X));
            AddRubble(new Static(633), new Point3D(X + 2, Y + 1, Z));
            return true;
        }

        private void DoEffects()
        {
            int range = 8;

            //Flamestrikes
            for (int i = 0; i < range; i++)
            {
                Timer.DelayCall(TimeSpan.FromMilliseconds(i * 50), o =>
                    {
                        Server.Misc.Geometry.Circle2D(Location, Map, o, (pnt, map) =>
                        {
                            Effects.SendLocationEffect(pnt, map, 0x3709, 30, 20, 0, 2);
                        });
                    }, i);
            }

            //Explosions
            Timer.DelayCall(TimeSpan.FromMilliseconds(1000), () =>
            {
                for (int i = 0; i < range + 3; i++)
                {
                    Server.Misc.Geometry.Circle2D(Location, Map, i, (pnt, map) =>
                    {
                        Effects.SendLocationEffect(pnt, map, 0x36CB, 14, 10, 2498, 2);
                    });
                }
            });

            // Black explosions
            Timer.DelayCall(TimeSpan.FromMilliseconds(1400), () =>
            {
                for (int i = 0; i < range - 3; i++)
                {
                    Timer.DelayCall(TimeSpan.FromMilliseconds(i * 50), o =>
                    {
                        Server.Misc.Geometry.Circle2D(Location, Map, o, (pnt, map) =>
                        {
                            Effects.SendLocationEffect(pnt, map, Utility.RandomBool() ? 14000 : 14013, 14, 20, 2018, 0);
                        });
                    }, i);
                }
            });
        }

        public override void OnIDChange(int oldID)
        {
            if (ItemID == IDHalfHits && oldID == IDStart)
            {
                AddRubble(new Static(6571), new Point3D(X, Y + 1, Z + 42));
                AddRubble(new Static(3118), new Point3D(X - 1, Y + 1, Z));
                AddRubble(new Static(3118), new Point3D(X + 1, Y - 1, Z));
            }
        }

        public override void OnDamage(int amount, Mobile from, bool willkill)
        {
            base.OnDamage(amount, from, willkill);

            if (ItemID == IDHalfHits && Hits <= (HitsMax * .25))
            {
                AddRubble(new Static(14732), new Point3D(X - 1, Y + 1, Z));
                AddRubble(new Static(14742), new Point3D(X + 1, Y - 1, Z));
                AddRubble(new Static(14742), new Point3D(X, Y, Z + 63));

                AddRubble(new Static(6571), new Point3D(X + 1, Y + 1, Z + 42));
                AddRubble(new Static(6571), new Point3D(X + 1, Y, Z + 59));

                OnHalfDamage();

                ItemID = 39300;
            }
            else if (CheckAreaDamage(from, amount))
            {
                DoAreaAttack();
            }
        }

        public virtual bool CheckAreaDamage(Mobile from, int amount)
        {
            return 0.02 > Utility.RandomDouble();
        }

        public virtual void OnHalfDamage()
        {
        }

        private void AddRubble(Item i, Point3D p)
        {
            i.MoveToWorld(p, Map);

            if (Rubble == null)
                Rubble = new List<Item>();

            Rubble.Add(i);
        }

        public override void Delete()
        {
            base.Delete();

            if (Rubble != null)
            {
                List<Item> rubble = new List<Item>(Rubble);

                rubble.ForEach(i => i.Delete());
                rubble.ForEach(i => Rubble.Remove(i));

                rubble.Clear();
                rubble.TrimExcess();
            }
        }

        public virtual void DoAreaAttack()
        {
            List<Mobile> list = new List<Mobile>();
            IPooledEnumerable eable = Map.GetMobilesInRange(Location, 8);

            foreach (Mobile m in eable)
            {
                if (m.AccessLevel > AccessLevel.Player)
                    continue;

                if (m is PlayerMobile || (m is BaseCreature && ((BaseCreature)m).GetMaster() is PlayerMobile))
                    list.Add(m);
            }

            eable.Free();

            list.ForEach(m =>
            {
                m.BoltEffect(0);
                AOS.Damage(m, null, Utility.RandomMinMax(80, 90), 0, 0, 0, 0, 100);

                if (m.NetState != null)
                    m.PrivateOverheadMessage(Server.Network.MessageType.Regular, 1154, 1154552, m.NetState); // *The beacon blasts a surge of energy at you!"
            });

            ColUtility.Free(list);
        }

        public Beacon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Component);

            writer.Write(Rubble == null ? 0 : Rubble.Count);

            if (Rubble != null)
                Rubble.ForEach(i => writer.Write(i));
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Component = reader.ReadItem() as BeaconItem;

            if (Component != null)
                Component.Beacon = this;

            int count = reader.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Item item = reader.ReadItem();
                if (item != null)
                {
                    if (Rubble == null)
                        Rubble = new List<Item>();

                    Rubble.Add(item);
                }
            }
        }
    }

    public class BeaconItem : Item
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public Beacon Beacon { get; set; }

        public override bool ForceShowProperties => true;

        public BeaconItem(Beacon beacon)
            : base(18223)
        {
            Movable = false;
            Beacon = beacon;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Beacon != null)
                Beacon.OnDoubleClick(from);
        }

        public BeaconItem(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
