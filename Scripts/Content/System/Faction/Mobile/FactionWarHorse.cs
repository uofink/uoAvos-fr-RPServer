﻿using Server.Mobiles;

namespace Server.Factions
{
	[CorpseName("a war horse corpse")]
	public class FactionWarHorse : BaseMount
	{
		private Faction m_Faction;

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public Faction Faction
		{
			get => m_Faction;
			set
			{
				m_Faction = value;

				Body = (m_Faction == null ? 0xE2 : m_Faction.Definition.WarHorseBody);
				ItemID = (m_Faction == null ? 0x3EA0 : m_Faction.Definition.WarHorseItem);
			}
		}

		public const int SilverPrice = 500;
		public const int GoldPrice = 3000;

		[Constructable]
		public FactionWarHorse() : this(null)
		{
		}

		public FactionWarHorse(Faction faction) : base("a war horse", 0xE2, 0x3EA0, AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4)
		{
			BaseSoundID = 0xA8;

			SetStr(400);
			SetDex(125);
			SetInt(51, 55);

			SetHits(240);
			SetMana(0);

			SetDamage(5, 8);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 40, 50);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Cold, 30, 40);
			SetResistance(ResistanceType.Poison, 30, 40);
			SetResistance(ResistanceType.Energy, 30, 40);

			SetSkill(SkillName.MagicResist, 25.1, 30.0);
			SetSkill(SkillName.Tactics, 29.3, 44.0);
			SetSkill(SkillName.Wrestling, 29.3, 44.0);

			Fame = 300;
			Karma = 300;

			Tamable = true;
			ControlSlots = 1;

			Faction = faction;
		}

		public override FoodType FavoriteFood => FoodType.FruitsAndVegies | FoodType.GrainsAndHay;

		public FactionWarHorse(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			var pl = PlayerState.Find(from);

			if (pl == null)
			{
				from.SendLocalizedMessage(1010366); // You cannot mount a faction war horse!
			}
			else if (pl.Faction != Faction)
			{
				from.SendLocalizedMessage(1010367); // You cannot ride an opposing faction's war horse!
			}
			else if (pl.Rank.Rank < 2)
			{
				from.SendLocalizedMessage(1010368); // You must achieve a faction rank of at least two before riding a war horse!
			}
			else
			{
				base.OnDoubleClick(from);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Faction.WriteReference(writer, m_Faction);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Faction = Faction.ReadReference(reader);
						break;
					}
			}
		}
	}

	[CorpseName("a war horse corpse")]
	public abstract class BaseWarHorse : BaseMount
	{
		public BaseWarHorse(int bodyID, int itemID, AIType aiType, FightMode fightMode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed) : base("a war horse", bodyID, itemID, aiType, fightMode, rangePerception, rangeFight, activeSpeed, passiveSpeed)
		{
			BaseSoundID = 0xA8;

			InitStats(Utility.Random(300, 100), 125, 60);

			SetStr(400);
			SetDex(125);
			SetInt(51, 55);

			SetHits(240);
			SetMana(0);

			SetDamage(5, 8);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 40, 50);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Cold, 30, 40);
			SetResistance(ResistanceType.Poison, 30, 40);
			SetResistance(ResistanceType.Energy, 30, 40);

			SetSkill(SkillName.MagicResist, 25.1, 30.0);
			SetSkill(SkillName.Tactics, 29.3, 44.0);
			SetSkill(SkillName.Wrestling, 29.3, 44.0);

			Fame = 300;
			Karma = 300;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 29.1;
		}

		public override FoodType FavoriteFood => FoodType.FruitsAndVegies | FoodType.GrainsAndHay;

		public BaseWarHorse(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}