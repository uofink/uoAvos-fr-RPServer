﻿using Server.Accounting;
using Server.Mobiles;
using Server.Network;

using System;
using System.Globalization;

namespace Server.Items
{
	public class BankCheck : Item
	{
		private int m_Worth;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Worth
		{
			get => m_Worth;
			set { m_Worth = value; InvalidateProperties(); }
		}

		public BankCheck(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Worth);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			LootType = LootType.Blessed;

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Worth = reader.ReadInt();
						break;
					}
			}
		}

		[Constructable]
		public BankCheck(int worth) : base(0x14F0)
		{
			Weight = 1.0;
			Hue = 0x34;
			LootType = LootType.Blessed;

			m_Worth = worth;
		}

		public override bool DisplayLootType => Core.AOS;

		public override int LabelNumber => 1041361;  // A bank check

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			string worth;

			if (Core.ML)
			{
				worth = m_Worth.ToString("N0", CultureInfo.GetCultureInfo("en-US"));
			}
			else
			{
				worth = m_Worth.ToString();
			}

			list.Add(1060738, worth); // value: ~1_val~
		}

#if NEWPARENT
		public override void OnAdded(IEntity parent)
#else
		public override void OnAdded(IEntity parent)
#endif
		{
			base.OnAdded(parent);

			if (!AccountGold.Enabled)
			{
				return;
			}

			Mobile owner = null;
			SecureTradeInfo tradeInfo = null;

			var root = parent as Container;

			while (root != null && root.Parent is Container)
			{
				root = (Container)root.Parent;
			}

			parent = root ?? parent;

			if (parent is SecureTradeContainer && AccountGold.ConvertOnTrade)
			{
				var trade = (SecureTradeContainer)parent;

				if (trade.Trade.From.Container == trade)
				{
					tradeInfo = trade.Trade.From;
					owner = tradeInfo.Mobile;
				}
				else if (trade.Trade.To.Container == trade)
				{
					tradeInfo = trade.Trade.To;
					owner = tradeInfo.Mobile;
				}
			}
			else if (parent is BankBox && AccountGold.ConvertOnBank)
			{
				owner = ((BankBox)parent).Owner;
			}

			if (owner == null || owner.Account == null || !owner.Account.DepositGold(Worth))
			{
				return;
			}

			if (tradeInfo != null)
			{
				if (owner.NetState != null && !owner.NetState.NewSecureTrading)
				{
					var total = Worth / Math.Max(1.0, Account.CurrencyThreshold);
					var plat = (int)Math.Truncate(total);
					var gold = (int)((total - plat) * Account.CurrencyThreshold);

					tradeInfo.Plat += plat;
					tradeInfo.Gold += gold;
				}

				if (tradeInfo.VirtualCheck != null)
				{
					tradeInfo.VirtualCheck.UpdateTrade(tradeInfo.Mobile);
				}
			}

			owner.SendLocalizedMessage(1042763, Worth.ToString("#,0"));

			Delete();

			((Container)parent).UpdateTotals();
		}

		public override void OnSingleClick(Mobile from)
		{
			from.Send(
				new MessageLocalizedAffix(
					Serial,
					ItemID,
					MessageType.Label,
					0x3B2,
					3,
					1041361,
					"",
					AffixType.Append,
					String.Concat(" ", m_Worth.ToString()),
					"")); // A bank check:
		}

		public override void OnDoubleClick(Mobile from)
		{
			// This probably isn't OSI accurate, but we can't just make the quests redundant.
			// Double-clicking the BankCheck in your pack will now credit your account.

			var box = AccountGold.Enabled ? from.Backpack : from.FindBankNoCreate();

			if (box == null || !IsChildOf(box))
			{
				from.SendLocalizedMessage(AccountGold.Enabled ? 1080058 : 1047026);
				// This must be in your backpack to use it. : That must be in your bank box to use it.
				return;
			}

			Delete();

			var deposited = 0;
			var toAdd = m_Worth;

			if (AccountGold.Enabled && from.Account != null && from.Account.DepositGold(toAdd))
			{
				deposited = toAdd;
				toAdd = 0;
			}

			if (toAdd > 0)
			{
				Gold gold;

				while (toAdd > 60000)
				{
					gold = new Gold(60000);

					if (box.TryDropItem(from, gold, false))
					{
						toAdd -= 60000;
						deposited += 60000;
					}
					else
					{
						gold.Delete();

						from.AddToBackpack(new BankCheck(toAdd));
						toAdd = 0;

						break;
					}
				}

				if (toAdd > 0)
				{
					gold = new Gold(toAdd);

					if (box.TryDropItem(from, gold, false))
					{
						deposited += toAdd;
					}
					else
					{
						gold.Delete();

						from.AddToBackpack(new BankCheck(toAdd));
					}
				}
			}

			// Gold was deposited in your account:
			from.SendLocalizedMessage(1042672, true, deposited.ToString("#,0"));

			var pm = from as PlayerMobile;

			if (pm != null)
			{
				var qs = pm.Quest;

				if (qs is Engines.Quests.Definitions.DarkTidesQuest)
				{
					var obj = qs.FindObjective(typeof(Engines.Quests.Definitions.CashBankCheckObjective_DarkTidesQuest));

					if (obj != null && !obj.Completed)
					{
						obj.Complete();
					}
				}

				if (qs is Engines.Quests.Definitions.UzeraanTurmoilQuest)
				{
					var obj = qs.FindObjective(typeof(Engines.Quests.Definitions.CashBankCheckObjective_UzeraanTurmoilQuest));

					if (obj != null && !obj.Completed)
					{
						obj.Complete();
					}
				}
			}
		}
	}
}