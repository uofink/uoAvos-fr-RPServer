﻿using Server.Engines.Quests.Definitions;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

using System.Collections.Generic;

namespace Server.Engines.Quests.Items
{
	public class HagApprenticeCorpse : Corpse
	{
		private static Mobile GetOwner()
		{
			var apprentice = new Mobile {
				Hue = Utility.RandomSkinHue(),
				Female = false,
				Body = 0x190
			};

			apprentice.Delete();

			return apprentice;
		}

		private static List<Item> GetEquipment()
		{
			return new List<Item>();
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add("a charred corpse");
		}

		public override void OnSingleClick(Mobile from)
		{
			var hue = Notoriety.GetHue(NotorietyHandlers.CorpseNotoriety(from, this));

			from.Send(new AsciiMessage(Serial, ItemID, MessageType.Label, hue, 3, "", "a charred corpse"));
		}

		[Constructable]
		public HagApprenticeCorpse() : base(GetOwner(), GetEquipment())
		{
			Direction = Direction.South;

			foreach (var item in EquipItems)
			{
				DropItem(item);
			}
		}

		public HagApprenticeCorpse(Serial serial) : base(serial)
		{
		}

		public override void Open(Mobile from, bool checkSelfLoot)
		{
			if (!from.InRange(GetWorldLocation(), 2))
			{
				return;
			}

			var player = from as PlayerMobile;

			if (player != null)
			{
				var qs = player.Quest;

				if (qs is WitchApprenticeQuest)
				{
					var obj = qs.FindObjective(typeof(FindApprenticeObjective_WitchApprenticeQuest)) as FindApprenticeObjective_WitchApprenticeQuest;

					if (obj != null && !obj.Completed)
					{
						if (obj.Corpse == this)
						{
							obj.Complete();
							Delete();
						}
						else
						{
							SendLocalizedMessageTo(from, 1055047); // You examine the corpse, but it doesn't fit the description of the particular apprentice the Hag tasked you with finding.
						}

						return;
					}
				}
			}

			SendLocalizedMessageTo(from, 1055048); // You examine the corpse, but find nothing of interest.
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