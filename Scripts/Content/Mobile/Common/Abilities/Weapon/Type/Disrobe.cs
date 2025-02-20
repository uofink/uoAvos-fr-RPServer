﻿
using System;

#region Developer Notations

/// This attack allows you to disrobe your foe.

#endregion

namespace Server.Items
{

	public class Disrobe : WeaponAbility
	{
		public Disrobe()
		{
		}

		public override int BaseMana => 20;  // Not Sure what amount of mana a creature uses.

		public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds(5.0);

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker))
			{
				return;
			}

			ClearCurrentAbility(attacker);
			var toDisrobe = defender.FindItemOnLayer(Layer.InnerTorso);

			if (toDisrobe == null || !toDisrobe.Movable)
			{
				toDisrobe = defender.FindItemOnLayer(Layer.OuterTorso);
			}

			var pack = defender.Backpack;

			if (pack == null || toDisrobe == null || !toDisrobe.Movable)
			{
				attacker.SendLocalizedMessage(1004001); // You cannot disarm your opponent.
			}
			else if (CheckMana(attacker, true))
			{
				//attacker.SendLocalizedMessage( 1060092 ); // You disarm their weapon!
				defender.SendLocalizedMessage(1062002); // You can no longer wear your ~1_ARMOR~

				defender.PlaySound(0x3B9);
				//defender.FixedParticles( 0x37BE, 232, 25, 9948, EffectLayer.InnerTorso );

				pack.DropItem(toDisrobe);

				BaseWeapon.BlockEquip(defender, BlockEquipDuration);
			}
		}
	}
}