﻿using System.Globalization;
using System.Reflection;
using Transformalize.Libs.Jint.Native;

namespace Transformalize.Libs.Jint.Runtime.Descriptors.Specialized
{
    public sealed class PropertyInfoDescriptor : PropertyDescriptor
    {
        private readonly Engine _engine;
        private readonly PropertyInfo _propertyInfo;
        private readonly object _item;

        public PropertyInfoDescriptor(Engine engine, PropertyInfo propertyInfo, object item)
        {
            _engine = engine;
            _propertyInfo = propertyInfo;
            _item = item;

            Writable = propertyInfo.CanWrite;
        }

        public override JsValue? Value
        {
            get
            {
                return JsValue.FromObject(_engine, _propertyInfo.GetValue(_item, null));
            }

            set
            {
                var currentValue = value.GetValueOrDefault();
                object obj;
                if (_propertyInfo.PropertyType == typeof (JsValue))
                {
                    obj = currentValue;
                }
                else
                {
                    // attempt to convert the JsValue to the target type
                    obj = currentValue.ToObject();
                    if (obj.GetType() != _propertyInfo.PropertyType)
                    {
                        obj = _engine.Options.GetTypeConverter().Convert(obj, _propertyInfo.PropertyType, CultureInfo.InvariantCulture);
                    }
                }
                
                _propertyInfo.SetValue(_item, obj, null);
            }
        }
    }
}