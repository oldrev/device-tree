using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandwych.DeviceTree.Model;
using Xunit;

namespace Sandwych.DeviceTree.Tests;

public class DeviceTreeElementTests {

    [Fact]
    public void TestGenericDirective1() {
        var dts = "/dts-v1/;";
        var elem = DeviceTreeParser.DirectiveTerm.Parse(dts);
        var directive = elem as GenericDirective;
        Assert.NotNull(directive);
        Assert.Equal("dts-v1", directive.Directive);
    }

    [Fact]
    public void TestIncludeDirective() {
        var dts = "/include/ \"bar/foo.dtsi\"";
        var elem = DeviceTreeParser.IncludeDirectiveTerm.Parse(dts);
        var directive = elem as IncludeDirective;
        Assert.NotNull(directive);
        Assert.Equal("bar/foo.dtsi", directive.Path);
    }

    [Fact]
    public void TestCellArray() {
        var dts = "< 1 2   3 >";
        var pa = DeviceTreeParser.CellArray.Parse(dts);
        var ca = pa.CellArrayValue;
        Assert.NotNull(ca);
        Assert.Equal(3, ca.Count);
        Assert.Equal(1, ca[0].IntegerValue);
        Assert.Equal(2, ca[1].IntegerValue);
        Assert.Equal(3, ca[2].IntegerValue);
    }

    [Fact]
    public void TestPropertyAssignment1() {
        var dts = "my-property;";
        var pa = DeviceTreeParser.PropertyAssignment.Parse(dts) as DeviceProperty;
        Assert.NotNull(pa);
        Assert.True(pa.IsEmpty);
    }

    [Fact]
    public void TestIntegerPropertyAssignment() {
        var dts = "reg = <0x12345678>;";
        var result = DeviceTreeParser.PropertyAssignment.Parse(dts);
        var pa = result as DeviceProperty;
        Assert.NotNull(pa);
        Assert.Equal(0x12345678, pa.Value.CellArrayValue.First().IntegerValue);
    }

    [Fact]
    public void TestCellArrayPropertyAssignmentValues() {
        var dts = "reg = <1  2  3>;";
        var result = DeviceTreeParser.PropertyAssignment.Parse(dts);
        var pa = result as DeviceProperty;
        Assert.NotNull(pa);
        Assert.Equal(3, pa.Value.CellArrayValue.Count);
        Assert.Equal(1, pa.Value.CellArrayValue[0].IntegerValue);
        Assert.Equal(2, pa.Value.CellArrayValue[1].IntegerValue);
        Assert.Equal(3, pa.Value.CellArrayValue[2].IntegerValue);
    }

    [Fact]
    public void CanParseByteArrayPropertyAssignmentStyle1() {
        var dts = "mac_address = [11 22 33 44 55 66];";
        var result = DeviceTreeParser.PropertyAssignment.Parse(dts);
        var pa = result as DeviceProperty;
        Assert.NotNull(pa);
        Assert.Equal(DevicePropertyValueType.ByteString, pa.Value.ValueType);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }, pa.Value.ByteStringValue);
    }

    [Fact]
    public void CanParseByteArrayPropertyAssignmentStyle2() {
        var dts = "local-mac-address = [000012345678];";
        var result = DeviceTreeParser.PropertyAssignment.Parse(dts);
        var pa = result as DeviceProperty;
        Assert.NotNull(pa);
        Assert.Equal(DevicePropertyValueType.ByteString, pa.Value.ValueType);
        Assert.Equal(new byte[] { 0x00, 0x00, 0x12, 0x34, 0x56, 0x78 }, pa.Value.ByteStringValue);
    }

}
