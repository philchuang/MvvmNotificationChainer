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
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };  
        protected event PropertyChangedEventHandler PropertyChangedInternal = delegate { };

        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            RaisePropertyChanged(propertyExpression.GetPropertyName());
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                var handler = PropertyChanged;
                handler(this, args);
            }
            if (PropertyChangedInternal != null)
            {
                var handler = PropertyChangedInternal;
                handler(this, args);
            }
        }

        protected readonly ChainedNotificationManager myChainedNotifications = new ChainedNotificationManager();

        public virtual void Dispose()
        {
            myChainedNotifications.Dispose();
        }

        protected ChainedNotification CreateChain([CallerMemberName] String propertyName = null)
        {
            return
                myChainedNotifications.Get(propertyName)
                ?? myChainedNotifications
                       .Create(propertyName)
                       .AndSetDefaultNotifier(this, h => PropertyChangedInternal += h, h => PropertyChangedInternal -= h)
                       .AndCall(RaisePropertyChanged);
        }
    }
}
