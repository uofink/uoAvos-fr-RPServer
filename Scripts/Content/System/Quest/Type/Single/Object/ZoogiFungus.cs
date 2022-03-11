﻿using Server.Items;

namespace Server.Engines.Quests.Items
{
	public class ZoogiFungus : Item, ICommodity
	{
		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => (Core.ML);

		[Constructable]
		public ZoogiFungus() : this(1)
		{
		}

		[Constructable]
		public ZoogiFungus(int amount) : base(0x26B7)
		{
			Stackable = true;
			Weight = 0.1;
			Amount = amount;
		}

		public ZoogiFungus(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}
}