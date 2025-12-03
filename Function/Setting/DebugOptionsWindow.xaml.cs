using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using PackageManager.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PackageManager.Function.Setting
{
    public partial class DebugOptionsWindow : Window
    {
        private readonly string _localPath;
        private JObject _data;
        public DebugOptionsWindow(string localPath)
        {
            InitializeComponent();
            _localPath = localPath;
            LoadData();
        }

        private string GetDebugSettingPath()
        {
            return System.IO.Path.Combine(_localPath ?? string.Empty, "config", "DebugSetting.json");
        }

        private void LoadData()
        {
            var p = GetDebugSettingPath();
            if (File.Exists(p))
            {
                var json = File.ReadAllText(p);
                _data = string.IsNullOrWhiteSpace(json) ? new JObject() : JObject.Parse(json);
            }
            else
            {
                _data = new JObject();
            }

            PropertyGrid.SelectedObject = new DynamicPropertyBag(_data);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dir = System.IO.Path.Combine(_localPath ?? string.Empty, "config");
            Directory.CreateDirectory(dir);
            var path = GetDebugSettingPath();
            var json = _data.ToString(Formatting.Indented);
            File.WriteAllText(path, json);
            LoggingService.LogInfo($"调试配置已保存: {path}");
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class DynamicPropertyBag : ICustomTypeDescriptor
    {
        private readonly JObject _data;
        private readonly PropertyDescriptorCollection _props;

        public DynamicPropertyBag(JObject data)
        {
            _data = data ?? new JObject();
            var list = new List<PropertyDescriptor>();
            foreach (var p in _data.Properties())
            {
                list.Add(new DictionaryPropertyDescriptor(_data, p.Name));
            }
            _props = new PropertyDescriptorCollection(list.ToArray());
        }

        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => nameof(DynamicPropertyBag);
        public string GetComponentName() => null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => _props;
        public PropertyDescriptorCollection GetProperties() => _props;
        public object GetPropertyOwner(PropertyDescriptor pd) => this;
    }

    public class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        private readonly JObject _data;
        private readonly string _name;

        public DictionaryPropertyDescriptor(JObject data, string name) : base(
            name,
            new Attribute[]
            {
                new DisplayNameAttribute(name),
                new CategoryAttribute("调试")
            })
        {
            _data = data;
            _name = name;
        }

        public override bool CanResetValue(object component) => false;
        public override Type ComponentType => typeof(DynamicPropertyBag);
        public override object GetValue(object component)
        {
            var t = _data[_name];
            if (t == null) return null;
            if (t.Type == JTokenType.Boolean) return t.Value<bool>();
            if (t.Type == JTokenType.Integer) return t.Value<long>();
            if (t.Type == JTokenType.Float) return t.Value<double>();
            return t.ToString();
        }
        public override bool IsReadOnly => false;
        public override Type PropertyType
        {
            get
            {
                var t = _data[_name];
                if (t == null) return typeof(string);
                if (t.Type == JTokenType.Boolean) return typeof(bool);
                if (t.Type == JTokenType.Integer) return typeof(long);
                if (t.Type == JTokenType.Float) return typeof(double);
                return typeof(string);
            }
        }
        public override void ResetValue(object component) { }
        public override void SetValue(object component, object value)
        {
            var t = _data[_name];
            if (t == null)
            {
                _data[_name] = JToken.FromObject(value ?? string.Empty);
                return;
            }
            if (t.Type == JTokenType.Boolean)
            {
                bool v;
                if (value is bool b) v = b; else v = string.Equals(Convert.ToString(value), "true", StringComparison.OrdinalIgnoreCase);
                _data[_name] = v;
                return;
            }
            if (t.Type == JTokenType.Integer)
            {
                long v;
                if (value is long l) v = l; else long.TryParse(Convert.ToString(value), out v);
                _data[_name] = v;
                return;
            }
            if (t.Type == JTokenType.Float)
            {
                double v;
                if (value is double d) v = d; else double.TryParse(Convert.ToString(value), out v);
                _data[_name] = v;
                return;
            }
            _data[_name] = JToken.FromObject(value ?? string.Empty);
        }
        public override bool ShouldSerializeValue(object component) => true;
    }
}
