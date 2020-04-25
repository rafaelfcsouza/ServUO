using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Third
{
    public class TelekinesisSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Telekinesis", "Ort Por Ylem",
            203,
            9031,
            Reagent.Bloodmoss,
            Reagent.MandrakeRoot);

        public TelekinesisSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;

        protected override Target CreateTarget() => new TelekinesisTarget(this);

        public override void Target(object o)
        {
            if (o is Container container)
                Target(container);
            else if (o is ITelekinesisable telekinesisable)
                Target(telekinesisable);
            else
                Caster.SendLocalizedMessage(501857); // This spell won't work on that!
        }

        private void Target(ITelekinesisable obj)
        {
            if (CheckSequence())
            {
                SpellHelper.Turn(Caster, obj);

                obj.OnTelekinesis(Caster);
            }

            FinishSequence();
        }

        private void Target(Container item)
        {
            if (CheckSequence())
            {
                SpellHelper.Turn(Caster, item);

                object root = item.RootParent;

                if (!item.IsAccessibleTo(Caster))
                {
                    item.OnDoubleClickNotAccessible(Caster);
                }
                else if (!item.CheckItemUse(Caster, item))
                {
                }
                else if (root != null && root is Mobile && root != Caster)
                {
                    item.OnSnoop(Caster);
                }
                else if (item is Corpse corpse && !corpse.CheckLoot(Caster, null))
                {
                }
                else if (Caster.Region.OnDoubleClick(Caster, item))
                {
                    Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
                    Effects.PlaySound(item.Location, item.Map, 0x1F5);

                    item.OnItemUsed(Caster, item);
                }
            }

            FinishSequence();
        }

        public class TelekinesisTarget : SpellTarget<TelekinesisSpell, Container>
        {
            public TelekinesisTarget(TelekinesisSpell spell) : base(spell, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object o)
            {
                if (from is PlayerMobile) Spell.Invoke(o);
                else if (o is Container container)
                    Spell.Target(container);
                else if (o is ITelekinesisable telekinesisable)
                    Spell.Target(telekinesisable);
                else
                    from.SendLocalizedMessage(501857); // This spell won't work on that!
            }
        }
    }
}

namespace Server
{
    public interface ITelekinesisable : IPoint3D
    {
        void OnTelekinesis(Mobile from);
    }
}
