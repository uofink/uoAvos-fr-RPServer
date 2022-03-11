﻿using Server.Engines.Plants;

using System;

namespace Server.Mobiles
{
	[CorpseName("a dryad's corpse")]
	public class MLDryad : BaseCreature
	{
		public override bool InitialInnocent => true;

		public override OppositionGroup OppositionGroup => OppositionGroup.FeyAndUndead;

		[Constructable]
		public MLDryad() : base(AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
		{
			Name = "a dryad";
			Body = 266;
			BaseSoundID = 0x57B;

			SetStr(132, 149);
			SetDex(152, 168);
			SetInt(251, 280);

			SetHits(304, 321);

			SetDamage(11, 20);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 40, 50);
			SetResistance(ResistanceType.Fire, 15, 25);
			SetResistance(ResistanceType.Cold, 40, 45);
			SetResistance(ResistanceType.Poison, 30, 40);
			SetResistance(ResistanceType.Energy, 25, 35);

			SetSkill(SkillName.Meditation, 80.0, 90.0);
			SetSkill(SkillName.EvalInt, 70.0, 80.0);
			SetSkill(SkillName.Magery, 70.0, 80.0);
			SetSkill(SkillName.Anatomy, 0);
			SetSkill(SkillName.MagicResist, 100.0, 120.0);
			SetSkill(SkillName.Tactics, 70.0, 80.0);
			SetSkill(SkillName.Wrestling, 70.0, 80.0);

			Fame = 5000;
			Karma = 5000;

			VirtualArmor = 28; // Don't know what it should be

			if (Core.ML && Utility.RandomDouble() < .60)
			{
				PackItem(Seed.RandomPeculiarSeed(1));
			}

			PackArcanceScroll(0.05);
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.MlRich);
		}

		public override int Meat => 1;

		public override void OnThink()
		{
			base.OnThink();

			AreaPeace();
			AreaUndress();
		}

		#region Area Peace
		private DateTime m_NextPeace;

		public void AreaPeace()
		{
			if (Combatant == null || Deleted || !Alive || m_NextPeace > DateTime.UtcNow || 0.1 < Utility.RandomDouble())
			{
				return;
			}

			var duration = TimeSpan.FromSeconds(Utility.RandomMinMax(20, 80));

			foreach (var m in GetMobilesInRange(RangePerception))
			{
				var p = m as PlayerMobile;

				if (IsValidTarget(p))
				{
					p.PeacedUntil = DateTime.UtcNow + duration;
					p.SendLocalizedMessage(1072065); // You gaze upon the dryad's beauty, and forget to continue battling!
					p.FixedParticles(0x376A, 1, 20, 0x7F5, EffectLayer.Waist);
					p.Combatant = null;
				}
			}

			m_NextPeace = DateTime.UtcNow + TimeSpan.FromSeconds(10);
			PlaySound(0x1D3);
		}

		public bool IsValidTarget(PlayerMobile m)
		{
			if (m != null && m.PeacedUntil < DateTime.UtcNow && !m.Hidden && m.AccessLevel == AccessLevel.Player && CanBeHarmful(m))
			{
				return true;
			}

			return false;
		}
		#endregion

		#region Undress
		private DateTime m_NextUndress;

		public void AreaUndress()
		{
			if (Combatant == null || Deleted || !Alive || m_NextUndress > DateTime.UtcNow || 0.005 < Utility.RandomDouble())
			{
				return;
			}

			foreach (var m in GetMobilesInRange(RangePerception))
			{
				if (m != null && m.Player && !m.Female && !m.Hidden && m.AccessLevel == AccessLevel.Player && CanBeHarmful(m))
				{
					UndressItem(m, Layer.OuterTorso);
					UndressItem(m, Layer.InnerTorso);
					UndressItem(m, Layer.MiddleTorso);
					UndressItem(m, Layer.Pants);
					UndressItem(m, Layer.Shirt);

					m.SendLocalizedMessage(1072197); // The dryad's beauty makes your blood race. Your clothing is too confining.
				}
			}

			m_NextUndress = DateTime.UtcNow + TimeSpan.FromMinutes(1);
		}

		public void UndressItem(Mobile m, Layer layer)
		{
			var item = m.FindItemOnLayer(layer);

			if (item != null && item.Movable)
			{
				m.PlaceInBackpack(item);
			}
		}
		#endregion

		public MLDryad(Serial serial) : base(serial)
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