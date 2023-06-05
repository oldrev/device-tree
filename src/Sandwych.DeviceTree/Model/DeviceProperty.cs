using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public sealed class DeviceProperty : IDeviceTreeElement {
    public string Name { get; }
    public DevicePropertyValue Value { get; }
    public bool IsEmpty => Value.ValueType == DevicePropertyValueType.Empty;

    public DeviceProperty(string name, DevicePropertyValue value) {
        this.Name = name;
        this.Value = value;
    }

    public DeviceProperty(string name) {
        this.Name = name;
        this.Value = DevicePropertyValue.Empty;
    }

}

