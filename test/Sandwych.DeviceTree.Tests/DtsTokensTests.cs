using System;
using System.Globalization;
using Parlot.Fluent;
using Xunit;

namespace Sandwych.DeviceTree.Tests;

public class DtsTokensTests {


    //  [Theory]
    //    [InlineData(Dts1)]
    //   [InlineData(Dts2)]
    [Fact]
    public void ShouldParseHexValue() {

        var src = " 0xFFFF ";
        var result = DeviceTreeParser.HexIntegerTerm.Parse(src);
        Assert.Equal(0xFFFF, result);

        src = " 0x19CA ";
        result = DeviceTreeParser.HexIntegerTerm.Parse(src);
        Assert.Equal(0x19CA, result);
    }

    [Fact]
    public void ShouldParseDecimalValue() {

        var src = " 9933142 ";
        var result = DeviceTreeParser.DecimalIntegerTerm.Parse(src);
        Assert.Equal(9933142, result);
    }

    [Fact]
    public void ShouldParseIntegerTermValue() {

        var src = " 22312 ";
        var result = DeviceTreeParser.IntegerTerm.Parse(src);
        Assert.Equal(22312, result);

        src = " 0x0093fd ";
        result = DeviceTreeParser.IntegerTerm.Parse(src);
        Assert.Equal(0x0093fd, result);
    }


    //[Theory]
    //[InlineData("{\"property\":\"value\"}")]
    //[InlineData("{\"property\":[\"value\",\"value\",\"value\"]}")]
    //[InlineData("{\"property\":{\"property\":\"value\"}}")]
    //public void ShouldParseJsonCompiled (string json)
    //{
    //    var _compiled = CompileTests.Compile(JsonParser.Json);

    //    var scanner = new Scanner(json);
    //    var context = new ParseContext(scanner);

    //    var result = _compiled(context);
    //    Assert.Equal(json, result.ToString());
    //}
}

