using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public class DeviceNode : IDeviceTreeElement {

    public string Name { get; }

    private readonly Dictionary<string, DeviceProperty> _properties;
    public IReadOnlyDictionary<string, DeviceProperty> Properties => _properties;

    public IReadOnlyList<DeviceNode> Children { get; }

    public DeviceNode(string name, IEnumerable<DeviceProperty> properties, IReadOnlyList<DeviceNode> children) {
        this.Name = name;
        _properties = new Dictionary<string, DeviceProperty>(properties.Count());
        foreach (var p in properties) {
            _properties.Add(p.Name, p);
        }

        this.Children = children;
    }

    public override string ToString()
        => $"{{{string.Join(",", Properties.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}}}";
}

