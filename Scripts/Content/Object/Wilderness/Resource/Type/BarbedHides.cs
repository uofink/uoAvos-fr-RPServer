﻿namespace Server.Items
{
	[FlipableAttribute(0x1079, 0x1078)]
	public class BarbedHides : BaseHides, IScissorable
	{
		[Constructable]
		public BarbedHides() : this(1)
		{
		}

		[Constructable]
		public BarbedHides(int amount) : base(CraftResource.BarbedLeather, amount)
		{
		}

		public BarbedHides(Serial serial) : base(serial)
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

		public bool Scissor(Mobile from, Scissors scissors)
		{
			if (Deleted || !from.CanSee(this))
			{
				return false;
			}

			if (Core.AOS && !IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(502437); // Items you wish to cut must be in your backpack
				return false;
			}

			base.ScissorHelper(from, new BarbedLeather(), 1);

			return true;
		}
	}
}