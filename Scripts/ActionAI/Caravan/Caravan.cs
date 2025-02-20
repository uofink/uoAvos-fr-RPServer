﻿using Server;
using Server.Items;

using System;
using System.Collections.Generic;

namespace Server
{
    public class ActionAI_CaravanPaths
    {
        public static List<Point3D> TownStoppingPoint = new List<Point3D>
        {
            (new Point3D(2501, 560, 0))
        };

        public static List<Point3D> Path_Armorer = new List<Point3D>
        {
            (new Point3D(2538, 622, 0)),
            (new Point3D(2524, 622, 0)),
            (new Point3D(2518, 620, 0)),
            (new Point3D(2513, 618, 0)),
            (new Point3D(2513, 605, 0)),
            (new Point3D(2513, 595, 0)),
            (new Point3D(2517, 591, 0)),
            (new Point3D(2517, 574, 0)),
            (new Point3D(2517, 562, 0)),
            (new Point3D(2503, 562, 0)),
            (new Point3D(2501, 560, 0))

        };
    }
}

namespace Server.Mobiles
{
    public class ActionAI_CaravanVendor : BaseVendor
    {
        public PathFollower m_Path;
        private int m_Index;
        private WayPoint m_waypointFirst;
        private List<Point3D> m_MobilePath;
        private Mobile m_Packhorse;

        private bool m_ReversesPathAtEnd;
        private bool m_IsReversing;

        private List<SBInfo> m_SBInfos = new List<SBInfo>();
        protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

        public virtual bool InitialInnocent { get { return true; } }
        public override bool IsInvulnerable { get { return false; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WayPoint waypointFirst
        {
            get { return m_waypointFirst; }
            set { m_waypointFirst = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public List<Point3D> MobilePath
        {
            get { return m_MobilePath; }
            set { m_MobilePath = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Packhorse
        {
            get { return m_Packhorse; }
            set { m_Packhorse = value; }
        }

        
        [CommandProperty(AccessLevel.GameMaster)]
        public bool ReversesPathAtEnd
        {
            get { return m_ReversesPathAtEnd; }
            set { m_ReversesPathAtEnd = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsReversing
        {
            get { return m_IsReversing; }
            set { m_IsReversing = value; }
        }


        public override bool PlayerRangeSensitive { get { return false; } }

        [Constructable]
        public ActionAI_CaravanVendor() : base("[the Caravan Vendor]")
        {
            Body = 0x190;

            m_ReversesPathAtEnd = true;
            m_IsReversing = false;

            SetStr(600, 800);
            SetDex(91, 510);
            SetInt(161, 585);

            SetHits(222, 308);

            SetDamage(23, 46);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 60, 80);
            SetResistance(ResistanceType.Fire, 60, 80);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 70, 80);

            SetSkill(SkillName.EvalInt, 97.5, 150.0);
            SetSkill(SkillName.Fencing, 92.5, 135.0);
            SetSkill(SkillName.Macing, 92.5, 135.0);
            SetSkill(SkillName.Magery, 92.5, 135.0);
            SetSkill(SkillName.Meditation, 97.5, 300.0);
            SetSkill(SkillName.MagicResist, 97.5, 130.0);
            SetSkill(SkillName.Swords, 92.5, 135.0);
            SetSkill(SkillName.Tactics, 92.5, 135.0);
            SetSkill(SkillName.Wrestling, 92.5, 135.0);

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), SetPath);
        }

        private void SetPath()
        {
            ActionAI_CaravanGuard guard = new ActionAI_CaravanGuard();
            guard.MoveToWorld(new Point3D(this.X + 2, this.Y + 2, this.Z), this.Map);
            guard.Protecting = this;
            // Set pet to "tamed"/owned by the player
            guard.Controlled = true;
            guard.ControlMaster = this;
            // Set pet to auto follow the player
            guard.ControlOrder = OrderType.Guard;
            guard.ControlTarget = this;
            /******/

            CaravanPackHorse packy = new CaravanPackHorse();
            packy.MoveToWorld(new Point3D(this.X - 2, this.Y + 2, this.Z), this.Map);
            m_Packhorse = packy;
            // Set pet to "tamed"/owned by the player
            packy.Controlled = true;
            packy.ControlMaster = this;
            // Set pet to auto follow the player
            packy.ControlOrder = OrderType.Follow;
            packy.ControlTarget = this;
            /******/

            if (m_Packhorse != null)
            {
                ActionAI_CaravanGuard guard2 = new ActionAI_CaravanGuard();
                guard2.MoveToWorld(new Point3D(m_Packhorse.X + 2, m_Packhorse.Y + 2, m_Packhorse.Z), this.Map);
                guard2.Protecting = ((BaseCreature)m_Packhorse);
                // Set pet to "tamed"/owned by the player
                guard2.Controlled = true;
                guard2.ControlMaster = m_Packhorse;
                // Set pet to auto follow the player
                guard2.ControlOrder = OrderType.Guard;
                guard2.ControlTarget = m_Packhorse;
                /******/
            }

            CurrentSpeed = 0.2;
            m_MobilePath = ActionAI_CaravanPaths.Path_Armorer;

            m_waypointFirst = new WayPoint();
            m_waypointFirst.MoveToWorld(m_MobilePath[0], Map);

            CurrentWayPoint = m_waypointFirst;

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), MoveWayPoint);
        }

        public override void OnThink()
        {

            if (this.FocusMob != null)
            {
                return;
            }
            else if (this.FocusMob == null)
            {
                CurrentSpeed = 0.2;
            }
            if (m_waypointFirst != null && m_waypointFirst.Location == this.Location)
            {
                CurrentSpeed = 0.2;
                Timer.DelayCall(TimeSpan.FromSeconds(0.2), MoveWayPoint);
            }

            

        }

        public void MoveWayPoint()
        {
            if (waypointFirst != null && (waypointFirst.X == Location.X && waypointFirst.Y == Location.Y))
            {
                CantWalk = false;

                if(IsReversing)
                {
                    if ((m_Index - 1) >= 0)
                        ReversePath();
                    else
                        ForwardPath();
                }

                else
                {
                    if ((m_Index + 1) < m_MobilePath.Count)
                        ForwardPath();
                    else
                        ReversePath();
                }
            }
        }

        private void ForwardPath()
        {
            IsReversing = false;
            m_Index++;

            NewPoint();
        }

        private void ReversePath()
        {
            IsReversing = true;
            m_Index--;
            
            NewPoint();
        }

        private void NewPoint()
        {
            waypointFirst.Location = m_MobilePath[m_Index];

            StoppingPoint();
            
            CurrentWayPoint = waypointFirst;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

        private void StoppingPoint()
        {
            if (ActionAI_CaravanPaths.TownStoppingPoint.Contains(this.Location))
            {
                CantWalk = true;

                Timer.DelayCall(TimeSpan.FromSeconds(45.0), delegate{ CantWalk = false; });
            }
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Add(new SBProvisioner());

            if (IsTokunoVendor)
                m_SBInfos.Add(new SBSEHats());
        }

        public ActionAI_CaravanVendor(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version

            // 1
            writer.Write(m_ReversesPathAtEnd);
            writer.Write(m_IsReversing);

            // 0
            writer.Write(m_waypointFirst);
            writer.Write(m_Packhorse);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch(version)
            {
                case 1:
                    {
                        m_ReversesPathAtEnd = reader.ReadBool();
                        m_IsReversing = reader.ReadBool();

                        break;
                    }
            }

            // 0 
            m_waypointFirst = reader.ReadItem() as WayPoint;
            m_Packhorse = reader.ReadMobile();
            m_MobilePath = ActionAI_CaravanPaths.Path_Armorer;

            Timer.DelayCall(TimeSpan.FromSeconds(1.0), MoveWayPoint);
        }
    }

    public class ActionAI_CaravanGuard : BaseCreature
    {
        private BaseCreature m_Protecting;

        [CommandProperty(AccessLevel.GameMaster)]
        public BaseCreature Protecting
        {
            get { return m_Protecting; }
            set { m_Protecting = value; }
        }

        public virtual bool InitialInnocent { get { return true; } }
        public override bool IsInvulnerable { get { return false; } }
        public override bool PlayerRangeSensitive { get { return false; } }

        [Constructable]
        public ActionAI_CaravanGuard() : base(AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.6)
        {
            Hue = Utility.RandomSkinHue();

            SetStr(600, 800);
            SetDex(91, 510);
            SetInt(161, 585);

            SetHits(222, 308);

            SetDamage(23, 46);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 60, 80);
            SetResistance(ResistanceType.Fire, 60, 80);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 70, 80);

            SetSkill(SkillName.EvalInt, 97.5, 150.0);
            SetSkill(SkillName.Fencing, 92.5, 135.0);
            SetSkill(SkillName.Macing, 92.5, 135.0);
            SetSkill(SkillName.Magery, 92.5, 135.0);
            SetSkill(SkillName.Meditation, 97.5, 300.0);
            SetSkill(SkillName.MagicResist, 97.5, 130.0);
            SetSkill(SkillName.Swords, 92.5, 135.0);
            SetSkill(SkillName.Tactics, 92.5, 135.0);
            SetSkill(SkillName.Wrestling, 92.5, 135.0);

            Fame = 5000;
            Karma = 1000;

            PackGold(1770, 3100);
            PackReg(10, 15);
            PackArmor(2, 5, 0.8);
            PackWeapon(3, 5, 0.8);
            PackSlayer();
            PackItem(new Bandage(Utility.RandomMinMax(1, 15)));
            this.Female = false;
            Body = 0x190;
            Name = NameList.RandomName("male");

            PlateArms arms = new PlateArms();
            arms.Resource = CraftResource.ShadowIron;
            AddItem(arms);

            PlateLegs legs = new PlateLegs();
            legs.Resource = CraftResource.ShadowIron;
            AddItem(legs);

            PlateChest chest = new PlateChest();
            chest.Resource = CraftResource.ShadowIron;
            AddItem(chest);

            Halberd halberd = new Halberd();
            halberd.Resource = CraftResource.ShadowIron;
            AddItem(halberd);

            // new ForestOstard().Rider = this;
        }

        public override void OnThink()
        {
            if (m_Protecting != null)
            {
                if (m_Protecting.Deleted && !m_Protecting.Alive)
                    this.Delete();

                if (m_Protecting.Combatant != null)
                    this.Combatant = m_Protecting.Combatant;

                /* if( ((CaravanVendor)m_Protecting).Packhorse != null && ((CaravanVendor)m_Protecting).Packhorse.Combatant != null )
                    KillPetAttacker(); */

                if (!m_Protecting.Deleted && m_Protecting.Alive && !this.InRange(m_Protecting.Location, 15))
                {
                    this.Combatant = null;
                    this.CurrentWayPoint = ((ActionAI_CaravanVendor)m_Protecting).waypointFirst;
                }
            }

            base.OnThink();
        }

        private void KillPetAttacker()
        {
            if (!((ActionAI_CaravanVendor)m_Protecting).Packhorse.Alive)
                return;

            BaseCreature pet = (BaseCreature)((ActionAI_CaravanVendor)m_Protecting).Packhorse;

            Mobile controlMaster = pet.ControlMaster;

            if (controlMaster == null || controlMaster.Deleted)
                return;

            Mobile combatant = pet.Combatant;

            List<AggressorInfo> aggressors = pet.Aggressors;

            if (aggressors.Count > 0)
            {
                for (int i = 0; i < aggressors.Count; ++i)
                {
                    AggressorInfo info = aggressors[i];
                    Mobile attacker = info.Attacker;

                    if (attacker != null && !attacker.Deleted && attacker.GetDistanceToSqrt(pet) <= this.RangePerception)
                    {
                        if (combatant == null || attacker.GetDistanceToSqrt(controlMaster) < combatant.GetDistanceToSqrt(pet))
                            this.Combatant = attacker;
                    }
                }
            }

        }

        public ActionAI_CaravanGuard(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version

            writer.Write(m_Protecting);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Protecting = reader.ReadMobile() as BaseCreature;
        }
    }


    [CorpseName( "a horse corpse" )]
	public class CaravanPackHorse : PackHorse
	{
        public override bool PlayerRangeSensitive { get { return false; } }

		[Constructable]
		public CaravanPackHorse() : base()
		{
			Name = "a pack horse";
			Body = 291;
			BaseSoundID = 0xA8;

			SetStr( 44, 120 );
			SetDex( 36, 55 );
			SetInt( 6, 10 );

			SetHits( 61, 80 );
			SetStam( 81, 100 );
			SetMana( 0 );

			SetDamage( 5, 11 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 20, 25 );
			SetResistance( ResistanceType.Fire, 10, 15 );
			SetResistance( ResistanceType.Cold, 20, 25 );
			SetResistance( ResistanceType.Poison, 10, 15 );
			SetResistance( ResistanceType.Energy, 10, 15 );

			SetSkill( SkillName.MagicResist, 25.1, 30.0 );
			SetSkill( SkillName.Tactics, 29.3, 44.0 );
			SetSkill( SkillName.Wrestling, 29.3, 44.0 );

			Fame = 0;
			Karma = 200;

			VirtualArmor = 16;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 11.1;

			Container pack = Backpack;

			if ( pack != null )
				pack.Delete();

			pack = new StrongBackpack();
			pack.Movable = false;

			AddItem( pack );
		}

		public override int Meat{ get{ return 3; } }
		public override int Hides{ get{ return 10; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public CaravanPackHorse( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}


}


