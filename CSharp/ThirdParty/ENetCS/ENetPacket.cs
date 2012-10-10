﻿#region License

/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

#endregion

using System;
using System.Runtime.InteropServices;

namespace ENet
{
	public sealed unsafe class ENetPacket : IDisposable
	{
		private Native.ENetPacket* packet;

		public ENetPacket(Native.ENetPacket* packet)
		{
			if (packet == null)
			{
				throw new InvalidOperationException("No native packet.");
			}
			this.packet = packet;
		}

		public ENetPacket(): this(new byte[]{})
		{
		}

		public ENetPacket(byte[] data): this(data, 0, data.Length)
		{
		}

		public ENetPacket(byte[] data, int offset, int length, PacketFlags flags = PacketFlags.None)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (offset < 0 || length < 0 || length > data.Length - offset)
			{
				throw new ArgumentOutOfRangeException();
			}
			fixed (byte* bytes = data)
			{
				this.packet = Native.enet_packet_create(bytes + offset, (uint)length, flags);
				if (this.packet == null)
				{
					throw new ENetException(0, "Packet creation call failed.");
				}
			}
		}

		~ENetPacket()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.packet == null)
			{
				return;
			}

			if (disposing)
			{
				if (this.packet->referenceCount == IntPtr.Zero)
				{
					Native.enet_packet_destroy(this.packet);
				}
				this.packet = null;
			}
		}

		public void CopyTo(byte[] array)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			this.CopyTo(array, 0, array.Length);
		}

		public void CopyTo(byte[] array, int offset, int length)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (offset < 0 || length < 0 || length > array.Length - offset)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (length > this.Length - offset)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (length > 0)
			{
				Marshal.Copy((IntPtr) ((byte*) this.Data + offset), array, offset, length);
			}
		}

		public byte[] GetBytes()
		{
			var array = new byte[this.Length];
			this.CopyTo(array);
			return array;
		}

		public void Resize(int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			int ret = Native.enet_packet_resize(this.packet, (uint)length);
			if (ret < 0)
			{
				throw new ENetException(ret, "Packet resize call failed.");
			}
		}

		public void* Data
		{
			get
			{
				return this.packet->data;
			}
		}

		public int Length
		{
			get
			{
				if (this.packet->dataLength.ToPointer() > (void*) int.MaxValue)
				{
					throw new ENetException(0, "Packet too long!");
				}
				return (int) this.packet->dataLength;
			}
		}

		public Native.ENetPacket* NativeData
		{
			get
			{
				return this.packet;
			}
			set
			{
				this.packet = value;
			}
		}
	}
}