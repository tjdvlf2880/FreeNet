// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Sessions
{
	public struct AddNotifySendSessionNativeInviteRequestedOptions
	{
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct AddNotifySendSessionNativeInviteRequestedOptionsInternal : ISettable<AddNotifySendSessionNativeInviteRequestedOptions>, System.IDisposable
	{
		private int m_ApiVersion;

		public void Set(ref AddNotifySendSessionNativeInviteRequestedOptions other)
		{
			m_ApiVersion = SessionsInterface.AddnotifysendsessionnativeinviterequestedApiLatest;
		}

		public void Set(ref AddNotifySendSessionNativeInviteRequestedOptions? other)
		{
			if (other.HasValue)
			{
				m_ApiVersion = SessionsInterface.AddnotifysendsessionnativeinviterequestedApiLatest;
			}
		}

		public void Dispose()
		{
		}
	}
}