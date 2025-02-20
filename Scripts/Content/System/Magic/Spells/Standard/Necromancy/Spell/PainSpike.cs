﻿using Server.Targeting;

using System;
using System.Collections;

namespace Server.Spells.Necromancy
{
	public class PainSpikeSpell : NecromancySpell
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.0);

		public PainSpikeSpell(Mobile caster, Item scroll) 
			: base(caster, scroll, NecromancySpellName.PainSpike)
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public override bool DelayedDamage => false;

		public void Target(Mobile m)
		{
			if (CheckHSequence(m))
			{
				SpellHelper.Turn(Caster, m);

				//SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m ); //Irrelevent asfter AoS

				/* Temporarily causes intense physical pain to the target, dealing direct damage.
				 * After 10 seconds the spell wears off, and if the target is still alive, 
				 * some of the Hit Points lost through Pain Spike are restored.
				 */

				m.FixedParticles(0x37C4, 1, 8, 9916, 39, 3, EffectLayer.Head);
				m.FixedParticles(0x37C4, 1, 8, 9502, 39, 4, EffectLayer.Head);
				m.PlaySound(0x210);

				var damage = ((GetDamageSkill(Caster) - GetResistSkill(m)) / 10) + (m.Player ? 18 : 30);
				m.CheckSkill(SkillName.MagicResist, 0.0, 120.0);    //Skill check for gain

				if (damage < 1)
				{
					damage = 1;
				}

				var buffTime = TimeSpan.FromSeconds(10.0);

				if (m_Table.Contains(m))
				{
					damage = Utility.RandomMinMax(3, 7);
					var t = m_Table[m] as Timer;

					if (t != null)
					{
						t.Delay += TimeSpan.FromSeconds(2.0);

						buffTime = t.Next - DateTime.UtcNow;
					}
				}
				else
				{
					new InternalTimer(m, damage).Start();
				}

				BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.PainSpike, 1075667, buffTime, m, Convert.ToString((int)damage)));

				Misc.WeightOverloading.DFA = Misc.DFAlgorithm.PainSpike;
				m.Damage((int)damage, Caster);
				SpellHelper.DoLeech((int)damage, Caster, m);
				Misc.WeightOverloading.DFA = Misc.DFAlgorithm.Standard;

				//SpellHelper.Damage( this, m, damage, 100, 0, 0, 0, 0, Misc.DFAlgorithm.PainSpike );
				HarmfulSpell(m);
			}

			FinishSequence();
		}

		private static readonly Hashtable m_Table = new Hashtable();

		private class InternalTimer : Timer
		{
			private readonly Mobile m_Mobile;
			private readonly int m_ToRestore;

			public InternalTimer(Mobile m, double toRestore) : base(TimeSpan.FromSeconds(10.0))
			{
				Priority = TimerPriority.OneSecond;

				m_Mobile = m;
				m_ToRestore = (int)toRestore;

				m_Table[m] = this;
			}

			protected override void OnTick()
			{
				m_Table.Remove(m_Mobile);

				if (m_Mobile.Alive && !m_Mobile.IsDeadBondedPet)
				{
					m_Mobile.Hits += m_ToRestore;
				}

				BuffInfo.RemoveBuff(m_Mobile, BuffIcon.PainSpike);
			}
		}

		private class InternalTarget : Target
		{
			private readonly PainSpikeSpell m_Owner;

			public InternalTarget(PainSpikeSpell owner) : base(Core.ML ? 10 : 12, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
				{
					m_Owner.Target((Mobile)o);
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}