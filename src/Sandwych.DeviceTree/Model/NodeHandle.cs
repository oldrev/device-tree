using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

internal sealed class NodeHandle : IDeviceTreeItem {
    public string Name { get; }
    public ulong UnitAddress { get; }

    public NodeHandle(string name, ulong unitAddress = 0) {
        this.Name = name;
        this.UnitAddress = unitAddress;
    }

    public override string ToString() => $"{this.Name}@{this.UnitAddress}";
}
