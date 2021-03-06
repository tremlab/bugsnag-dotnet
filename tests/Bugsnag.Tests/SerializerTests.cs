using System;
using System.Collections.Generic;
using Bugsnag.Payload;
using Xunit;

namespace Bugsnag.Tests
{
  public class SerializerTests
  {
    [Fact]
    public void CanSerialiseReport()
    {
      System.Exception exception = null;
      var configuration = new Configuration("123456");

      try
      {
        throw new System.Exception("test");
      }
      catch (System.Exception caughtException)
      {
        exception = caughtException;
      }

      var report = new Report(configuration, exception, Bugsnag.Payload.HandledState.ForHandledException(), new Breadcrumb[] { new Breadcrumb("test", BreadcrumbType.Manual) }, new Session());

      var json = Serializer.SerializeObject(report);
      Assert.NotNull(json);
    }

    [Fact]
    public void CircularReferenceTest()
    {
      var primary = new Dictionary<string, object>();
      var secondary = new Dictionary<string, object>() { { "primary", primary } };
      primary["secondary"] = secondary;
      var json = Serializer.SerializeObject(primary);
      Assert.Contains("[Circular]", json);
    }

    [Fact]
    public void OtherCircularReferenceTest()
    {
      var inner = new Circular { Name = "inner" };
      var outer = new Circular { Name = "outer", Inner = inner };
      inner.Inner = outer;

      var json = Serializer.SerializeObject(outer);

      Assert.Contains("[Circular]", json);
    }

    [Fact]
    public void NullValueTest()
    {
      var o = new Dictionary<string, object> { { "test", null } };
      var json = Serializer.SerializeObject(o);
      Assert.NotNull(json);
    }

    private class Circular
    {
      public string Name { get; set; }

      public Circular Inner { get; set; }
    }
  }
}
