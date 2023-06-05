using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model ;

public class CellArray : List<DeviceTreeValue>, IDeviceTreeItem {
    public CellArray(IEnumerable<DeviceTreeValue> items) : base(items) {
    }
}
