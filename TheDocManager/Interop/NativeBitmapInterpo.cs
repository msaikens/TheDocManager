using System;
using System.Runtime.InteropServices;

namespace TheDocManager.Interop
{
   public static partial class NativeBitmapInterop
    {
        //        [LibraryImport("BitmapConverter.dll")]
        //        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        //        [return: MarshalAs(UnmanagedType.Bool)]
        //        public static partial bool InitGDIPlus();

        //        [LibraryImport("BitmapConverter.dll")]
        //        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        //        internal static partial void ShutdownGDIPlus();

        //        [LibraryImport("BitmapConverter.dll", StringMarshalling = StringMarshalling.Utf16)]
        //        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        //        [return: MarshalAs(UnmanagedType.Bool)]
        //        public static partial bool ConvertBitmapToBuffer(
        //            string filePath,
        //            out IntPtr buffer,
        //            out int width,
        //            out int height,
        //            out int stride);
        //    }
        //}
        [LibraryImport("BitmapConverter.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ConvertAndProcessBitmap(
            IntPtr inputBuffer,
            int width,
            int height,
            int stride,
            int inputFormat,
            int outputFormat,
            int rotateDegrees,
            [MarshalAs(UnmanagedType.Bool)] bool flipHorizontal,
            [MarshalAs(UnmanagedType.Bool)] bool flipVertical,
            [MarshalAs(UnmanagedType.Bool)] bool compressToPng,
            out IntPtr outBuffer,
            out int outSize,
            out int outDpiX,
            out int outDpiY);

        [LibraryImport("BitmapConverter.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool InitGDIPlus();

        [LibraryImport("BitmapConverter.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static partial void ShutdownGDIPlus();

        [LibraryImport("BitmapConverter.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial void FreeBuffer(IntPtr buffer);
    }
}
