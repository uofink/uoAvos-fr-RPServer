﻿namespace Server.Items
{
	public class SummonFeyScroll : SpellScroll
	{
		[Constructable]
		public SummonFeyScroll()
			: this(1)
		{
		}

		[Constructable]
		public SummonFeyScroll(int amount)
			: base(606, 0x2D57, amount)
		{
			Hue = 0x8FD;
		}

		public SummonFeyScroll(Serial serial)
			: base(serial)
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