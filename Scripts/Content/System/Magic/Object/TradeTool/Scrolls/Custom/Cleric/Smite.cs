
namespace Server.Items
{
	public class SmiteScroll : SpellScroll
	{
		[Constructable]
		public SmiteScroll() : this(1)
		{
		}

		[Constructable]
		public SmiteScroll(int amount) : base(SpellName.Smite, 0x1F6D, amount)
		{
		}

		public SmiteScroll(Serial serial) : base(serial)
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
