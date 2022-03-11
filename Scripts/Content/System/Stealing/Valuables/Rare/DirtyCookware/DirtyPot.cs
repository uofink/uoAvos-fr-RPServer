﻿namespace Server.Engines.Stealables
{
	public class DirtyPot : Item
	{
		[Constructable]
		public DirtyPot() : base(0x9E6)
		{
			Weight = 1.0;
		}

		public DirtyPot(Serial serial) : base(serial)
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