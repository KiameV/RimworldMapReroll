﻿using System;
using System.Reflection;
using Verse;

namespace MapReroll {
	public static class MapRerollUtility {

		public static bool IsModActive(string modName) {
			foreach (InstalledMod current in InstalledModLister.AllInstalledMods) {
				if (modName.Equals(current.Name) && current.Active) {
					return true;
				}
			}
			return false;
		}


		/**
		 * Code borrowed from the Community Core Library to avoid incurring a dependency.
		 * Hopefully just a temporary solution to a compatibility issue.
		 **/
		public static unsafe bool TryDetourFromTo ( MethodInfo source, MethodInfo destination ){
            if( IntPtr.Size == sizeof( Int64 ) )
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                long Source_Base        = source     .MethodHandle.GetFunctionPointer().ToInt64();
                long Destination_Base   = destination.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                long* Pointer_Raw_Address = (long*)( Pointer_Raw_Source + 0x02 );

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *( Pointer_Raw_Source + 0x00 ) = 0x48;
                *( Pointer_Raw_Source + 0x01 ) = 0xB8;
                *Pointer_Raw_Address           = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *( Pointer_Raw_Source + 0x0A ) = 0xFF;
                *( Pointer_Raw_Source + 0x0B ) = 0xE0;

            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                int Source_Base        = source     .MethodHandle.GetFunctionPointer().ToInt32();
                int Destination_Base   = destination.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                byte* Pointer_Raw_Source = (byte*)Source_Base;

                // Pointer to insert jump address into native code
                int* Pointer_Raw_Address = (int*)( Pointer_Raw_Source + 1 );

                // Jump offset (less instruction size)
                int offset = ( Destination_Base - Source_Base ) - 5;

                // Insert 32-bit relative jump into native code
                *Pointer_Raw_Source = 0xE9;
                *Pointer_Raw_Address = offset;
            }

            // done!
            return true;
        }
	}
}