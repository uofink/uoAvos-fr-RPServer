﻿namespace Server.Items
{
	public class LeatherDyeTub : DyeTub, Engines.VeteranRewards.IRewardItem
	{
		public override bool AllowDyables => false;
		public override bool AllowLeather => true;
		public override int TargetMessage => 1042416;  // Select the leather item to dye.
		public override int FailMessage => 1042418;  // You can only dye leather with this tub.
		public override int LabelNumber => 1041284;  // Leather Dye Tub
		public override CustomHuePicker CustomHuePicker => CustomHuePicker.LeatherDyeTub;

		private bool m_IsRewardItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem
		{
			get => m_IsRewardItem;
			set => m_IsRewardItem = value;
		}

		[Constructable]
		public LeatherDyeTub()
		{
			LootType = LootType.Blessed;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_IsRewardItem && !Engines.VeteranRewards.RewardSystem.CheckIsUsableBy(from, this, null))
			{
				return;
			}

			base.OnDoubleClick(from);
		}

		public LeatherDyeTub(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && m_IsRewardItem)
			{
				list.Add(1076218); // 2nd Year Veteran Reward
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_IsRewardItem = reader.ReadBool();
						break;
					}
			}
		}
	}
}