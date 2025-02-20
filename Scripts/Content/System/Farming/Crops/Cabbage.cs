﻿namespace Server.Items
{
	public class FarmableCabbage : FarmableCrop
	{
		public static int GetCropID()
		{
			return 3254;
		}

		public override Item GetCropObject()
		{
			var cabbage = new Cabbage {
				ItemID = Utility.Random(3195, 2)
			};

			return cabbage;
		}

		public override int GetPickedID()
		{
			return 3254;
		}

		[Constructable]
		public FarmableCabbage() : base(GetCropID())
		{
		}

		public FarmableCabbage(Serial serial) : base(serial)
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