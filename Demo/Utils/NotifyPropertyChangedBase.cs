using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Com.PhilChuang.Utils;
using Com.PhilChuang.Utils.MvvmNotificationChainer;

namespace Demo.Utils
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected event PropertyChangedEventHandler PropertyChangedInternal = delegate { };

        protected virtual void RaisePropertyChanged<T> (Expression<Func<T>> propertyExpression)
        {
            RaisePropertyChanged (propertyExpression.GetPropertyName ());
        }

        protected virtual void RaisePropertyChanged ([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged (new PropertyChangedEventArgs (propertyName));
        }

        protected virtual void RaisePropertyChanged (PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                if (args.PropertyName != "PropertyChangedOutput")
                    AppendPropertyChangedOutput ("[PropertyChanged] " + args.PropertyName);
                var handler = PropertyChanged;
                handler (this, args);
            }
            RaisePropertyChangedInternal (args);
        }

        protected virtual void RaisePropertyChangedInternal ([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChangedInternal (new PropertyChangedEventArgs (propertyName));
        }

        protected virtual void RaisePropertyChangedInternal (PropertyChangedEventArgs args)
        {
            if (PropertyChangedInternal != null)
            {
                if (args.PropertyName != "PropertyChangedOutput")
                    AppendPropertyChangedOutput ("[PropertyChangedInternal] " + args.PropertyName);
                var handler = PropertyChangedInternal;
                handler (this, args);
            }
        }

        private StringBuilder myPropertyChangedOutput = new StringBuilder ();
        private void AppendPropertyChangedOutput (String line)
        {
            myPropertyChangedOutput.AppendFormat ("[{0:s}] {1}\r\n", DateTime.Now, line);
            RaisePropertyChanged (() => PropertyChangedOutput);
        }

        public String PropertyChangedOutput
        { get { return myPropertyChangedOutput.ToString (); } }

        protected readonly NotificationChainManager myNotificationChainManager = new NotificationChainManager ();

        protected NotifyPropertyChangedBase ()
        {
            myNotificationChainManager.SetDefaultNotifyingObject (this, h => PropertyChangedInternal += h, h => PropertyChangedInternal -= h);
            myNotificationChainManager.AddDefaultCall ((notifyingProperty, dependentProperty) => RaisePropertyChanged (dependentProperty));
        }

        public virtual void Dispose ()
        {
            myNotificationChainManager.Dispose ();
        }
    }
}