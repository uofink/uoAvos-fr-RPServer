﻿using Server.Mobiles;
using Server.Multis;
using Server.Targeting;

using System;

namespace Server.Items
{
	[FlipableAttribute(0x1EBA, 0x1EBB)]
	public class TaxidermyKit : Item
	{
		public override int LabelNumber => 1041279;  // a taxidermy kit

		[Constructable]
		public TaxidermyKit() : base(0x1EBA)
		{
			Weight = 1.0;
		}

		public TaxidermyKit(Serial serial) : base(serial)
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

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (from.Skills[SkillName.Carpentry].Base < 90.0)
			{
				from.SendLocalizedMessage(1042594); // You do not understand how to use this.
			}
			else
			{
				from.SendLocalizedMessage(1042595); // Target the corpse to make a trophy out of.
				from.Target = new CorpseTarget(this);
			}
		}

		private static readonly TrophyInfo[] m_Table = new TrophyInfo[]
		{
			new TrophyInfo( typeof( BrownBear ),    0x1E60,     1041093, 1041107 ),
			new TrophyInfo( typeof( GreatHart ),    0x1E61,     1041095, 1041109 ),
			new TrophyInfo( typeof( BigFish ),      0x1E62,     1041096, 1041110 ),
			new TrophyInfo( typeof( Gorilla ),      0x1E63,     1041091, 1041105 ),
			new TrophyInfo( typeof( Orc ),          0x1E64,     1041090, 1041104 ),
			new TrophyInfo( typeof( PolarBear ),    0x1E65,     1041094, 1041108 ),
			new TrophyInfo( typeof( Troll ),        0x1E66,     1041092, 1041106 )
		};

		public class TrophyInfo
		{
			public TrophyInfo(Type type, int id, int deedNum, int addonNum)
			{
				m_CreatureType = type;
				m_NorthID = id;
				m_DeedNumber = deedNum;
				m_AddonNumber = addonNum;
			}

			private readonly Type m_CreatureType;
			private readonly int m_NorthID;
			private readonly int m_DeedNumber;
			private readonly int m_AddonNumber;

			public Type CreatureType => m_CreatureType;
			public int NorthID => m_NorthID;
			public int DeedNumber => m_DeedNumber;
			public int AddonNumber => m_AddonNumber;
		}


		private class CorpseTarget : Target
		{
			private readonly TaxidermyKit m_Kit;

			public CorpseTarget(TaxidermyKit kit) : base(3, false, TargetFlags.None)
			{
				m_Kit = kit;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Kit.Deleted)
				{
					return;
				}

				if (!(targeted is Corpse) && !(targeted is BigFish))
				{
					from.SendLocalizedMessage(1042600); // That is not a corpse!
				}
				else if (targeted is Corpse && ((Corpse)targeted).VisitedByTaxidermist)
				{
					from.SendLocalizedMessage(1042596); // That corpse seems to have been visited by a taxidermist already.
				}
				else if (!m_Kit.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				}
				else if (from.Skills[SkillName.Carpentry].Base < 90.0)
				{
					from.SendLocalizedMessage(1042603); // You would not understand how to use the kit.
				}
				else
				{
					var obj = targeted;

					if (obj is Corpse)
					{
						obj = ((Corpse)obj).Owner;
					}

					if (obj != null)
					{
						for (var i = 0; i < m_Table.Length; i++)
						{
							if (m_Table[i].CreatureType == obj.GetType())
							{
								var pack = from.Backpack;

								if (pack != null && pack.ConsumeTotal(typeof(Board), 10))
								{
									from.SendLocalizedMessage(1042278); // You review the corpse and find it worthy of a trophy.
									from.SendLocalizedMessage(1042602); // You use your kit up making the trophy.

									Mobile hunter = null;
									var weight = 0;

									if (targeted is BigFish)
									{
										var fish = targeted as BigFish;

										hunter = fish.Fisher;
										weight = (int)fish.Weight;

										fish.Consume();
									}


									from.AddToBackpack(new TrophyDeed(m_Table[i], hunter, weight));

									if (targeted is Corpse)
									{
										((Corpse)targeted).VisitedByTaxidermist = true;
									}

									m_Kit.Delete();
									return;
								}
								else
								{
									from.SendLocalizedMessage(1042598); // You do not have enough boards.
									return;
								}
							}
						}
					}

					from.SendLocalizedMessage(1042599); // That does not look like something you want hanging on a wall.
				}
			}
		}
	}

	public class TrophyAddon : Item, IAddon
	{
		public override bool ForceShowProperties => ObjectPropertyList.Enabled;

		private int m_WestID;
		private int m_NorthID;
		private int m_DeedNumber;
		private int m_AddonNumber;

		private Mobile m_Hunter;
		private int m_AnimalWeight;

		[CommandProperty(AccessLevel.GameMaster)]
		public int WestID { get => m_WestID; set => m_WestID = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int NorthID { get => m_NorthID; set => m_NorthID = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DeedNumber { get => m_DeedNumber; set => m_DeedNumber = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AddonNumber { get => m_AddonNumber; set { m_AddonNumber = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Hunter { get => m_Hunter; set { m_Hunter = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AnimalWeight { get => m_AnimalWeight; set { m_AnimalWeight = value; InvalidateProperties(); } }

		public override int LabelNumber => m_AddonNumber;

		[Constructable]
		public TrophyAddon(Mobile from, int itemID, int westID, int northID, int deedNumber, int addonNumber) : this(from, itemID, westID, northID, deedNumber, addonNumber, null, 0)
		{
		}

		public TrophyAddon(Mobile from, int itemID, int westID, int northID, int deedNumber, int addonNumber, Mobile hunter, int animalWeight) : base(itemID)
		{
			m_WestID = westID;
			m_NorthID = northID;
			m_DeedNumber = deedNumber;
			m_AddonNumber = addonNumber;

			m_Hunter = hunter;
			m_AnimalWeight = animalWeight;

			Movable = false;

			MoveToWorld(from.Location, from.Map);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_AnimalWeight >= 20)
			{
				if (m_Hunter != null)
				{
					list.Add(1070857, m_Hunter.Name); // Caught by ~1_fisherman~
				}

				list.Add(1070858, m_AnimalWeight.ToString()); // ~1_weight~ stones
			}
		}

		public TrophyAddon(Serial serial) : base(serial)
		{
		}

		public bool CouldFit(IPoint3D p, Map map)
		{
			if (!map.CanFit(p.X, p.Y, p.Z, ItemData.Height))
			{
				return false;
			}

			if (ItemID == m_NorthID)
			{
				return BaseAddon.IsWall(p.X, p.Y - 1, p.Z, map); // North wall
			}
			else
			{
				return BaseAddon.IsWall(p.X - 1, p.Y, p.Z, map); // West wall
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Hunter);
			writer.Write(m_AnimalWeight);

			writer.Write(m_WestID);
			writer.Write(m_NorthID);
			writer.Write(m_DeedNumber);
			writer.Write(m_AddonNumber);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Hunter = reader.ReadMobile();
						m_AnimalWeight = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_WestID = reader.ReadInt();
						m_NorthID = reader.ReadInt();
						m_DeedNumber = reader.ReadInt();
						m_AddonNumber = reader.ReadInt();
						break;
					}
			}

			Timer.DelayCall(TimeSpan.Zero, FixMovingCrate);
		}

		private void FixMovingCrate()
		{
			if (Deleted)
			{
				return;
			}

			if (Movable || IsLockedDown)
			{
				var deed = Deed;

				if (Parent is Item)
				{
					((Item)Parent).AddItem(deed);
					deed.Location = Location;
				}
				else
				{
					deed.MoveToWorld(Location, Map);
				}

				Delete();
			}
		}

		public Item Deed => new TrophyDeed(m_WestID, m_NorthID, m_DeedNumber, m_AddonNumber, m_Hunter, m_AnimalWeight);

		public override void OnDoubleClick(Mobile from)
		{
			var house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsCoOwner(from))
			{
				if (from.InRange(GetWorldLocation(), 1))
				{
					from.AddToBackpack(Deed);
					Delete();
				}
				else
				{
					from.SendLocalizedMessage(500295); // You are too far away to do that.
				}
			}
		}
	}

	[Flipable(0x14F0, 0x14EF)]
	public class TrophyDeed : Item
	{
		private int m_WestID;
		private int m_NorthID;
		private int m_DeedNumber;
		private int m_AddonNumber;

		private Mobile m_Hunter;
		private int m_AnimalWeight;

		[CommandProperty(AccessLevel.GameMaster)]
		public int WestID { get => m_WestID; set => m_WestID = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int NorthID { get => m_NorthID; set => m_NorthID = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DeedNumber { get => m_DeedNumber; set { m_DeedNumber = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AddonNumber { get => m_AddonNumber; set => m_AddonNumber = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Hunter { get => m_Hunter; set { m_Hunter = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AnimalWeight { get => m_AnimalWeight; set { m_AnimalWeight = value; InvalidateProperties(); } }

		public override int LabelNumber => m_DeedNumber;

		[Constructable]
		public TrophyDeed(int westID, int northID, int deedNumber, int addonNumber) : this(westID, northID, deedNumber, addonNumber, null, 0)
		{
		}

		public TrophyDeed(int westID, int northID, int deedNumber, int addonNumber, Mobile hunter, int animalWeight) : base(0x14F0)
		{
			m_WestID = westID;
			m_NorthID = northID;
			m_DeedNumber = deedNumber;
			m_AddonNumber = addonNumber;
			m_Hunter = hunter;
			m_AnimalWeight = animalWeight;
		}

		public TrophyDeed(TaxidermyKit.TrophyInfo info, Mobile hunter, int animalWeight)
			: this(info.NorthID + 7, info.NorthID, info.DeedNumber, info.AddonNumber, hunter, animalWeight)
		{
		}
		public TrophyDeed(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_AnimalWeight >= 20)
			{
				if (m_Hunter != null)
				{
					list.Add(1070857, m_Hunter.Name); // Caught by ~1_fisherman~
				}

				list.Add(1070858, m_AnimalWeight.ToString()); // ~1_weight~ stones
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Hunter);
			writer.Write(m_AnimalWeight);

			writer.Write(m_WestID);
			writer.Write(m_NorthID);
			writer.Write(m_DeedNumber);
			writer.Write(m_AddonNumber);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Hunter = reader.ReadMobile();
						m_AnimalWeight = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_WestID = reader.ReadInt();
						m_NorthID = reader.ReadInt();
						m_DeedNumber = reader.ReadInt();
						m_AddonNumber = reader.ReadInt();
						break;
					}
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				var house = BaseHouse.FindHouseAt(from);

				if (house != null && house.IsCoOwner(from))
				{
					var northWall = BaseAddon.IsWall(from.X, from.Y - 1, from.Z, from.Map);
					var westWall = BaseAddon.IsWall(from.X - 1, from.Y, from.Z, from.Map);

					if (northWall && westWall)
					{
						switch (from.Direction & Direction.Mask)
						{
							case Direction.North:
							case Direction.South: northWall = true; westWall = false; break;

							case Direction.East:
							case Direction.West: northWall = false; westWall = true; break;

							default: from.SendMessage("Turn to face the wall on which to hang this trophy."); return;
						}
					}

					var itemID = 0;

					if (northWall)
					{
						itemID = m_NorthID;
					}
					else if (westWall)
					{
						itemID = m_WestID;
					}
					else
					{
						from.SendLocalizedMessage(1042626); // The trophy must be placed next to a wall.
					}

					if (itemID > 0)
					{
						house.Addons.Add(new TrophyAddon(from, itemID, m_WestID, m_NorthID, m_DeedNumber, m_AddonNumber, m_Hunter, m_AnimalWeight));
						Delete();
					}
				}
				else
				{
					from.SendLocalizedMessage(502092); // You must be in your house to do this.
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}
	}
}