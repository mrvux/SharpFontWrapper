using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpFontWrapper
{
    /// <summary>
    /// Font wrapper factory, this is use to crate font wrapper instances
    /// </summary>
    public partial class Factory
    {
        private class NativeMethods86
        {
            [DllImport("FW1FontWrapper_x86.dll", EntryPoint = "FW1CreateFactory", CallingConvention = CallingConvention.StdCall)]
            public unsafe static extern int FW1CreateFactory_(int arg0, out IntPtr arg1);
        }

        private class NativeMethods64
        {
            [DllImport("FW1FontWrapper_x64.dll", EntryPoint = "FW1CreateFactory", CallingConvention = CallingConvention.StdCall)]
            public unsafe static extern int FW1CreateFactory_(int arg0, out IntPtr arg1);
        }

        private const int version = 0x110f;

        /// <summary>
        /// Creates a font wrapper factory
        /// </summary>
        public Factory()
        {
            IntPtr factoryPointer;
            int result;

            if (System.Environment.Is64BitProcess)
            {
                result = NativeMethods64.FW1CreateFactory_(version, out factoryPointer);
            }
            else
            {
                result = NativeMethods86.FW1CreateFactory_(version, out factoryPointer);
            }

            SharpDX.Result sr = new SharpDX.Result(result);
            sr.CheckError();

            this.NativePointer = factoryPointer;

        }
    }
}
