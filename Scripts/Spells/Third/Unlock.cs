using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Spells.Third
{
    public interface IMageUnlockable
    {
        void OnMageUnlock(Mobile Caster);
    }

    public class UnlockSpell : MagerySpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Unlock Spell", "Ex Por",
            215,
            9001,
            Reagent.Bloodmoss,
            Reagent.SulfurousAsh);

        public UnlockSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        public override SpellCircle Circle => SpellCircle.Third;

        protected override Target CreateTarget() => new SpellTarget<UnlockSpell, IPoint3D>(this, TargetFlags.None);

        public override void Target(object o)
        {
            if (!(o is IPoint3D loc))
                return;

            if (CheckSequence())
            {
                SpellHelper.Turn(Caster, o);

                Effects.SendLocationParticles(EffectItem.Create(new Point3D(loc), Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5024);

                Effects.PlaySound(loc, Caster.Map, 0x1FF);

                if (o is Mobile)
                    Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503101); // That did not need to be unlocked.
                else if (o is IMageUnlockable unlockable)
                    unlockable.OnMageUnlock(Caster);
                else if (!(o is LockableContainer))
                    Caster.SendLocalizedMessage(501666); // You can't unlock that!
                else
                {
                    LockableContainer cont = (LockableContainer) o;

                    if (Multis.BaseHouse.CheckSecured(cont))
                        Caster.SendLocalizedMessage(503098); // You cannot cast this on a secure item.
                    else if (!cont.Locked)
                        Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503101); // That did not need to be unlocked.
                    else if (cont.LockLevel == 0)
                        Caster.SendLocalizedMessage(501666); // You can't unlock that!
                    else if (cont is TreasureMapChest chest && chest.Level > 2)
                        Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503099); // My spell does not seem to have an effect on that lock.
                    else
                    {
                        int level;
                        int reqSkill;

                        if (cont is TreasureMapChest mapChest && TreasureMapInfo.NewSystem)
                        {
                            level = (int) Caster.Skills[SkillName.Magery].Value;

                            switch (mapChest.Level)
                            {
                                default:
                                    reqSkill = 50;
                                    break;
                                case 1:
                                    reqSkill = 80;
                                    break;
                                case 2:
                                    reqSkill = 100;
                                    break;
                            }
                        }
                        else
                        {
                            level = (int) (Caster.Skills[SkillName.Magery].Value * 0.8) - 4;
                            reqSkill = cont.RequiredSkill;
                        }

                        if (level >= reqSkill)
                        {
                            cont.Locked = false;

                            if (cont.LockLevel == -255)
                                cont.LockLevel = cont.RequiredSkill - 10;
                        }
                        else
                            Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 503099); // My spell does not seem to have an effect on that lock.
                    }
                }
            }

            FinishSequence();
        }
    }
}
