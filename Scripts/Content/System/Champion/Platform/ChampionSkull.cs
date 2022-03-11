﻿using Server.Engines.CannedEvil;

namespace Server.Engines.CannedEvil
{
	public enum ChampionSkullType
	{
		Power,
		Enlightenment,
		Venom,
		Pain,
		Greed,
		Death
	}
}

namespace Server.Items
{
	public class ChampionSkull : Item
	{
		private ChampionSkullType m_Type;

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionSkullType Type { get => m_Type; set { m_Type = value; InvalidateProperties(); } }

		public override int LabelNumber => 1049479 + (int)m_Type;

		[Constructable]
		public ChampionSkull(ChampionSkullType type) : base(0x1AE1)
		{
			m_Type = type;
			LootType = LootType.Cursed;

			// TODO: All hue values
			switch (type)
			{
				case ChampionSkullType.Power: Hue = 0x159; break;
				case ChampionSkullType.Venom: Hue = 0x172; break;
				case ChampionSkullType.Greed: Hue = 0x1EE; break;
				case ChampionSkullType.Death: Hue = 0x025; break;
				case ChampionSkullType.Pain: Hue = 0x035; break;
			}
		}

		public ChampionSkull(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						m_Type = (ChampionSkullType)reader.ReadInt();

						break;
					}
			}

			if (version == 0)
			{
				if (LootType != LootType.Cursed)
				{
					LootType = LootType.Cursed;
				}

				if (Insured)
				{
					Insured = false;
				}
			}
		}
	}
}