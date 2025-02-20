﻿namespace Server.Items
{
	public class SkullCollection : Item
	{
		[Constructable]
		public SkullCollection() : base(0x21FC)
		{
		}

		public SkullCollection(Serial serial) : base(serial)
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