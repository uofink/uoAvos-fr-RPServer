﻿using Server.Engines.Publishing;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.SkillHandlers
{
	public class Inscribe
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Inscribe].Callback = new SkillUseCallback(OnUse);
		}

		public static TimeSpan OnUse(Mobile m)
		{
			Target target = new InternalTargetSrc();
			m.Target = target;
			m.SendLocalizedMessage(1046295); // Target the book you wish to copy.
			target.BeginTimeout(m, TimeSpan.FromMinutes(1.0));

			return TimeSpan.FromSeconds(1.0);
		}

		private static readonly Dictionary<BaseBook, Mobile> m_UseTable = new Dictionary<BaseBook, Mobile>();

		private static void SetUser(BaseBook book, Mobile mob)
		{
			m_UseTable[book] = mob;
		}

		private static void CancelUser(BaseBook book)
		{
			m_UseTable.Remove(book);
		}

		public static Mobile GetUser(BaseBook book)
		{
			Mobile m = null;
			m_UseTable.TryGetValue(book, out m);
			return m;
		}

		public static bool IsEmpty(BaseBook book)
		{
			foreach (var page in book.Pages)
			{
				foreach (var line in page.Lines)
				{
					if (line.Trim().Length != 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static void Copy(BaseBook bookSrc, BaseBook bookDst)
		{
			bookDst.Title = bookSrc.Title;
			bookDst.Author = bookSrc.Author;

			var pagesSrc = bookSrc.Pages;
			var pagesDst = bookDst.Pages;
			for (var i = 0; i < pagesSrc.Length && i < pagesDst.Length; i++)
			{
				var pageSrc = pagesSrc[i];
				var pageDst = pagesDst[i];

				var length = pageSrc.Lines.Length;
				pageDst.Lines = new string[length];

				for (var j = 0; j < length; j++)
				{
					pageDst.Lines[j] = pageSrc.Lines[j];
				}
			}
		}

		private class InternalTargetSrc : Target
		{
			public InternalTargetSrc() : base(3, false, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				var book = targeted as BaseBook;
				if (book == null)
				{
					from.SendLocalizedMessage(1046296); // That is not a book
				}
				else if (Inscribe.IsEmpty(book))
				{
					from.SendLocalizedMessage(501611); // Can't copy an empty book.
				}
				else if (Inscribe.GetUser(book) != null)
				{
					from.SendLocalizedMessage(501621); // Someone else is inscribing that item.
				}
				else
				{
					Target target = new InternalTargetDst(book);
					from.Target = target;
					from.SendLocalizedMessage(501612); // Select a book to copy this to.
					target.BeginTimeout(from, TimeSpan.FromMinutes(1.0));
					Inscribe.SetUser(book, from);
				}
			}

			protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
			{
				if (cancelType == TargetCancelType.Timeout)
				{
					from.SendLocalizedMessage(501619); // You have waited too long to make your inscribe selection, your inscription attempt has timed out.
				}
			}
		}

		private class InternalTargetDst : Target
		{
			private readonly BaseBook m_BookSrc;

			public InternalTargetDst(BaseBook bookSrc) : base(3, false, TargetFlags.None)
			{
				m_BookSrc = bookSrc;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_BookSrc.Deleted)
				{
					return;
				}

				var bookDst = targeted as BaseBook;

				if (bookDst == null)
				{
					from.SendLocalizedMessage(1046296); // That is not a book
				}
				else if (Inscribe.IsEmpty(m_BookSrc))
				{
					from.SendLocalizedMessage(501611); // Can't copy an empty book.
				}
				else if (bookDst == m_BookSrc)
				{
					from.SendLocalizedMessage(501616); // Cannot copy a book onto itself.
				}
				else if (!bookDst.Writable)
				{
					from.SendLocalizedMessage(501614); // Cannot write into that book.
				}
				else if (Inscribe.GetUser(bookDst) != null)
				{
					from.SendLocalizedMessage(501621); // Someone else is inscribing that item.
				}
				else
				{
					if (from.CheckTargetSkill(SkillName.Inscribe, bookDst, 0, 50))
					{
						Inscribe.Copy(m_BookSrc, bookDst);

						from.SendLocalizedMessage(501618); // You make a copy of the book.
						from.PlaySound(0x249);
					}
					else
					{
						from.SendLocalizedMessage(501617); // You fail to make a copy of the book.
					}
				}
			}

			protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
			{
				if (cancelType == TargetCancelType.Timeout)
				{
					from.SendLocalizedMessage(501619); // You have waited too long to make your inscribe selection, your inscription attempt has timed out.
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				Inscribe.CancelUser(m_BookSrc);
			}
		}
	}
}