﻿namespace Server.Items
{
	public class Bloodmoss : BaseReagent, ICommodity
	{
		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => true;

		[Constructable]
		public Bloodmoss() : this(1)
		{
		}

		[Constructable]
		public Bloodmoss(int amount) : base(0xF7B, amount)
		{
		}

		public Bloodmoss(Serial serial) : base(serial)
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