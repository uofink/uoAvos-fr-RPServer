﻿namespace Server.Items
{
	public class FishBones : Item
	{
		public override int LabelNumber => 1074601;  // Fish bones
		public override double DefaultWeight => 1.0;

		[Constructable]
		public FishBones() : base(0x3B0C)
		{
		}

		public FishBones(Serial serial) : base(serial)
		{
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			list.Add(1073634); // An aquarium decoration
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