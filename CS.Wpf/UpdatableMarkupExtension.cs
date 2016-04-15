using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace CS.Wpf
{
    public abstract class UpdatableMarkupExtension : MarkupExtension
    {
        protected object TargetObject => _targetObject.Target;
        protected object TargetProperty => _targetProperty.Target;

        private WeakReference _targetObject;
        private WeakReference _targetProperty;

        public sealed override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (target != null)
            {
                _targetObject = new WeakReference(target.TargetObject);
                _targetProperty = new WeakReference(target.TargetProperty);
            }

            return ProvideInitialValue(serviceProvider);
        }

        protected void UpdateValue(object value)
        {
            if (_targetObject == null || !_targetObject.IsAlive || _targetProperty == null || !_targetProperty.IsAlive)
                return;

            DependencyObject obj = _targetObject.Target as DependencyObject;
            object tProp = _targetProperty.Target;

            if (tProp is DependencyProperty)
            {
                DependencyProperty prop = tProp as DependencyProperty;
                Action updateAction = () => obj.SetValue(prop, value);

                // Check whether the target object can be accessed from the
                // current thread, and use Dispatcher.Invoke if it can't
                if (obj.CheckAccess())
                    updateAction();
                else
                    obj.Dispatcher.Invoke(updateAction);
            }
            else if (tProp is PropertyInfo)
            {
                PropertyInfo prop = tProp as PropertyInfo;
                prop.SetValue(_targetObject, value, null);
            }
        }

        protected abstract object ProvideInitialValue(IServiceProvider serviceProvider);
    }
}
