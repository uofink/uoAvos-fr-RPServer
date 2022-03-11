﻿using Server.Items;

namespace Server.Engines.ChainQuests.Items
{
	public class MiniatureMushroom : Food
	{
		public override int LabelNumber => 1073138;  // Miniature mushroom

		[Constructable]
		public MiniatureMushroom() : base(0xD16)
		{
			LootType = LootType.Blessed;
		}

		public MiniatureMushroom(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}