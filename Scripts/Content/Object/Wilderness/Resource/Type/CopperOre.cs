﻿namespace Server.Items
{
	public class CopperOre : BaseOre
	{
		[Constructable]
		public CopperOre() : this(1)
		{
		}

		[Constructable]
		public CopperOre(int amount) : base(CraftResource.Copper, amount)
		{
		}

		public CopperOre(Serial serial) : base(serial)
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

		public override BaseIngot GetIngot()
		{
			return new CopperIngot();
		}
	}
}