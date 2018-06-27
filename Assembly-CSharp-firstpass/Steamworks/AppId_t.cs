﻿using System;

namespace Steamworks
{
	public struct AppId_t : IEquatable<AppId_t>, IComparable<AppId_t>
	{
		public static readonly AppId_t Invalid = new AppId_t(0u);

		public uint m_AppId;

		public AppId_t(uint value)
		{
			this.m_AppId = value;
		}

		public override string ToString()
		{
			return this.m_AppId.ToString();
		}

		public override bool Equals(object other)
		{
			return other is AppId_t && this == (AppId_t)other;
		}

		public override int GetHashCode()
		{
			return this.m_AppId.GetHashCode();
		}

		public static bool operator ==(AppId_t x, AppId_t y)
		{
			return x.m_AppId == y.m_AppId;
		}

		public static bool operator !=(AppId_t x, AppId_t y)
		{
			return !(x == y);
		}

		public static explicit operator AppId_t(uint value)
		{
			return new AppId_t(value);
		}

		public static explicit operator uint(AppId_t that)
		{
			return that.m_AppId;
		}

		public bool Equals(AppId_t other)
		{
			return this.m_AppId == other.m_AppId;
		}

		public int CompareTo(AppId_t other)
		{
			return this.m_AppId.CompareTo(other.m_AppId);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static AppId_t()
		{
		}
	}
}