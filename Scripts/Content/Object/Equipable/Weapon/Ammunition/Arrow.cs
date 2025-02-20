﻿namespace Server.Items
{
	public class Arrow : Item, ICommodity
	{
		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => true;

		public override double DefaultWeight => 0.1;

		[Constructable]
		public Arrow() : this(1)
		{
		}

		[Constructable]
		public Arrow(int amount) : base(0xF3F)
		{
			Stackable = true;
			Amount = amount;
		}

		public Arrow(Serial serial) : base(serial)
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