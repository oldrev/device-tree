using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;


public enum DevicePropertyValueType : byte {
    Empty = 0,
    Integer,
    ByteString,
    String,
    StringList,
    CellArray,
    Reference,
}

public readonly struct DeviceTreeValue : IDeviceTreeItem {

    public DevicePropertyValueType ValueType { get; }

    private readonly object _refValue { get; }
    private readonly long _longValue { get; }

    public IReadOnlyList<byte> ByteStringValue {
        get {
            if (this.ValueType != DevicePropertyValueType.ByteString) {
                throw new InvalidCastException();
            }
            var bytes = (IReadOnlyList<byte>)_refValue;
            return bytes;
        }
    }

    public long IntegerValue {
        get {
            if (this.ValueType != DevicePropertyValueType.Integer) {
                throw new InvalidCastException();
            }
            return _longValue;
        }
    }

    public string StringValue {
        get {
            if (this.ValueType != DevicePropertyValueType.String) {
                throw new InvalidCastException();
            }
            var str = (string)_refValue;
            return str;
        }
    }

    public CellArray CellArrayValue {
        get {
            if (this.ValueType != DevicePropertyValueType.CellArray) {
                throw new InvalidCastException();
            }
            var ca = (CellArray)_refValue;
            return ca;
        }
    }

    public DeviceTreeValue(long integer) {
        this.ValueType = DevicePropertyValueType.Integer;
        _longValue = integer;
        _refValue = null!;
    }

    public DeviceTreeValue(int integer) {
        this.ValueType = DevicePropertyValueType.Integer;
        _longValue = integer;
        _refValue = null!;
    }

    public DeviceTreeValue(string str) {
        this.ValueType = DevicePropertyValueType.String;
        _longValue = 0;
        _refValue = str;
    }

    public DeviceTreeValue(IEnumerable<string> strs) {
        this.ValueType = DevicePropertyValueType.StringList;
        _longValue = 0;
        _refValue = strs;
    }

    public DeviceTreeValue(ReadOnlySpan<char> chars) {
        this.ValueType = DevicePropertyValueType.String;
        _longValue = 0;
        _refValue = chars.ToArray();
    }

    public DeviceTreeValue(IReadOnlyList<byte> bytes) {
        this.ValueType = DevicePropertyValueType.ByteString;
        _longValue = 0;
        _refValue = bytes;
    }

    public DeviceTreeValue(ReadOnlySpan<byte> bytes) {
        this.ValueType = DevicePropertyValueType.ByteString;
        _longValue = 0;
        _refValue = bytes.ToArray();
    }

    public DeviceTreeValue(CellArray value) {
        _refValue = value;
        this.ValueType = DevicePropertyValueType.CellArray;
        _longValue = 0;
    }

    public DeviceTreeValue(object refValue, DevicePropertyValueType type) {
        _refValue = refValue;
        this.ValueType = type;
        _longValue = 0;
    }

    public DeviceTreeValue(long longValue, object refValue, DevicePropertyValueType type) {
        _longValue = longValue;
        _refValue = refValue;
        this.ValueType = type;
    }

    public static readonly DeviceTreeValue Empty = new DeviceTreeValue(0, null!, DevicePropertyValueType.Empty);

}
