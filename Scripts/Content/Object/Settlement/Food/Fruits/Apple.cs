﻿namespace Server.Items
{
	public class Apple : Food
	{
		[Constructable]
		public Apple() : this(1)
		{
		}

		[Constructable]
		public Apple(int amount) : base(amount, 0x9D0)
		{
			Weight = 1.0;
			FillFactor = 1;
		}

		public Apple(Serial serial) : base(serial)
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