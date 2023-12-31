﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct BufferDesc
	{
		public string Name;
		public ulong Size;
		public BindFlags BindFlags;
		public Usage Usage;
		public CpuAccessFlags AccessFlags;
		public BufferMode Mode;
		public uint ElementByteStride;

		public BufferDesc()
		{
			Name = string.Empty;
			Size = 0;
			BindFlags = BindFlags.None;
			Usage = Usage.Default;
			AccessFlags = CpuAccessFlags.None;
			Mode = BufferMode.Undefined;
			ElementByteStride = 0;
		}
	}
	public struct BufferViewFormat
	{
		public ValueType ValueType;
		public byte NumComponents;
		public bool IsNormalized;
	}
	public struct BufferViewDesc
	{
		public string Name;
		public BufferViewType ViewType;
		public BufferViewFormat Format;
		public ulong ByteOffset;
		public ulong ByteWidth;
	}
	public interface IBuffer : IGPUObject, IGPUState, IGPUHandler
	{
		public BufferDesc Desc { get; }
		public ulong Size { get; }
		public IBufferView GetDefaultView(BufferViewType viewType);
		public IBufferView CreateView(BufferViewDesc desc);
	}

	public interface IBufferView : IGPUObject
	{
		public IBuffer Buffer { get; }
	}
}
