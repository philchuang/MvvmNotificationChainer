using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Com.PhilChuang.Utils.MvvmNotificationChainer
{
    [AttributeUsage (AttributeTargets.Property)]
    public class NotificationChainPropertyAttribute : Attribute
    {
        /// <summary>
        /// Looks for every property decorated with the NotificationChainPropertyAttribute and calls the getter, which should initialize any NotificationChains.
        /// </summary>
        /// <param name="obj"></param>
        public static void CallProperties (object obj)
        {
            obj.ThrowIfNull ("obj");

            var objType = obj.GetType ();

            foreach (var prop in
                objType.GetProperties (BindingFlags.Public | BindingFlags.Instance)
                       .Union (objType.GetProperties (BindingFlags.NonPublic | BindingFlags.Instance))
                       .Union (objType.GetProperties (BindingFlags.Public | BindingFlags.Static))
                       .Union (objType.GetProperties (BindingFlags.NonPublic | BindingFlags.Static)))
            {
                if (!prop.GetCustomAttributes (typeof (NotificationChainPropertyAttribute), true).Any ()) continue;
                var propGetter = prop.GetGetMethod ();
                if (propGetter == null) continue;
                if (propGetter.GetParameters ().Any ())
                    throw new InvalidOperationException ("NotificationChainPropertyAttribute cannot be applied to property {0}.{1} because it has parameters."
                                                             .FormatWith (prop.DeclaringType.FullName, prop.Name));
                var value = propGetter.Invoke (!propGetter.IsStatic ? obj : null, null);
            }

            // TODO think about this more
            //foreach (var method in
            //    objType.GetMethods (BindingFlags.Public | BindingFlags.Instance)
            //           .Union (objType.GetMethods (BindingFlags.NonPublic | BindingFlags.Instance))
            //           .Union (objType.GetMethods (BindingFlags.Public | BindingFlags.Static))
            //           .Union (objType.GetMethods (BindingFlags.NonPublic | BindingFlags.Static)))
            //{
            //    if (!method.GetCustomAttributes (typeof (NotificationChainPropertyAttribute), true).Any ()) continue;
            //    if (method.GetParameters ().Any ())
            //        throw new InvalidOperationException ("NotificationChainPropertyAttribute cannot be applied to method {0}.{1} because it has parameters."
            //                                                 .FormatWith (method.DeclaringType.FullName, method.Name));
            //    var value = method.Invoke (!method.IsStatic ? obj : null, null);
            //}
        }
    }
}