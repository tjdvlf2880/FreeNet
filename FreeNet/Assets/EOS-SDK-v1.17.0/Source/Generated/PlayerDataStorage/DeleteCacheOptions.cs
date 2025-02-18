// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.PlayerDataStorage
{
	/// <summary>
	/// Input data for the <see cref="TitleStorage.TitleStorageInterface.DeleteCache" /> function
	/// </summary>
	public struct DeleteCacheOptions
	{
		/// <summary>
		/// Product User ID of the local user who is deleting his cache
		/// </summary>
		public ProductUserId LocalUserId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct DeleteCacheOptionsInternal : ISettable<DeleteCacheOptions>, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public void Set(ref DeleteCacheOptions other)
		{
			m_ApiVersion = PlayerDataStorageInterface.DeletecacheApiLatest;
			LocalUserId = other.LocalUserId;
		}

		public void Set(ref DeleteCacheOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = PlayerDataStorageInterface.DeletecacheApiLatest;
				LocalUserId = other.Value.LocalUserId;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_LocalUserId);
		}
	}
}