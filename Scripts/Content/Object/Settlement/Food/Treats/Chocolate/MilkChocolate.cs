﻿namespace Server.Items
{
	public class MilkChocolate : CandyCane
	{
		public override int LabelNumber => 1079995;  // Milk chocolate
		public override double DefaultWeight => 1.0;

		[Constructable]
		public MilkChocolate()
			: base(0xF18)
		{
			Hue = 0x461;
			LootType = LootType.Regular;
		}

		public MilkChocolate(Serial serial)
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

			var version = reader.ReadInt();
		}
	}
}