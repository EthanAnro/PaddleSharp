﻿using Sdcb.PaddleInference.Native;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Sdcb.PaddleInference
{
    public class PaddleConfig : IDisposable
	{
		private IntPtr _ptr;

		public PaddleConfig()
		{
			_ptr = PaddleNative.PD_ConfigCreate();
		}

		public PaddleConfig(IntPtr configPointer)
		{
			_ptr = configPointer;
		}

		static PaddleConfig()
		{
			Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + @"C:\_\3rd\paddle\dll");
		}

		public static string Version => Marshal.PtrToStringUTF8(PaddleNative.PD_GetVersion());

		public bool GLogEnabled
		{
			get => PaddleNative.PD_ConfigGlogInfoDisabled() == 0;
			set
			{
				if (!value)
				{
					PaddleNative.PD_ConfigDisableGlogInfo(_ptr);
				}
				else if (!GLogEnabled)
				{
					Console.WriteLine($"Warn: Glog cannot re-enable after disabled.");
				}
			}
		}

		public bool IsMemoryModel => PaddleNative.PD_ConfigModelFromMemory(_ptr) != 0;
		public bool MkldnnEnabled
		{
			get => PaddleNative.PD_ConfigMkldnnEnabled(_ptr) != 0;
			set
			{
				if (value)
				{
					PaddleNative.PD_ConfigEnableMKLDNN(_ptr);
				}
				else if (MkldnnEnabled)
				{
					Console.WriteLine($"Warn: Mkldnn cannot disabled after enabled.");
				}
			}
		}

		private int _MkldnnCacheCapacity = 0;
		public int MkldnnCacheCapacity
		{
			get => _MkldnnCacheCapacity;
			set
			{
				_MkldnnCacheCapacity = value;
				PaddleNative.PD_ConfigSetMkldnnCacheCapacity(_ptr, value);
			}
		}

		//public string ModelDir
		//{
		//	get => Marshal.PtrToStringUTF8(PdInvoke.PD_ConfigGetModelDir(_ptr));
		//	set => PdInvoke.PD_ConfigSetModelDir(_ptr, value);
		//}

		public void SetModel(string programPath, string paramsPath)
		{
			if (programPath == null) throw new ArgumentNullException(nameof(programPath));
			if (paramsPath == null) throw new ArgumentNullException(nameof(paramsPath));
			if (!File.Exists(programPath)) throw new FileNotFoundException("programPath not found", programPath);
			if (!File.Exists(paramsPath)) throw new FileNotFoundException("paramsPath not found", paramsPath);
			PaddleNative.PD_ConfigSetModel(_ptr, programPath, paramsPath);
		}

		public string ProgramPath => Marshal.PtrToStringUTF8(PaddleNative.PD_ConfigGetProgFile(_ptr));
		public string ParamsPath => Marshal.PtrToStringUTF8(PaddleNative.PD_ConfigGetParamsFile(_ptr));

		public unsafe void SetMemoryModel(byte[] programBuffer, byte[] paramsBuffer)
		{
			fixed (byte* pprogram = programBuffer)
			fixed (byte* pparams = paramsBuffer)
			{
				PaddleNative.PD_ConfigSetModelBuffer(_ptr,
					(IntPtr)pprogram, programBuffer.Length,
					(IntPtr)pparams, paramsBuffer.Length);
			}
		}

		public int CpuMathThreadCount
		{
			get => PaddleNative.PD_ConfigGetCpuMathLibraryNumThreads(_ptr);
			set => PaddleNative.PD_ConfigSetCpuMathLibraryNumThreads(_ptr, value);
		}

		public PaddlePredictor CreatePredictor()
		{
			try
			{
				return new PaddlePredictor(PaddleNative.PD_PredictorCreate(_ptr));
			}
			finally
			{
				_ptr = IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			if (_ptr != IntPtr.Zero)
			{
				PaddleNative.PD_ConfigDestroy(_ptr);
				_ptr = IntPtr.Zero;
			}
		}
	}
}
