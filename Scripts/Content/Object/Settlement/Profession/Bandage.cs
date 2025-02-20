﻿using Server.Gumps;
using Server.Mobiles;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class Bandage : Item, IDyable
	{
		public static int Range = (Core.AOS ? 2 : 1);

		public override double DefaultWeight => 0.1;

		public static void Initialize()
		{
			EventSink.BandageTargetRequest += new BandageTargetRequestEventHandler(EventSink_BandageTargetRequest);
		}

		[Constructable]
		public Bandage() : this(1)
		{
		}

		[Constructable]
		public Bandage(int amount) : base(0xE21)
		{
			Stackable = true;
			Amount = amount;
		}

		public Bandage(Serial serial) : base(serial)
		{
		}

		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			Hue = sender.DyedHue;

			return true;
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

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), Range))
			{
				from.RevealingAction();

				from.SendLocalizedMessage(500948); // Who will you use the bandages on?

				from.Target = new InternalTarget(this);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		private static void EventSink_BandageTargetRequest(BandageTargetRequestEventArgs e)
		{
			var b = e.Bandage as Bandage;

			if (b == null || b.Deleted)
			{
				return;
			}

			var from = e.Mobile;

			if (from.InRange(b.GetWorldLocation(), Range))
			{
				var t = from.Target;

				if (t != null)
				{
					Target.Cancel(from);
					from.Target = null;
				}

				from.RevealingAction();

				from.SendLocalizedMessage(500948); // Who will you use the bandages on?

				new InternalTarget(b).Invoke(from, e.Target);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		private class InternalTarget : Target
		{
			private readonly Bandage m_Bandage;

			public InternalTarget(Bandage bandage) : base(Bandage.Range, false, TargetFlags.Beneficial)
			{
				m_Bandage = bandage;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Bandage.Deleted)
				{
					return;
				}

				if (targeted is Mobile)
				{
					if (from.InRange(m_Bandage.GetWorldLocation(), Bandage.Range))
					{
						if (BandageContext.BeginHeal(from, (Mobile)targeted) != null)
						{
							if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
							{
								m_Bandage.Consume();
							}
						}
					}
					else
					{
						from.SendLocalizedMessage(500295); // You are too far away to do that.
					}
				}
				else if (targeted is PlagueBeastInnard)
				{
					if (((PlagueBeastInnard)targeted).OnBandage(from))
					{
						m_Bandage.Consume();
					}
				}
				else
				{
					from.SendLocalizedMessage(500970); // Bandages can not be used on that.
				}
			}

			protected override void OnNonlocalTarget(Mobile from, object targeted)
			{
				if (targeted is PlagueBeastInnard)
				{
					if (((PlagueBeastInnard)targeted).OnBandage(from))
					{
						m_Bandage.Consume();
					}
				}
				else
				{
					base.OnNonlocalTarget(from, targeted);
				}
			}
		}
	}

	public class BandageContext
	{
		private readonly Mobile m_Healer;
		private readonly Mobile m_Patient;
		private int m_Slips;
		private Timer m_Timer;

		public Mobile Healer => m_Healer;
		public Mobile Patient => m_Patient;
		public int Slips { get => m_Slips; set => m_Slips = value; }
		public Timer Timer => m_Timer;

		public void Slip()
		{
			m_Healer.SendLocalizedMessage(500961); // Your fingers slip!
			++m_Slips;
		}

		public BandageContext(Mobile healer, Mobile patient, TimeSpan delay)
		{
			m_Healer = healer;
			m_Patient = patient;

			m_Timer = new InternalTimer(this, delay);
			m_Timer.Start();
		}

		public void StopHeal()
		{
			m_Table.Remove(m_Healer);

			if (m_Timer != null)
			{
				m_Timer.Stop();
			}

			m_Timer = null;
		}

		private static readonly Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

		public static BandageContext GetContext(Mobile healer)
		{
			BandageContext bc = null;
			m_Table.TryGetValue(healer, out bc);
			return bc;
		}

		public static SkillName GetPrimarySkill(Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
			{
				return SkillName.Veterinary;
			}
			else
			{
				return SkillName.Healing;
			}
		}

		public static SkillName GetSecondarySkill(Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
			{
				return SkillName.AnimalLore;
			}
			else
			{
				return SkillName.Anatomy;
			}
		}

		public void EndHeal()
		{
			StopHeal();

			int healerNumber = -1, patientNumber = -1;
			var playSound = true;
			var checkSkills = false;

			var primarySkill = GetPrimarySkill(m_Patient);
			var secondarySkill = GetSecondarySkill(m_Patient);

			var petPatient = m_Patient as BaseCreature;

			if (!m_Healer.Alive)
			{
				healerNumber = 500962; // You were unable to finish your work before you died.
				patientNumber = -1;
				playSound = false;
			}
			else if (!m_Healer.InRange(m_Patient, Bandage.Range))
			{
				healerNumber = 500963; // You did not stay close enough to heal your target.
				patientNumber = -1;
				playSound = false;
			}
			else if (!m_Patient.Alive || (petPatient != null && petPatient.IsDeadPet))
			{
				var healing = m_Healer.Skills[primarySkill].Value;
				var anatomy = m_Healer.Skills[secondarySkill].Value;
				var chance = ((healing - 68.0) / 50.0) - (m_Slips * 0.02);

				if (((checkSkills = (healing >= 80.0 && anatomy >= 80.0)) && chance > Utility.RandomDouble())
					  || (Core.SE && petPatient is Factions.FactionWarHorse && petPatient.ControlMaster == m_Healer))   //TODO: Dbl check doesn't check for faction of the horse here?
				{
					if (m_Patient.Map == null || !m_Patient.Map.CanFit(m_Patient.Location, 16, false, false))
					{
						healerNumber = 501042; // Target can not be resurrected at that location.
						patientNumber = 502391; // Thou can not be resurrected there!
					}
					else if (m_Patient.Region != null && m_Patient.Region.IsPartOf("Khaldun"))
					{
						healerNumber = 1010395; // The veil of death in this area is too strong and resists thy efforts to restore life.
						patientNumber = -1;
					}
					else
					{
						healerNumber = 500965; // You are able to resurrect your patient.
						patientNumber = -1;

						m_Patient.PlaySound(0x214);
						m_Patient.FixedEffect(0x376A, 10, 16);

						if (petPatient != null && petPatient.IsDeadPet)
						{
							var master = petPatient.ControlMaster;

							if (master != null && m_Healer == master)
							{
								petPatient.ResurrectPet();

								for (var i = 0; i < petPatient.Skills.Length; ++i)
								{
									petPatient.Skills[i].Base -= 0.1;
								}
							}
							else if (master != null && master.InRange(petPatient, 3))
							{
								healerNumber = 503255; // You are able to resurrect the creature.

								master.CloseGump(typeof(PetResurrectGump));
								master.SendGump(new PetResurrectGump(m_Healer, petPatient));
							}
							else
							{
								var found = false;

								var friends = petPatient.Friends;

								for (var i = 0; friends != null && i < friends.Count; ++i)
								{
									var friend = friends[i];

									if (friend.InRange(petPatient, 3))
									{
										healerNumber = 503255; // You are able to resurrect the creature.

										friend.CloseGump(typeof(PetResurrectGump));
										friend.SendGump(new PetResurrectGump(m_Healer, petPatient));

										found = true;
										break;
									}
								}

								if (!found)
								{
									healerNumber = 1049670; // The pet's owner must be nearby to attempt resurrection.
								}
							}
						}
						else
						{
							m_Patient.CloseGump(typeof(ResurrectGump));
							m_Patient.SendGump(new ResurrectGump(m_Patient, m_Healer));
						}
					}
				}
				else
				{
					if (petPatient != null && petPatient.IsDeadPet)
					{
						healerNumber = 503256; // You fail to resurrect the creature.
					}
					else
					{
						healerNumber = 500966; // You are unable to resurrect your patient.
					}

					patientNumber = -1;
				}
			}
			else if (m_Patient.Poisoned)
			{
				m_Healer.SendLocalizedMessage(500969); // You finish applying the bandages.

				var healing = m_Healer.Skills[primarySkill].Value;
				var anatomy = m_Healer.Skills[secondarySkill].Value;
				var chance = ((healing - 30.0) / 50.0) - (m_Patient.Poison.Level * 0.1) - (m_Slips * 0.02);

				if ((checkSkills = (healing >= 60.0 && anatomy >= 60.0)) && chance > Utility.RandomDouble())
				{
					if (m_Patient.CurePoison(m_Healer))
					{
						healerNumber = (m_Healer == m_Patient) ? -1 : 1010058; // You have cured the target of all poisons.
						patientNumber = 1010059; // You have been cured of all poisons.
					}
					else
					{
						healerNumber = -1;
						patientNumber = -1;
					}
				}
				else
				{
					healerNumber = 1010060; // You have failed to cure your target!
					patientNumber = -1;
				}
			}
			else if (BleedAttack.IsBleeding(m_Patient))
			{
				healerNumber = 1060088; // You bind the wound and stop the bleeding
				patientNumber = 1060167; // The bleeding wounds have healed, you are no longer bleeding!

				BleedAttack.EndBleed(m_Patient, false);
			}
			else if (MortalStrike.IsWounded(m_Patient))
			{
				healerNumber = (m_Healer == m_Patient ? 1005000 : 1010398);
				patientNumber = -1;
				playSound = false;
			}
			else if (m_Patient.Hits == m_Patient.HitsMax)
			{
				healerNumber = 500967; // You heal what little damage your patient had.
				patientNumber = -1;
			}
			else
			{
				checkSkills = true;
				patientNumber = -1;

				var healing = m_Healer.Skills[primarySkill].Value;
				var anatomy = m_Healer.Skills[secondarySkill].Value;
				var chance = ((healing + 10.0) / 100.0) - (m_Slips * 0.02);

				if (chance > Utility.RandomDouble())
				{
					healerNumber = 500969; // You finish applying the bandages.

					double min, max;

					if (Core.AOS)
					{
						min = (anatomy / 8.0) + (healing / 5.0) + 4.0;
						max = (anatomy / 6.0) + (healing / 2.5) + 4.0;
					}
					else
					{
						min = (anatomy / 5.0) + (healing / 5.0) + 3.0;
						max = (anatomy / 5.0) + (healing / 2.0) + 10.0;
					}

					var toHeal = min + (Utility.RandomDouble() * (max - min));

					if (m_Patient.Body.IsMonster || m_Patient.Body.IsAnimal)
					{
						toHeal += m_Patient.HitsMax / 100;
					}

					if (Core.AOS)
					{
						toHeal -= toHeal * m_Slips * 0.35; // TODO: Verify algorithm
					}
					else
					{
						toHeal -= m_Slips * 4;
					}

					if (toHeal < 1)
					{
						toHeal = 1;
						healerNumber = 500968; // You apply the bandages, but they barely help.
					}

					m_Patient.Heal((int)toHeal, m_Healer, false);
				}
				else
				{
					healerNumber = 500968; // You apply the bandages, but they barely help.
					playSound = false;
				}
			}

			if (healerNumber != -1)
			{
				m_Healer.SendLocalizedMessage(healerNumber);
			}

			if (patientNumber != -1)
			{
				m_Patient.SendLocalizedMessage(patientNumber);
			}

			if (playSound)
			{
				m_Patient.PlaySound(0x57);
			}

			if (checkSkills)
			{
				m_Healer.CheckSkill(secondarySkill, 0.0, 120.0);
				m_Healer.CheckSkill(primarySkill, 0.0, 120.0);
			}
		}

		private class InternalTimer : Timer
		{
			private readonly BandageContext m_Context;

			public InternalTimer(BandageContext context, TimeSpan delay) : base(delay)
			{
				m_Context = context;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				m_Context.EndHeal();
			}
		}

		public static BandageContext BeginHeal(Mobile healer, Mobile patient)
		{
			var isDeadPet = (patient is BaseCreature && ((BaseCreature)patient).IsDeadPet);

			if (patient is Golem)
			{
				healer.SendLocalizedMessage(500970); // Bandages cannot be used on that.
			}
			else if (patient is BaseCreature && ((BaseCreature)patient).IsAnimatedDead)
			{
				healer.SendLocalizedMessage(500951); // You cannot heal that.
			}
			else if (!patient.Poisoned && patient.Hits == patient.HitsMax && !BleedAttack.IsBleeding(patient) && !isDeadPet)
			{
				healer.SendLocalizedMessage(500955); // That being is not damaged!
			}
			else if (!patient.Alive && (patient.Map == null || !patient.Map.CanFit(patient.Location, 16, false, false)))
			{
				healer.SendLocalizedMessage(501042); // Target cannot be resurrected at that location.
			}
			else if (healer.CanBeBeneficial(patient, true, true))
			{
				healer.DoBeneficial(patient);

				var onSelf = (healer == patient);
				var dex = healer.Dex;

				double seconds;
				var resDelay = (patient.Alive ? 0.0 : 5.0);

				if (onSelf)
				{
					if (Core.AOS)
					{
						seconds = 5.0 + (0.5 * ((double)(120 - dex) / 10)); // TODO: Verify algorithm
					}
					else
					{
						seconds = 9.4 + (0.6 * ((double)(120 - dex) / 10));
					}
				}
				else
				{
					if (Core.AOS && GetPrimarySkill(patient) == SkillName.Veterinary)
					{
						seconds = 2.0;
					}
					else if (Core.AOS)
					{
						if (dex < 204)
						{
							seconds = 3.2 - (Math.Sin((double)dex / 130) * 2.5) + resDelay;
						}
						else
						{
							seconds = 0.7 + resDelay;
						}
					}
					else
					{
						if (dex >= 100)
						{
							seconds = 3.0 + resDelay;
						}
						else if (dex >= 40)
						{
							seconds = 4.0 + resDelay;
						}
						else
						{
							seconds = 5.0 + resDelay;
						}
					}
				}

				var context = GetContext(healer);

				if (context != null)
				{
					context.StopHeal();
				}

				seconds *= 1000;

				context = new BandageContext(healer, patient, TimeSpan.FromMilliseconds(seconds));

				m_Table[healer] = context;

				if (!onSelf)
				{
					patient.SendLocalizedMessage(1008078, false, healer.Name); //  : Attempting to heal you.
				}

				healer.SendLocalizedMessage(500956); // You begin applying the bandages.
				return context;
			}

			return null;
		}
	}
}