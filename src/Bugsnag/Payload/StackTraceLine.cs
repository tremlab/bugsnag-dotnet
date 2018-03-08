using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bugsnag.Payload
{
  /// <summary>
  /// Represents a set of Bugsnag payload stacktrace lines that are generated from a single StackTrace provided
  /// by the runtime.
  /// </summary>
  public class StackTrace : IEnumerable<StackTraceLine>
  {
    private readonly System.Exception _originalException;

    public StackTrace(System.Exception exception)
    {
      _originalException = exception;
    }

    public IEnumerator<StackTraceLine> GetEnumerator()
    {
      if (_originalException == null)
      {
        yield break;
      }

      var needFileInfo = true;
      var stackTrace = new System.Diagnostics.StackTrace(_originalException, needFileInfo);
      var stackFrames = stackTrace.GetFrames();

      if (stackFrames == null)
      {
        yield break;
      }

      foreach (var frame in stackFrames)
      {
        var stackFrame = StackTraceLine.FromStackFrame(frame);

        yield return stackFrame;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  /// <summary>
  /// Represents an individual stack trace line in the Bugsnag payload.
  /// </summary>
  public class StackTraceLine : Dictionary<string, object>
  {
    public static StackTraceLine FromStackFrame(StackFrame stackFrame)
    {
      var method = stackFrame.GetMethod();
      var file = stackFrame.GetFileName();
      var lineNumber = stackFrame.GetFileLineNumber();
      var methodName = new Method(method).DisplayName();
      var inProject = false;
      var code = new Code(stackFrame, 5).Display();

      return new StackTraceLine(file, lineNumber, methodName, inProject, code);
    }

    public StackTraceLine(string file, int lineNumber, string methodName, bool inProject, Dictionary<string, string> code)
    {
      this.AddToPayload("file", file);
      this.AddToPayload("lineNumber", lineNumber);
      this.AddToPayload("method", methodName);
      this.AddToPayload("inProject", inProject);
      this.AddToPayload("code", code);
    }

    public string FileName
    {
      get
      {
        return this.Get("file") as string;
      }
      set
      {
        this.AddToPayload("file", value);
      }
    }

    public string MethodName
    {
      get
      {
        return this.Get("method") as string;
      }
      set
      {
        this.AddToPayload("method", value);
      }
    }

    public bool InProject
    {
      get
      {
        return (bool)this.Get("inProject");
      }
      set
      {
        this.AddToPayload("inProject", value);
      }
    }
  }
}
