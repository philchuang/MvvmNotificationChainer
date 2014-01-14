using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*
 * By including these, the compiler will honor these attributes even in a PCL!
 * They are declared as internal so that they won't conflict when this PCL is referenced by non-PCLs
 */

namespace System.Runtime.CompilerServices
{
    // Summary:
    //     Allows you to obtain the method or property name of the caller to the method.
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class CallerMemberNameAttribute : Attribute { }

    // Summary:
    //     Allows you to obtain the line number in the source file at which the method
    //     is called.
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class CallerLineNumberAttribute : Attribute { }

    // Summary:
    //     Allows you to obtain the full path of the source file that contains the caller.
    //     This is the file path at the time of compile.
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class CallerFilePathAttribute : Attribute { }
}