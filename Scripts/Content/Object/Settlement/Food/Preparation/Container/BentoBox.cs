﻿namespace Server.Items
{
	public class BentoBox : Container
	{
		[Constructable]
		public BentoBox() : base(0x2834)
		{
			switch (Utility.Random(2))
			{
				case 1:
					{
						DropItem(new SushiRolls(Utility.Random(1, 3)));
						break;
					}
				case 0:
					{
						DropItem(new SushiPlatter(Utility.Random(1, 3)));
						break;
					}
			}

			DropItem(new Wasabi());
			Weight = 5.0;
		}

		public BentoBox(Serial serial) : base(serial)
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