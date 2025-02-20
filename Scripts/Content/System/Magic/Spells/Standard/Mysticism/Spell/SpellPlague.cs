﻿using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
	public class SpellPlagueSpell : MysticismSpell
	{
		private static readonly Dictionary<Mobile, SpellPlagueContext> m_Table = new();

		public static void Initialize()
		{
			EventSink.PlayerDeath += e => RemoveEffect(e.Mobile);
			EventSink.CreatureDeath += e => RemoveEffect(e.Mobile);
		}

		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.25);

		public SpellPlagueSpell(Mobile caster, Item scroll)
			: base(caster, scroll, MysticismSpellName.SpellPlague)
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public void Target(Mobile targeted)
		{
			if (!Caster.CanSee(targeted))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckHSequence(targeted))
			{
				SpellHelper.Turn(Caster, targeted);

				SpellHelper.CheckReflect(6, Caster, ref targeted);

				/* The target is hit with an explosion of chaos damage and then inflicted
				 * with the spell plague curse. Each time the target is damaged while under
				 * the effect of the spell plague, they may suffer an explosion of chaos
				 * damage. The initial chance to trigger the explosion starts at 90% and
				 * reduces by 30% every time an explosion occurs. Once the target is
				 * afflicted by 3 explosions or 8 seconds have passed, that spell plague
				 * is removed from the target. Spell Plague will stack with other spell
				 * plagues so that they are applied one after the other.
				 */

				VisualEffect(targeted);

				var damage = GetNewAosDamage(33, 1, 5, targeted);

				var types = new int[4];

				types[Utility.Random(types.Length)] = 100;

				SpellHelper.Damage(this, targeted, damage, 0, types[0], types[1], types[2], types[3]);

				var context = new SpellPlagueContext(this, targeted);

				if (m_Table.TryGetValue(targeted, out var oldContext))
				{
					oldContext.SetNext(context);
				}
				else
				{
					m_Table[targeted] = context;

					context.Start();
				}
			}

			FinishSequence();
		}

		public static bool UnderEffect(Mobile m)
		{
			return m_Table.ContainsKey(m);
		}

		public static void RemoveEffect(Mobile m)
		{
			if (m_Table.TryGetValue(m, out var context))
			{
				context.EndPlague(false);
			}
		}

		public static void CheckPlague(Mobile m)
		{
			if (m_Table.TryGetValue(m, out var context))
			{
				context.OnDamage();
			}
		}

		private static void VisualEffect(Mobile to)
		{
			to.PlaySound(0x658);

			to.FixedParticles(0x3728, 1, 13, 0x26B8, 0x47E, 7, EffectLayer.Head, 0);
			to.FixedParticles(0x3779, 1, 15, 0x251E, 0x43, 7, EffectLayer.Head, 0);
		}

		private class SpellPlagueContext
		{
			private readonly SpellPlagueSpell m_Owner;
			private readonly Mobile m_Target;
			private DateTime m_LastExploded;
			private int m_Explosions;
			private Timer m_Timer;
			private SpellPlagueContext m_Next;

			public SpellPlagueContext(SpellPlagueSpell owner, Mobile target)
			{
				m_Owner = owner;
				m_Target = target;
			}

			public void SetNext(SpellPlagueContext context)
			{
				if (m_Next == null)
				{
					m_Next = context;
				}
				else
				{
					m_Next.SetNext(context);
				}
			}

			public void Start()
			{
				m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(8.0), EndPlague);
				m_Timer.Start();

				BuffInfo.AddBuff(m_Target, new BuffInfo(BuffIcon.SpellPlague, 1031690, 1080167, TimeSpan.FromSeconds(8.5), m_Target));
			}

			public void OnDamage()
			{
				if (DateTime.UtcNow > (m_LastExploded + TimeSpan.FromSeconds(2.0)))
				{
					var exploChance = 90 - (m_Explosions * 30);

					var resist = m_Target.Skills[SkillName.MagicResist].Value;

					if (resist >= 70)
					{
						exploChance -= (int)((resist - 70.0) * 3.0 / 10.0);
					}

					if (exploChance > Utility.Random(100))
					{
						VisualEffect(m_Target);

						var damage = m_Owner.GetNewAosDamage(15 + (m_Explosions * 3), 1, 5, m_Target);

						++m_Explosions;

						m_LastExploded = DateTime.UtcNow;

						var types = new int[4];

						types[Utility.Random(types.Length)] = 100;

						SpellHelper.Damage(m_Owner, m_Target, damage, 0, types[0], types[1], types[2], types[3]);

						if (m_Explosions >= 3)
						{
							EndPlague();
						}
					}
				}
			}

			private void EndPlague()
			{
				EndPlague(true);
			}

			public void EndPlague(bool restart)
			{
				if (m_Timer != null)
				{
					m_Timer.Stop();
				}

				if (restart && m_Next != null)
				{
					m_Table[m_Target] = m_Next;

					m_Next.Start();
				}
				else
				{
					m_Table.Remove(m_Target);

					BuffInfo.RemoveBuff(m_Target, BuffIcon.SpellPlague);
				}
			}
		}

		private class InternalTarget : Target
		{
			private readonly SpellPlagueSpell m_Owner;

			public InternalTarget(SpellPlagueSpell owner)
				: base(12, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile m)
				{
					m_Owner.Target(m);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}