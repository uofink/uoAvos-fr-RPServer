﻿namespace Server.Items
{
	/// WoodenBench
	[Furniture]
	[Flipable(0xB2D, 0xB2C)]
	public class WoodenBench : Item
	{
		[Constructable]
		public WoodenBench() : base(0xB2D)
		{
			Weight = 6;
		}

		public WoodenBench(Serial serial) : base(serial)
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