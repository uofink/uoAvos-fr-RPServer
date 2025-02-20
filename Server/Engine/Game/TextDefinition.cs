﻿using Server.Gumps;
using Server.Network;

using System;
using System.Globalization;

namespace Server
{
	[Parsable]
	public class TextDefinition
	{
		private readonly int m_Number;
		private readonly string m_String;

		public int Number => m_Number;
		public string String => m_String;

		public bool IsEmpty => m_Number <= 0 && m_String == null;

		public TextDefinition() : this(0, null)
		{
		}

		public TextDefinition(int number) : this(number, null)
		{
		}

		public TextDefinition(string text) : this(0, text)
		{
		}

		public TextDefinition(int number, string text)
		{
			m_Number = number;
			m_String = text;
		}

		public override string ToString()
		{
			if (m_Number > 0)
			{
				return $"#{m_Number}";
			}
			
			if (m_String != null)
			{
				return m_String;
			}

			return "";
		}

		public string Format(bool propsGump)
		{
			if (m_Number > 0)
			{
				return $"{m_Number} (0x{m_Number:X})";
			}
			
			if (m_String != null)
			{
				return $"\"{m_String}\"";
			}

			return propsGump ? "-empty-" : "empty";
		}

		public string Combine()
		{
			if (m_Number > 0)
			{
				var cli = StringList.Localization.GetEntry(m_Number);

				if (cli != null)
				{
					if (m_String != null)
					{
						return cli.ToString(m_String);
					}

					return cli.ToString();
				}
			}

			return ToString();
		}

		public string GetValue()
		{
			if (m_Number > 0)
			{
				return m_Number.ToString();
			}
			
			if (m_String != null)
			{
				return m_String;
			}

			return "";
		}

		public static void Serialize(GenericWriter writer, TextDefinition def)
		{
			if (def == null)
			{
				writer.WriteEncodedInt(3);
			}
			else if (def.m_Number > 0)
			{
				writer.WriteEncodedInt(1);
				writer.WriteEncodedInt(def.m_Number);
			}
			else if (def.m_String != null)
			{
				writer.WriteEncodedInt(2);
				writer.Write(def.m_String);
			}
			else
			{
				writer.WriteEncodedInt(0);
			}
		}

		public static TextDefinition Deserialize(GenericReader reader)
		{
			var type = reader.ReadEncodedInt();

			switch (type)
			{
				case 0: return new TextDefinition();
				case 1: return new TextDefinition(reader.ReadEncodedInt());
				case 2: return new TextDefinition(reader.ReadString());
			}

			return null;
		}

		public static void AddTo(ObjectPropertyList list, TextDefinition def)
		{
			if (def == null)
			{
				return;
			}

			if (def.m_Number > 0)
			{
				list.Add(def.m_Number);
			}
			else if (def.m_String != null)
			{
				list.Add(def.m_String);
			}
		}

		public static void AddHtmlText(Gump g, int x, int y, int width, int height, TextDefinition def, bool back, bool scroll, short numberColor, int stringColor)
		{
			if (def == null)
			{
				return;
			}

			if (def.m_Number > 0)
			{
				if (numberColor >= 0) // 5 bits per RGB component (15 bit RGB)
				{
					g.AddHtmlLocalized(x, y, width, height, def.m_Number, numberColor, back, scroll);
				}
				else
				{
					g.AddHtmlLocalized(x, y, width, height, def.m_Number, back, scroll);
				}
			}
			else if (def.m_String != null)
			{
				if (stringColor >= 0) // 8 bits per RGB component (24 bit RGB)
				{
					g.AddHtml(x, y, width, height, String.Format("<BASEFONT COLOR=#{0:X6}>{1}", stringColor & 0x00FFFFFF, def.m_String), back, scroll);
				}
				else
				{
					g.AddHtml(x, y, width, height, def.m_String, back, scroll);
				}
			}
		}

		public static void AddHtmlText(Gump g, int x, int y, int width, int height, TextDefinition def, bool back, bool scroll)
		{
			AddHtmlText(g, x, y, width, height, def, back, scroll, -1, -1);
		}

		public static void SendMessageTo(Mobile m, TextDefinition def)
		{
			if (def == null)
			{
				return;
			}

			if (def.m_Number > 0)
			{
				m.SendLocalizedMessage(def.m_Number);
			}
			else if (def.m_String != null)
			{
				m.SendMessage(def.m_String);
			}
		}

		public static void SendMessageTo(Mobile m, TextDefinition def, int hue)
		{
			if (def == null)
			{
				return;
			}

			if (def.m_Number > 0)
			{
				m.SendLocalizedMessage(def.m_Number, "", hue);
			}
			else if (def.m_String != null)
			{
				m.SendMessage(hue, def.m_String);
			}
		}

		public static void PublicOverheadMessage(Mobile m, MessageType messageType, int hue, TextDefinition def)
		{
			if (def == null)
			{
				return;
			}

			if (def.m_Number > 0)
			{
				m.PublicOverheadMessage(messageType, hue, def.m_Number);
			}
			else if (def.m_String != null)
			{
				m.PublicOverheadMessage(messageType, hue, false, def.m_String);
			}
		}

		public static TextDefinition Parse(string value)
		{
			if (value == null)
			{
				return null;
			}

			int i;
			bool isInteger;

			if (value.StartsWith("0x"))
			{
				isInteger = Int32.TryParse(value.AsSpan(2), NumberStyles.HexNumber, null, out i);
			}
			else
			{
				isInteger = Int32.TryParse(value, out i);
			}

			if (isInteger)
			{
				value = null;
			}
			
			return new TextDefinition(i, value);
		}

		public static bool IsNullOrEmpty(TextDefinition def)
		{
			return def?.IsEmpty != false;
		}

		public static implicit operator TextDefinition(int v)
		{
			return new TextDefinition(v);
		}

		public static implicit operator TextDefinition(string s)
		{
			return new TextDefinition(s);
		}

		public static implicit operator int(TextDefinition m)
		{
			return m?.m_Number ?? 0;
		}

		public static implicit operator string(TextDefinition m)
		{
			return m?.m_String;
		}
	}
}