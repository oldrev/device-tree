using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public class DeviceNode : IDeviceTreeItem {

    public string Name { get; }

    public bool IsRoot => this.Name == "/"; 

    private readonly Dictionary<string, DeviceProperty> _properties;
    public IReadOnlyDictionary<string, DeviceProperty> Properties => _properties;

    public IReadOnlyList<DeviceNode> Children { get; }

    public ulong UnitAddress { get; }

    public DeviceNode(string name, IEnumerable<DeviceProperty> properties, IReadOnlyList<DeviceNode> children, ulong unitAddress = 0) {
        this.Name = name;
        _properties = new Dictionary<string, DeviceProperty>(properties.Count());
        foreach (var p in properties) {
            _properties.Add(p.Name, p);
        }

        this.Children = children;
        this.UnitAddress = unitAddress;
    }

    public override string ToString()
        => $"{{{string.Join(",", Properties.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}}}";
}

